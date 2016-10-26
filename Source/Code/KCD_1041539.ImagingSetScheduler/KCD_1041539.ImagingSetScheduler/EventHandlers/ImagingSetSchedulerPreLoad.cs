using System;
using System.Text;
using kCura.EventHandler;
using KCD_1041539.ImagingSetScheduler.Helper;
using System.Data;
using Relativity.API;

namespace KCD_1041539.ImagingSetScheduler.EventHandlers
{
	[System.Runtime.InteropServices.Guid("4DB80950-F0B8-4166-AC23-4FEECDFD2350")]
	[kCura.EventHandler.CustomAttributes.Description("Loads the current Imaging Set statuses for the associated Imaging Set Scheduler object.")]
	class ImagingSetSchedulerPreLoad : PreLoadEventHandler
	{
		public override Response Execute()
		{
			if (ActiveArtifact.IsNew)
			{
				ActiveArtifact.Fields[Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_STATUS.ToString()].Value.Value = String.Empty;
				ActiveArtifact.Fields[Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_IMAGE_QC_REVIEW_STATUS.ToString()].Value.Value = String.Empty;
				ActiveArtifact.Fields[Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_IMAGE_COMPLETION.ToString()].Value.Value = String.Empty;
				ActiveArtifact.Fields[Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_LAST_RUN_ERROR.ToString()].Value.Value = String.Empty;
				return new Response();
			}
			else
			{
				if (ActiveArtifact.Fields["Imaging Set"].Value.Value != null)
				{
					return ExecuteEventHandler(Helper.GetDBContext(-1), Helper.GetDBContext(Application.ArtifactID), Application.ArtifactID, (Int32)ActiveArtifact.Fields["Imaging Set"].Value.Value);
				}
				else
				{
					return new Response();
				}
			}
		}

