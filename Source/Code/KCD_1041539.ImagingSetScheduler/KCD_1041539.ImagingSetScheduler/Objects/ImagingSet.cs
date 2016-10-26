using System;
using System.Collections.Generic;
using Relativity.API;
using kCura.Relativity.Client;
using KCD_1041539.ImagingSetScheduler.Helper;
using DTOs = kCura.Relativity.Client.DTOs;
using System.Data.SqlClient;

namespace KCD_1041539.ImagingSetScheduler.Objects
{
	public class ImagingSet
	{
		private string Name { get; set; }
		public int ArtifactId { get; set; }
		public int ImagingProfileArtifactId { get; set; }
		public int SubmittedByUserArtifactId { get; set; }
		public Database.SqlQueryHelper SqlQueryHelper { get; set; }
		public IDBContext EddsDbContext { get; set; }
		public CapiHelper CapiHelper { get; set; }
		public string Status { get; set; }

		public ImagingSet(DTOs.RDO artifact, int imagingSetScheduleCreatedByUserArtifactId, IDBContext eddsDbContext)
		{
			ArtifactId = artifact.ArtifactID;

			DTOs.Artifact imagingProfileField = artifact[Constant.Guids.Field.ImagingSet.IMAGING_PROFILE].ValueAsSingleObject;
			if (imagingProfileField == null)
			{
				throw new CustomExceptions.ImagingSetSchedulerException("Imaging Set - Imaging Profile field is NULL.");
			}
			ImagingProfileArtifactId = imagingProfileField.ArtifactID;

			string nameField = artifact[Constant.Guids.Field.ImagingSet.NAME].ValueAsFixedLengthText;
			if (nameField == null)
			{
				throw new CustomExceptions.ImagingSetSchedulerException("Imaging Set - Name field is NULL.");
			}
			Name = nameField;

			SubmittedByUserArtifactId = imagingSetScheduleCreatedByUserArtifactId;

			string statusField = artifact[Constant.Guids.Field.ImagingSet.STATUS].ValueAsFixedLengthText;
			if (statusField == null)
			{
				throw new CustomExceptions.ImagingSetSchedulerException("Imaging Set - Status field is NULL.");
			}
			Status = statusField;

			SqlQueryHelper = new Database.SqlQueryHelper();
			EddsDbContext = eddsDbContext;
			CapiHelper = new CapiHelper();
		}

		public void Run(string jobType, IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, SqlConnection masterDbConnection, SqlConnection workspaceDbConnection, bool lockImagesForQc)
		{
			//Insert token in token table
			string token = Guid.NewGuid().ToString();
			SqlQueryHelper.InsertImagingJobAuthenticationToken(EddsDbContext, token);

			//Get MasterUserArtifactId if there are any migrated cases. CAPI requires the MasterUserArtifactId for it to run and it doesn't work when WorkspaceUserArtifactId is provided
			Int32 masterUserArtifactId = SqlQueryHelper.GetMasterUserArtifactId(EddsDbContext, workspaceArtifactId, SubmittedByUserArtifactId);
 
			//Submit CAPI call
			Boolean isSubmitSuccess = CapiHelper.SubmitRunImagingSetExternal(workspaceArtifactId, ArtifactId, masterUserArtifactId, token, lockImagesForQc);

			if (!isSubmitSuccess)
			{
				string errorContext = String.Format(
					"{0} [WorkspaceArtifactId={1}], ImagingSetArtifactId={2}, UserArtifactId={3}]",
					Constant.ErrorMessages.IMAGING_SET_FAIL,
					workspaceArtifactId,
					ArtifactId,
					SubmittedByUserArtifactId);

				throw new CustomExceptions.ImagingSetSchedulerException(errorContext);
			}
		}
	}
}
