using System;
using System.Threading.Tasks;
using KCD_1041539.ImagingSetScheduler.Context;
using KCD_1041539.ImagingSetScheduler.Interfaces;
using Relativity.Services.Exceptions;
using Relativity.Services.Interfaces.Workspace;

namespace KCD_1041539.ImagingSetScheduler.Helper
{
	public class WorkspaceRepository : IWorkspaceRepository
	{
		public async Task<bool> DoesWorkspaceExists(int workspaceArtifactId, IContextContainer contextContainer)
		{
			if (workspaceArtifactId < 1)
			{
				throw new ArgumentException(Constant.ErrorMessages.WORKSPACE_ARTIFACT_ID_CANNOT_BE_NEGATIVE);
			}

			try
			{
				using (IWorkspaceManager workspaceManager =
				       contextContainer.ServicesProxyFactory.CreateServiceProxy<IWorkspaceManager>())
				{
					var res = await workspaceManager.ReadAsync(workspaceArtifactId, false, false).ConfigureAwait(false);
					return true;
				}
			}
			catch (NotFoundException)
			{
				return false;
			}
		}
	}
}
