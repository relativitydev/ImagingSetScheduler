using System;
using kCura.EventHandler;
using KCD_1041539.ImagingSetScheduler.Helper;

namespace KCD_1041539.ImagingSetScheduler.EventHandlers
{
	[System.Runtime.InteropServices.Guid("FEC16364-DD3C-42CA-A82A-44A37D0DF032")]
	[kCura.EventHandler.CustomAttributes.Description("Removes record from the queue upon deletion.")]
	class RemoveFromQueue : PreDeleteEventHandler
	{
		public override Response Execute()
		{
			var response = new Response
			{
				Message = String.Empty,
				Success = true
			};

			try
			{
				Database.SqlQueryHelper.RemoveRecordFromQueue(Helper.GetDBContext(-1), Constant.Tables.IMAGING_SET_SCHEDULER_QUEUE, ActiveArtifact.ArtifactID, Helper.GetActiveCaseID());
			}
			catch (Exception ex)
			{
				response.Message = ex.ToString();
				response.Success = false;
			}

			return response;
		}

		public override FieldCollection RequiredFields
		{
			get { return new FieldCollection(); }
		}

		public override void Commit()
		{

		}

		public override void Rollback()
		{

		}
	}
}
