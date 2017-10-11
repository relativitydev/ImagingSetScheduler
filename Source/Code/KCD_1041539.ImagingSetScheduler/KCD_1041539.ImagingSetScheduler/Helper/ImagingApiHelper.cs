using Relativity.API;
using Relativity.Imaging.Services.Interfaces;
using System;

namespace KCD_1041539.ImagingSetScheduler.Helper
{
	public class ImagingApiHelper
	{
		//TODO#Bill: Make these Async?
		// Could make this a proper repository, but probably not worth the effort unless it's decided to do a major re-write.

		/// <summary>
		/// Retrieve a <see cref="ImagingSet"/> with the given parameters
		/// </summary>
		/// <param name="svcMgr">the <see cref="IServicesMgr"/>dependency for api access</param>
		/// <param name="identity">the <see cref="ExecutionIdentity"/>to create proxies with</param>
		/// <param name="workspaceArtifactId">the artifactId of the workspace the set is located in</param
		/// <param name="imagingSetArtifactId">artifact Id of the ImagingSet to retrieve</param>
		/// <returns>a <see cref="ImagingSet"/></returns>
		public static ImagingSet RetrieveSingleImagingSet(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, int imagingSetArtifactId)
		{
			try
			{
				using (IImagingSetManager imagingSetManager = svcMgr.CreateProxy<IImagingSetManager>(identity))
				{
					return imagingSetManager.ReadAsync(imagingSetArtifactId, workspaceArtifactId).GetAwaiter().GetResult();
				}
			}
			catch (Exception ex)
			{
				var errorContext = String.Format("An error occured when retrieving Imaging Set [WorkspaceArtifactId: {0}, ImagingSetArtifactId: {1}]",
					workspaceArtifactId,
					imagingSetArtifactId);
				throw new CustomExceptions.ImagingSetSchedulerException(errorContext, ex);
			}
		}

		//TODO#Bill: Possible to use the lastEnabledOnQC field here if we get confirmation for how it's supposed to behave
		/// <summary>
		/// Run an imaging set with the given parameters
		/// </summary>
		/// <param name="imagingSet">the <see cref="ImagingSet"/>to execute</param>
		/// <param name="svcMgr">the <see cref="IServicesMgr"/>dependency for api access</param>
		/// <param name="identity">the <see cref="ExecutionIdentity"/>to create proxies with</param>
		/// <param name="workspaceArtifactId">the artifactId of the workspace the set is located in</param>
		/// <param name="lockImagesForQc">if true, will hide images for QC</param>
		/// <param name="userSubmissionId">the artifactId for the user who created the ImagingSetScheduler RDO being executed</param>
		public static void RunImagingSet(ImagingSet imagingSet, IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, bool lockImagesForQc, int userSubmissionId)
		{
			try
			{
				if (lockImagesForQc)
				{
					using (IImagingSetManager imagingSetManager = svcMgr.CreateProxy<IImagingSetManager>(identity))
					{
						imagingSetManager.HideImagingSetAsync(new ImagingSetRef { ArtifactID = imagingSet.ArtifactID, Name = imagingSet.Name }, workspaceArtifactId).GetAwaiter().GetResult();
					}
				}
				else
				{
					using (IImagingSetManager imagingSetManager = svcMgr.CreateProxy<IImagingSetManager>(identity))
					{
						imagingSetManager.ReleaseImagingSetAsync(new ImagingSetRef { ArtifactID = imagingSet.ArtifactID, Name = imagingSet.Name }, workspaceArtifactId).GetAwaiter().GetResult();
					}
				}
				using (IImagingJobManager imagingJobManger = svcMgr.CreateProxy<IImagingJobManager>(identity))
				{
					imagingJobManger.RunImagingSetAsync(new ImagingJob { ImagingSetId = imagingSet.ArtifactID, WorkspaceId = workspaceArtifactId }).GetAwaiter().GetResult();
				}
			}
			catch (Exception ex)
			{
				string errorContext = String.Format(
					"{0} [WorkspaceArtifactId={1}], ImagingSetArtifactId={2}, UserArtifactId={3}]",
					Constant.ErrorMessages.IMAGING_SET_FAIL,
					workspaceArtifactId,
					imagingSet.ArtifactID,
					userSubmissionId);

				throw new CustomExceptions.ImagingSetSchedulerException($"{errorContext}", ex);
			}
		}
	}
}