		public Response ExecuteEventHandler(IDBContext masterDbContext, IDBContext caseDbContext, Int32 appArtifactId, Int32 imagingSetArtifactId)
		{
			var response = new Response()
			{
				Success = true,
				Message = String.Empty
			};

			try
			{
				string imagingSetStatus = ImagingSetHandlerQueries.GetStatusInstanced(caseDbContext, imagingSetArtifactId);
				string status = imagingSetStatus.ToLower();

				if ((ActiveArtifact.IsNew))
				{
					ActiveArtifact.Fields[Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_STATUS.ToString()].Value.Value = String.Empty;
					ActiveArtifact.Fields[Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_IMAGE_QC_REVIEW_STATUS.ToString()].Value.Value = String.Empty;
					ActiveArtifact.Fields[Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_IMAGE_COMPLETION.ToString()].Value.Value = String.Empty;
					ActiveArtifact.Fields[Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_LAST_RUN_ERROR.ToString()].Value.Value = String.Empty;
				}
				else
				{
					//Imaging Set: Image Completion
					if (status.Equals(Constant.ImagingSetStatus.PROCESSING_BUILDING_TABLES)
						|| status.Equals(Constant.ImagingSetStatus.STAGING)
						|| status.Equals(Constant.ImagingSetStatus.WAITING)
						|| status.Equals(Constant.ImagingSetStatus.STOPPING)
						|| status.Equals(Constant.ImagingSetStatus.STOPPED_BY_USER)
						|| status.Equals(string.Empty)
						|| status.Equals(Constant.ImagingSetStatus.INITIALIZING))
					{
						ActiveArtifact.Fields[Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_IMAGE_COMPLETION.ToString()].Value.Value = "";
					}
					else
					{
						ImagingSetStatusDTO setStatus = ImagingSetHandlerQueries.GetImageCompletion(caseDbContext, imagingSetArtifactId);

						string imagesSkippedString = string.Format("{0} {1} skipped.<br>",
							setStatus.DocumentsSkipped,
							ImagingSetHandlerQueries.GetPluralOrSingular(setStatus.DocumentsSkipped));

						string imagesRemainingString = string.Format("{0} {1} remaining. <br>",
							setStatus.DocumentsWaiting,
							ImagingSetHandlerQueries.GetPluralOrSingular(setStatus.DocumentsWaiting));

						string imagesCompletedString = string.Format("{0} {1} successfully imaged. <br>",
							setStatus.DocumentsDone,
							ImagingSetHandlerQueries.GetPluralOrSingular(setStatus.DocumentsWaiting));

						var imagesErrorString = string.Format(
							setStatus.DocumentsErrored > 0
								? "<span style=\"color: red;\"> {0} {1} with errors. <br> </span>"
								: "{0} {1} with errors. <br>", setStatus.DocumentsErrored, ImagingSetHandlerQueries.GetPluralOrSingular(setStatus.DocumentsErrored)
							);

						var sbr = new StringBuilder();
						sbr.Append(imagesRemainingString);
						sbr.Append(imagesCompletedString);
						sbr.Append(imagesErrorString);
						sbr.Append(imagesSkippedString);

						string imagingSetImagingCompletion = sbr.ToString();
						ActiveArtifact.Fields[Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_IMAGE_COMPLETION.ToString()].Value.Value = imagingSetImagingCompletion;
					}

					//Imaging Set: Image QC Review Status
					if (status.Equals(Constant.ImagingSetStatus.PROCESSING_BUILDING_TABLES)
						|| status.Equals(Constant.ImagingSetStatus.STAGING)
						|| status.Equals(Constant.ImagingSetStatus.WAITING)
						|| status.Equals(Constant.ImagingSetStatus.STOPPING)
						|| status.Equals(Constant.ImagingSetStatus.STOPPED_BY_USER)
						|| status.Equals(string.Empty)
						|| status.Equals(Constant.ImagingSetStatus.INITIALIZING))
					{
						ActiveArtifact.Fields[Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_IMAGE_QC_REVIEW_STATUS.ToString()].Value.Value = "";
					}
					else
					{
						DataSet setQcStatus = ImagingSetHandlerQueries.GetQcCounts(Helper.GetDBContext(appArtifactId), imagingSetArtifactId);

						string qcHiddenString = string.Format("{0} {1} set to hidden.<br>",
							Convert.ToString(setQcStatus.Tables[0].Rows[0]["HoldCount"]),
							ImagingSetHandlerQueries.GetPluralOrSingular(Convert.ToInt32(setQcStatus.Tables[0].Rows[0]["HoldCount"])));

						string qcReleasedString = string.Format("{0} {1} viewable.<br>",
							Convert.ToString(setQcStatus.Tables[0].Rows[0]["ReleaseCount"]),
							ImagingSetHandlerQueries.GetPluralOrSingular(Convert.ToInt32(setQcStatus.Tables[0].Rows[0]["ReleaseCount"])));

						var stbr = new StringBuilder();
						stbr.Append(qcHiddenString);
						stbr.Append(qcReleasedString);

						ActiveArtifact.Fields[Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_IMAGE_QC_REVIEW_STATUS.ToString()].Value.Value = stbr.ToString();
					}

					//Imaging Set: Status
					ActiveArtifact.Fields[Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_STATUS.ToString()].Value.Value = imagingSetStatus;

					//Imaging Set: Last Run Error
					string imagingSetLastRunError = ImagingSetHandlerQueries.GetImagingSetLastRunError(caseDbContext, imagingSetArtifactId);
					ActiveArtifact.Fields[Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_LAST_RUN_ERROR.ToString()].Value.Value = imagingSetLastRunError;
				}

				return response;
			}
			catch (Exception ex)
			{
				response.Message = ex.ToString();
				response.Success = false;
			}
			return response;
		}

		public override FieldCollection RequiredFields
		{
			get
			{
				var retVal = new FieldCollection
				{
					new Field(Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_STATUS), 
					new Field(Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_IMAGE_COMPLETION), 
					new Field(Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_IMAGE_QC_REVIEW_STATUS), 
					new Field(Constant.Guids.Field.ImagingSetScheduler.IMAGINGSET_LAST_RUN_ERROR)
				};
				return retVal;
			}
		}
	}
}
