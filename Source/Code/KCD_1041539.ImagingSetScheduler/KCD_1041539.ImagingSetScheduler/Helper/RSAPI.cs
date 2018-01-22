using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;
using DTOs = kCura.Relativity.Client.DTOs;
using Relativity.API;
using Relativity.Imaging.Services.Interfaces;

namespace KCD_1041539.ImagingSetScheduler.Helper
{
	public class RSAPI
	{
		public static DTOs.RDO RetrieveSingleImagingSetScheduler(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int imagingSetSchedulerArtifactId)
		{
			if (workspaceArtifactId < 0)
			{
				throw new ArgumentException(Constant.ErrorMessages.WORKSPACE_ARTIFACT_ID_CANNOT_BE_NEGATIVE);
			}

			if (imagingSetSchedulerArtifactId < 0)
			{
				throw new ArgumentException(Constant.ErrorMessages.IMAGING_SET_SCHEDULER_ARTIFACT_ID_CANNOT_BE_NEGATIVE);
			}

			try
			{
				DTOs.RDO retVal;

				using (IRSAPIClient client = svcMgr.CreateProxy<IRSAPIClient>(identity))
				{
					client.APIOptions.WorkspaceID = workspaceArtifactId;

					Response<IEnumerable<DTOs.RDO>> res = RetrieveImagingSetScheule(client, imagingSetSchedulerArtifactId);
					if (res.Success)
					{
						if (res.Results == null || !res.Results.Any())
						{
							var errorContext = String.Format("{2}. [WorkspaceArtifactId: {0}, ImagingSetArtifactId: {1}]", workspaceArtifactId, imagingSetSchedulerArtifactId, Constant.ErrorMessages.IMAGING_SET_SCHEDULER_DOES_NOT_EXIST);
							throw new CustomExceptions.ImagingSetSchedulerException(errorContext);
						}

						retVal = res.Results.First();
					}
					else
					{
						throw new CustomExceptions.ImagingSetSchedulerException(res.Message);
					}
				}

				return retVal;
			}
			catch (Exception ex)
			{
				var errorContext = String.Format("An error occured when retrieving Imaging Set Scheduler [WorkspaceArtifactId: {0}, ImagingSetSchedulerArtifactId: {1}]",
					workspaceArtifactId,
					imagingSetSchedulerArtifactId);
				throw new CustomExceptions.ImagingSetSchedulerException(errorContext, ex);
			}
		}

		public static IEnumerable<DTOs.RDO> RetrieveAllImagingSetSchedulesNotWaiting(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId)
		{
			if (workspaceArtifactId < 0)
			{
				throw new ArgumentException(Constant.ErrorMessages.WORKSPACE_ARTIFACT_ID_CANNOT_BE_NEGATIVE);
			}

			try
			{
				IEnumerable<DTOs.RDO> retVal;

				using (IRSAPIClient client = svcMgr.CreateProxy<IRSAPIClient>(identity))
				{
					client.APIOptions.WorkspaceID = workspaceArtifactId;

					Response<IEnumerable<DTOs.RDO>> res = RetrieveAllImagingSetScheules(client);
					if (res.Success)
					{
						retVal = res.Results;
					}
					else
					{
						throw new CustomExceptions.ImagingSetSchedulerException(res.Message);
					}
				}

				return retVal;
			}
			catch (Exception ex)
			{
				var errorContext = String.Format("An error occured when retrieving all Imaging Set Scheduler's in a workspace. [WorkspaceArtifactId: {0}]",
					workspaceArtifactId);
				throw new CustomExceptions.ImagingSetSchedulerException(errorContext, ex);
			}
		}

		public static Response<IEnumerable<DTOs.RDO>> UpdateImagingSetScheduler(IRSAPIClient proxy, int imagingSetSchedulerArtifactId, DateTime? lastRun, DateTime? nextRun, string messages, string status, int workspaceArtifactId)
		{
			try
			{
				Response<IEnumerable<DTOs.RDO>> retVal;

				proxy.APIOptions.WorkspaceID = workspaceArtifactId;

				string message = "";
				bool success = true;

				DTOs.RDO rdOtoUpdate = new DTOs.RDO(Constant.Guids.ObjectType.IMAGING_SET_SCHEDULER, imagingSetSchedulerArtifactId);
				if (lastRun.HasValue)
				{
					rdOtoUpdate.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.LAST_RUN, lastRun.Value));
				}

