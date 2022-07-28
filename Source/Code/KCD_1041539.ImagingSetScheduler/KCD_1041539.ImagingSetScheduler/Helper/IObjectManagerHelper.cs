using System.Collections.Generic;
using System.Threading.Tasks;
using KCD_1041539.ImagingSetScheduler.Context;
using Relativity.Services.Objects.DataContracts;

namespace KCD_1041539.ImagingSetScheduler.Helper
{
    public interface IObjectManagerHelper
    {
        /// <summary>
        /// Retrieves a single Imaging Set Schedule in a workspace.
        /// </summary>
        /// <param name="workspaceId">The Artifact ID of the workspace.</param>
        /// <param name="contextContainer">The context container.</param>
        /// /// <param name="imagingSetSchedulerId">The Artifact ID of the desired Imaging Set Schedule.</param>
        /// <returns>An Imaging Set Schedule objects with Guid fields.</returns>
        Task<RelativityObject> RetrieveSingleImagingSetScheduler(int workspaceId, IContextContainer contextContainer, int imagingSetSchedulerId);

        /// <summary>
        /// Retrieves Imaging Set Schedules whose status is not "Waiting" in a workspace.
        /// </summary>
        /// <param name="workspaceId">The Artifact ID of the workspace.</param>
        /// <param name="contextContainer">The context container.</param>
        /// <returns>A list of Imaging Set Schedules objects with Guid fields.</returns>
        Task<List<RelativityObject>> RetrieveAllImagingSetSchedulesNotWaitingAsync(int workspaceId, IContextContainer contextContainer);
    }
}
