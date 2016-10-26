using System;
using kCura.EventHandler;
using System.Globalization;
using KCD_1041539.ImagingSetScheduler.Helper;

namespace KCD_1041539.ImagingSetScheduler.EventHandlers
{
	[System.Runtime.InteropServices.Guid("B8397947-C222-4D91-8962-A27F8EC16DAD")]
	[kCura.EventHandler.CustomAttributes.Description("Validates the format of the time field on the imaging set scheduler when it's saved.")]
	class ValidateTimeFormat : PreSaveEventHandler
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
				string originalDate = (string)ActiveArtifact.Fields[Constant.Guids.Field.ImagingSetScheduler.TIME.ToString()].Value.Value;
				DateTime time;
				bool valid = DateTime.TryParseExact(originalDate,
													"HH:mm",
													CultureInfo.InvariantCulture,
													DateTimeStyles.None,
													out time);

				if (valid == false)
				{
					response.Message = Constant.Messages.MESSAGE_TIME_FORMAT;
					response.Success = false;
				}

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
					new Field(Constant.Guids.Field.ImagingSetScheduler.TIME)
				};
				return retVal;
			}
		}
	}
}
