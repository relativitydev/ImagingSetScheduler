using System;
using kCura.EventHandler;
using KCD_1041539.ImagingSetScheduler.Helper;

namespace KCD_1041539.ImagingSetScheduler.EventHandlers
{
	[System.Runtime.InteropServices.Guid("AEDBF1C4-35CB-4160-BBE9-1FBF28284F0A")]
	[kCura.EventHandler.CustomAttributes.Description("Sets the next run date.")]
	class SetNextRunDate : PreSaveEventHandler
	{
		public override Response Execute()
		{
			Response response = new Response
			{
				Success = true,
				Message = string.Empty
			};

			try
			{
				var imagingSetScheduler = new Objects.ImagingSetScheduler(ActiveArtifact, ActiveUser.ArtifactID);

				imagingSetScheduler.GetNextRunDate(imagingSetScheduler.FrequencyList, DateTime.Now, imagingSetScheduler.Time);
				ActiveArtifact.Fields[Constant.Guids.Field.ImagingSetScheduler.NEXT_RUN.ToString()].Value.Value = imagingSetScheduler.NextRunDate;
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
			get
			{
				FieldCollection retVal = new FieldCollection
				{
					new Field(Constant.Guids.Field.ImagingSetScheduler.NEXT_RUN)
				};
				return retVal;
			}
		}
	}
}
