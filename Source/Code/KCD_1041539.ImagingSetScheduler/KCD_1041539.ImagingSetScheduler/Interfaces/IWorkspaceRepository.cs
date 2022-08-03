using System.Threading.Tasks;
using KCD_1041539.ImagingSetScheduler.Context;

namespace KCD_1041539.ImagingSetScheduler.Interfaces
{
	public interface IWorkspaceRepository
	{
		/// <summary>
		/// Determines if a workspace exists.
		/// </summary>
		/// <param name="workspaceArtifactId">The Artifact ID of the workspace.</param>
		/// <param name="contextContainer">The context container.</param>
		/// <returns>A boolean, true if the workspace exists, false otherwise.</returns>
		Task<bool> DoesWorkspaceExists(int workspaceArtifactId, IContextContainer contextContainer);
	}
}
