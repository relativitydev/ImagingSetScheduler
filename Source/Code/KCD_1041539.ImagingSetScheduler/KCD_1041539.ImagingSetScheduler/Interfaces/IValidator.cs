using System.Threading.Tasks;
using KCD_1041539.ImagingSetScheduler.Context;
using KCD_1041539.ImagingSetScheduler.Helper;
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

		Task<bool> DoesWorkspaceExists(int workspaceArtifactId, IContextContainer contextContainer, IObjectManagerHelper objectManagerHelper);
	}
}
