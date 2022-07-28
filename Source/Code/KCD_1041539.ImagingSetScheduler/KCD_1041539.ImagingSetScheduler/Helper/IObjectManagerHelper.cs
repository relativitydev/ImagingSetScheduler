using System;
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
        /// <param name="workspaceArtifactId">The Artifact ID of the workspace.</param>
        /// <param name="contextContainer">The context container.</param>
        /// <param name="imagingSetSchedulerArtifactId">The Artifact ID of the desired Imaging Set Schedule.</param>
        /// <returns>An Imaging Set Schedule objects with Guid fields.</returns>
        Task<RelativityObject> RetrieveSingleImagingSetScheduler(int workspaceArtifactId, IContextContainer contextContainer, int imagingSetSchedulerArtifactId);

        /// <summary>
        /// Retrieves Imaging Set Schedules whose status is not "Waiting" in a workspace.
        /// </summary>
        /// <param name="workspaceArtifactId">The Artifact ID of the workspace.</param>
        /// <param name="contextContainer">The context container.</param>
        /// <returns>A list of Imaging Set Schedules objects with Guid fields.</returns>
        Task<List<RelativityObject>> RetrieveAllImagingSetSchedulesNotWaitingAsync(int workspaceArtifactId, IContextContainer contextContainer);

        /// <summary>
        /// Updates the "Last Run" and "Next Run" fields for an Imaging Set Schedule, also updates "Messages" and "Status" for an Imaging Set Schedule.
        /// </summary>
        /// <param name="workspaceArtifactId">The Artifact ID of the workspace.</param>
        /// <param name="contextContainer">The context container.</param>
        /// <param name="imagingSetSchedulerArtifactId">The Artifact ID of the desired Imaging Set Schedule.</param>
        /// <param name="lastRun">The last run of the Imaging Set Schedule.</param>
        /// <param name="nextRun">The next run of the Imaging Set Schedule.</param>
        /// <param name="message">Description of the update operation.</param>
        /// <param name="status">The new status of the Imaging Set Schedule.</param>
        /// <returns>A count of the objects updated, success status, and a description of the update.</returns>
        Task<MassUpdateResult> UpdateImagingSetScheduler(int workspaceArtifactId, IContextContainer contextContainer, int imagingSetSchedulerArtifactId, DateTime? lastRun, DateTime? nextRun, string message, string status);
    }
}