				if (nextRun.HasValue)
				{
					rdOtoUpdate.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.NEXT_RUN, nextRun.Value));
				}

				rdOtoUpdate.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.MESSAGES, messages));
				rdOtoUpdate.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.STATUS, status));

				DTOs.ResultSet<DTOs.RDO> theseResults = proxy.Repositories.RDO.Update(rdOtoUpdate);

				message += theseResults.Message;
				if (!theseResults.Success)
				{
					success = false;
				}

				Response<IEnumerable<DTOs.RDO>> res = new Response<IEnumerable<DTOs.RDO>>
				{
					Success = success,
					Message = FormatMessage(theseResults.Results.Select(x => x.Message).ToList(), message, success)
				};
				retVal = res;
				return retVal;
			}
			catch (Exception ex)
			{
				var errorContext = String.Format("An error occured when updating Imaging Set Scheduler [WorkspaceArtifactId: {0}, ImagingSetSchedulerArtifactId: {1}]",
									workspaceArtifactId,
									imagingSetSchedulerArtifactId);
				throw new CustomExceptions.ImagingSetSchedulerException(errorContext, ex);
			}
		}

		public static bool DoesWorkspaceExists(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId)
		{
			if (workspaceArtifactId < 1)
			{
				throw new ArgumentException(Constant.ErrorMessages.WORKSPACE_ARTIFACT_ID_CANNOT_BE_NEGATIVE);
			}

			try
			{
				using (var proxy = svcMgr.CreateProxy<IRSAPIClient>(identity))
				{
					// ReSharper disable once UnusedVariable
					DTOs.Workspace workspaceArtifact = proxy.Repositories.Workspace.ReadSingle(workspaceArtifactId);
					return true;
				}
			}
			catch (Exception)
			{
				//workspace not found
				return false;
			}
		}

		#region Private Methods
		private static Response<IEnumerable<DTOs.RDO>> RetrieveImagingSetScheule(IRSAPIClient proxy, int imagingSetSchedulerArtifactId)
		{
			string message = "", queryToken = "";
			int batchSize, iterator = 0;
			bool success = true;
			DTOs.QueryResultSet<DTOs.RDO> theseResults;
			List<DTOs.Result<DTOs.RDO>> resultList = new List<DTOs.Result<DTOs.RDO>>();
			WholeNumberCondition artifactIDCondition = new WholeNumberCondition("ArtifactId", NumericConditionEnum.EqualTo, imagingSetSchedulerArtifactId);

			DTOs.Query<DTOs.RDO> query = new DTOs.Query<DTOs.RDO>
			{
				ArtifactTypeGuid = Constant.Guids.ObjectType.IMAGING_SET_SCHEDULER,
				Condition = artifactIDCondition
			};
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.FREQUENCY));
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.NAME));
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.IMAGING_SET));
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.TIME));
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.LOCK_IMAGES_FOR_QC));
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.LAST_RUN));
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.NEXT_RUN));
			query.Fields.Add(new DTOs.FieldValue(DTOs.ArtifactFieldNames.SystemCreatedBy));

			theseResults = proxy.Repositories.RDO.Query(query);

			message += theseResults.Message;
			if (!theseResults.Success)
			{
				success = false;
			}

			resultList.AddRange(theseResults.Results);
			if (!String.IsNullOrEmpty(theseResults.QueryToken))
			{
				queryToken = theseResults.QueryToken;
				batchSize = theseResults.Results.Count();
				iterator += batchSize;
				do
				{
					theseResults = proxy.Repositories.RDO.QuerySubset(theseResults.QueryToken, iterator + 1, batchSize);
					resultList.AddRange(theseResults.Results);
					message += theseResults.Message;
					if (!theseResults.Success)
					{
						success = false;
					}
					iterator += batchSize;
				} while (iterator < theseResults.TotalCount);
			}

			Response<IEnumerable<DTOs.RDO>> res = new Response<IEnumerable<DTOs.RDO>>
			{
				Results = resultList.Select(x => x.Artifact),
				Success = success,
				Message = FormatMessage(resultList.Select(x => x.Message).ToList(), message, success)
			};
			return res;
		}

		private static Response<IEnumerable<DTOs.RDO>> RetrieveImagingSet(IRSAPIClient proxy, int imagingSetArtifactId)
		{
			string message = "", queryToken = "";
			int batchSize, iterator = 0;
			bool success = true;
			DTOs.QueryResultSet<DTOs.RDO> theseResults;
			List<DTOs.Result<DTOs.RDO>> resultList = new List<DTOs.Result<DTOs.RDO>>();
			WholeNumberCondition artifactIDCondition = new WholeNumberCondition("ArtifactId", NumericConditionEnum.EqualTo, imagingSetArtifactId);

			DTOs.Query<DTOs.RDO> query = new DTOs.Query<DTOs.RDO>
			{
				ArtifactTypeGuid = Constant.Guids.ObjectType.IMAGING_SET,
				Condition = artifactIDCondition
			};
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSet.NAME));
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSet.DATA_SOURCE));
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSet.IMAGING_PROFILE));
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSet.STATUS));

			theseResults = proxy.Repositories.RDO.Query(query);

			message += theseResults.Message;
			if (!theseResults.Success)
			{
				success = false;
			}

			resultList.AddRange(theseResults.Results);
			if (!String.IsNullOrEmpty(theseResults.QueryToken))
			{
				queryToken = theseResults.QueryToken;
				batchSize = theseResults.Results.Count();
				iterator += batchSize;
				do
				{
					theseResults = proxy.Repositories.RDO.QuerySubset(theseResults.QueryToken, iterator + 1, batchSize);
					resultList.AddRange(theseResults.Results);
					message += theseResults.Message;
					if (!theseResults.Success)
					{
						success = false;
					}
					iterator += batchSize;
				} while (iterator < theseResults.TotalCount);
			}

			Response<IEnumerable<DTOs.RDO>> res = new Response<IEnumerable<DTOs.RDO>>
			{
				Results = resultList.Select(x => x.Artifact),
				Success = success,
				Message = FormatMessage(resultList.Select(x => x.Message).ToList(), message, success)
			};
			return res;
		}

		private static Response<IEnumerable<DTOs.RDO>> RetrieveAllImagingSetScheules(IRSAPIClient proxy)
		{
			string message = "", queryToken = "";
			int batchSize, iterator = 0;
			bool success = true;
			DTOs.QueryResultSet<DTOs.RDO> theseResults;
			List<DTOs.Result<DTOs.RDO>> resultList = new List<DTOs.Result<DTOs.RDO>>();
			TextCondition waitingStatusCondition = new TextCondition(Constant.Guids.Field.ImagingSetScheduler.STATUS, TextConditionEnum.EqualTo, Constant.ImagingSetSchedulerStatus.WAITING);
			NotCondition notCondition = new NotCondition(waitingStatusCondition);

			DTOs.Query<DTOs.RDO> query = new DTOs.Query<DTOs.RDO>
			{
				ArtifactTypeGuid = Constant.Guids.ObjectType.IMAGING_SET_SCHEDULER,
				Condition = notCondition
			};
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.FREQUENCY));
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.NAME));
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.IMAGING_SET));
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.TIME));
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.LOCK_IMAGES_FOR_QC));
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.LAST_RUN));
			query.Fields.Add(new DTOs.FieldValue(Constant.Guids.Field.ImagingSetScheduler.NEXT_RUN));
			query.Fields.Add(new DTOs.FieldValue(DTOs.ArtifactFieldNames.SystemCreatedBy));

			theseResults = proxy.Repositories.RDO.Query(query);

			message += theseResults.Message;
			if (!theseResults.Success)
			{
				success = false;
			}

			resultList.AddRange(theseResults.Results);
			if (!String.IsNullOrEmpty(theseResults.QueryToken))
			{
				queryToken = theseResults.QueryToken;
				batchSize = theseResults.Results.Count();
				iterator += batchSize;
				do
				{
					theseResults = proxy.Repositories.RDO.QuerySubset(theseResults.QueryToken, iterator + 1, batchSize);
					resultList.AddRange(theseResults.Results);
					message += theseResults.Message;
					if (!theseResults.Success)
					{
						success = false;
					}
					iterator += batchSize;
				} while (iterator < theseResults.TotalCount);
			}

			Response<IEnumerable<DTOs.RDO>> res = new Response<IEnumerable<DTOs.RDO>>
			{
				Results = resultList.Select(x => x.Artifact),
				Success = success,
				Message = FormatMessage(resultList.Select(x => x.Message).ToList(), message, success)
			};
			return res;
		}

		private static string FormatMessage(List<String> results, string message, bool success)
		{
			string messageList = "";

			if (!success)
			{
				messageList = message;
				results.ToList().ForEach(w => messageList += (w));

				var x = results.ToList();
			}
			return messageList;
		}

		#endregion
	}
}
