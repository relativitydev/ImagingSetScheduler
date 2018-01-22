using kCura.Relativity.Client;
using Relativity.API;
using Relativity.Imaging.Services.Interfaces;
using DTOs = kCura.Relativity.Client.DTOs;

namespace KCD_1041539.ImagingSetScheduler.Interfaces
{
	public interface IValidator
	{
		void ValidateImagingSetScheduler(DTOs.RDO imagingSetSchedulerRdo);

		bool VerifyRelativityVersionForImagingSetExternalApi(IDBContext eddsDbContext);

		bool VerifyImagingApplicationVersionForImagingSetExternalApi(IDBContext workspaceDbContext);

		bool VerifyIfImagingSetIsCurrentlyRunning(ImagingSet imagingSet);

		bool DoesWorkspaceExists(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId);
	}
}
