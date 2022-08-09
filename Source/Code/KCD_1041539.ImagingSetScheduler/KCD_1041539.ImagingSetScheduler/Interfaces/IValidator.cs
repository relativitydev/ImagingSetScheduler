using Relativity.API;
using Relativity.Imaging.Services.Interfaces;

namespace KCD_1041539.ImagingSetScheduler.Interfaces
{
	public interface IValidator
	{
		bool VerifyRelativityVersionForImagingSetExternalApi(IDBContext eddsDbContext);

		bool VerifyImagingApplicationVersionForImagingSetExternalApi(IDBContext workspaceDbContext);

		bool VerifyIfImagingSetIsCurrentlyRunning(ImagingSet imagingSet);
	}
}
