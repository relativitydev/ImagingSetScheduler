using kCura.Relativity.Client;
using Relativity.API;
using DTOs = kCura.Relativity.Client.DTOs;

namespace KCD_1041539.ImagingSetScheduler.Interfaces
{
	public interface IValidator
	{
		void ValidateImagingSet(DTOs.RDO imagingSetRdo);

		void ValidateImagingSetScheduler(DTOs.RDO imagingSetSchedulerRdo);

		bool VerifyRelativityVersionForImagingSetExternalApi(IDBContext eddsDbContext);

		bool VerifyImagingApplicationVersionForImagingSetExternalApi(IDBContext workspaceDbContext);

		bool VerifyIfImagingSetIsCurrentlyRunning(Objects.ImagingSet imagingSet);

		bool DoesWorkspaceExists(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId);
	}
}
