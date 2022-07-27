using System;
using System.Collections.Generic;
using System.Linq;
using KCD_1041539.ImagingSetScheduler.Helper;
using DTOs = kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client;
using System.Globalization;
using System.Data.SqlClient;
using kCura.EventHandler;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Choice = Relativity.Services.Objects.DataContracts.Choice;
using Field = Relativity.Services.Objects.DataContracts.Field;

namespace KCD_1041539.ImagingSetScheduler.Objects
{
	public class ImagingSetScheduler
	{
		public int ArtifactId { get; set; }
		public string Name { get; set; }
		public int ImagingSetArtifactId { get; set; }
		public List<DayOfWeek> FrequencyList { get; set; }
		public string Time { get; set; }
		public bool LockImagesForQc { get; set; }
		public DateTime? LastRunDate { get; set; }
		public DateTime? NextRunDate { get; set; }
		public int CreatedByUserId { get; set; }
        public ImagingSetScheduler(RelativityObject artifact)
        {
            List<FieldValuePair> fieldValuePairs = artifact.FieldValues;
            Name = (string)fieldValuePairs.Find(x => x.Field.Name == "Name").Value ?? throw new CustomExceptions.ImagingSetSchedulerException("Imaging Set Scheduler - Name field is NULL.");

            ArtifactId = artifact.ArtifactID;

            FieldValuePair imagingSetFieldValuePair = fieldValuePairs.Find(x => x.Field.Name == "Imaging Set");
            if ((RelativityObjectValue)imagingSetFieldValuePair.Value == null)
            {
                throw new CustomExceptions.ImagingSetSchedulerException("Imaging Set Scheduler - Imaging Set field is NULL.");
            }
            ImagingSetArtifactId = ((RelativityObjectValue)imagingSetFieldValuePair.Value).ArtifactID;

            FrequencyList = new List<DayOfWeek>();
            List<Choice> choices = (List<Choice>)fieldValuePairs.Find(x => x.Field.Name == "Frequency").Value ?? throw new CustomExceptions.ImagingSetSchedulerException("Imaging Set Scheduler - Frequency field is NULL.");
            choices.ForEach(c => FrequencyList.Add(ConvertStringToDayOfWeek(c.Name)));

            if (choices.Count == 0)
            {
				throw new CustomExceptions.ImagingSetSchedulerException("Imaging Set Scheduler - Frequency field is empty.");
            }

			Time = (string)fieldValuePairs.Find(x => x.Field.Name == "Time").Value ?? throw new CustomExceptions.ImagingSetSchedulerException("Imaging Set Scheduler - Time field is NULL.");

			LockImagesForQc = (bool?)fieldValuePairs.Find(x => x.Field.Name == "Lock Images for QC").Value ?? throw new CustomExceptions.ImagingSetSchedulerException("Imaging Set Scheduler - Lock Images for QC field is NULL.");

            if (fieldValuePairs.Find(x => x.Field.Name == "Last Run").Value == null)
            {
                LastRunDate = DateTime.Now;
            } else
            {
                LastRunDate = (DateTime?)fieldValuePairs.Find(x => x.Field.Name == "Last Run").Value;
            }

            if (fieldValuePairs.Find(x => x.Field.Name == "Next Run").Value != null)
            {
                NextRunDate = (DateTime?)fieldValuePairs.Find(x => x.Field.Name == "Next Run").Value;
            }

            imagingSetFieldValuePair = fieldValuePairs.Find(x => x.Field.Name == "System Created By");
            Relativity.Services.Objects.DataContracts.User user = (Relativity.Services.Objects.DataContracts.User)imagingSetFieldValuePair.Value;
            CreatedByUserId = user.ArtifactID;
        }

		public ImagingSetScheduler(kCura.EventHandler.Artifact artifact, int currentUserArtifactId)
		{
			FrequencyList = new List<DayOfWeek>();
			var frequencyChoiceField = (ChoiceFieldValue)artifact.Fields[Constant.Guids.Field.ImagingSetScheduler.FREQUENCY.ToString()].Value;

			foreach (kCura.EventHandler.Choice choice in frequencyChoiceField.Choices)
			{
				FrequencyList.Add(ConvertStringToDayOfWeek(choice.Name));
			}

			ArtifactId = artifact.ArtifactID;
			Name = artifact.Fields[Constant.Guids.Field.ImagingSetScheduler.NAME.ToString()].Value.Value.ToString();
			ImagingSetArtifactId = (int)artifact.Fields[Constant.Guids.Field.ImagingSetScheduler.IMAGING_SET.ToString()].Value.Value;
			Time = artifact.Fields[Constant.Guids.Field.ImagingSetScheduler.TIME.ToString()].Value.Value.ToString();
			LockImagesForQc = (bool)artifact.Fields[Constant.Guids.Field.ImagingSetScheduler.LOCK_IMAGES_FOR_QC.ToString()].Value.Value;

			LastRunDate = (artifact.Fields[Constant.Guids.Field.ImagingSetScheduler.LAST_RUN.ToString()].Value.IsNull)
				? DateTime.Now
				: (DateTime)artifact.Fields[Constant.Guids.Field.ImagingSetScheduler.LAST_RUN.ToString()].Value.Value;

			if (!artifact.Fields[Constant.Guids.Field.ImagingSetScheduler.NEXT_RUN.ToString()].Value.IsNull)
			{
				NextRunDate = (DateTime)artifact.Fields[Constant.Guids.Field.ImagingSetScheduler.NEXT_RUN.ToString()].Value.Value;
			}

			CreatedByUserId = currentUserArtifactId;
		}

		public ImagingSetScheduler(DTOs.RDO artifact)
		{
			FrequencyList = new List<DayOfWeek>();
			artifact[Constant.Guids.Field.ImagingSetScheduler.FREQUENCY].ValueAsMultipleChoice.ToList().ForEach(c => FrequencyList.Add(ConvertStringToDayOfWeek(c.Name)));
			ArtifactId = artifact.ArtifactID;
			Name = artifact[Constant.Guids.Field.ImagingSetScheduler.NAME].ValueAsFixedLengthText;

			ImagingSetArtifactId = artifact[Constant.Guids.Field.ImagingSetScheduler.IMAGING_SET].ValueAsSingleObject.ArtifactID;
			if (ImagingSetArtifactId == 0)
			{
				throw new CustomExceptions.ImagingSetSchedulerException(Constant.Messages.MESSAGE_IMAGING_SET_DELETED);
			}

			Time = artifact[Constant.Guids.Field.ImagingSetScheduler.TIME].ValueAsFixedLengthText;
			LockImagesForQc = artifact[Constant.Guids.Field.ImagingSetScheduler.LOCK_IMAGES_FOR_QC].ValueAsYesNo.Value;

			LastRunDate = (artifact[Constant.Guids.Field.ImagingSetScheduler.LAST_RUN].ValueAsDate.HasValue)
				? artifact[Constant.Guids.Field.ImagingSetScheduler.LAST_RUN].ValueAsDate.Value
				: DateTime.Now;

			if (artifact[Constant.Guids.Field.ImagingSetScheduler.NEXT_RUN].ValueAsDate.HasValue)
			{
				NextRunDate = artifact[Constant.Guids.Field.ImagingSetScheduler.NEXT_RUN].ValueAsDate.Value;
			}

			CreatedByUserId = artifact.SystemCreatedBy.ArtifactID;
		}

		public void SetToComplete(IServicesMgr svcMgr, ExecutionIdentity identity, int imagingSetArtifactId, int workspaceArtifactId)
		{
			LastRunDate = DateTime.Now;
			GetNextRunDate(FrequencyList, DateTime.Now, Time);
			Update(svcMgr, identity, workspaceArtifactId, LastRunDate, NextRunDate, "", Constant.ImagingSetSchedulerStatus.COMPLETED_AT + " " + LastRunDate.Value);
		}

		public void SetToCompleteWithErrors(IServicesMgr svcMgr, ExecutionIdentity identity, int imagingSetArtifactId, int workspaceArtifactId, string errorMessages)
		{
			LastRunDate = DateTime.Now;
			GetNextRunDate(FrequencyList, DateTime.Now, Time);
			Update(svcMgr, identity, workspaceArtifactId, LastRunDate, NextRunDate, errorMessages, Constant.ImagingSetSchedulerStatus.COMPLETE_WITH_ERRORS + " at " + LastRunDate.Value);
		}

		public void SetToSkipped(IServicesMgr svcMgr, ExecutionIdentity identity, int imagingSetArtifactId, int workspaceArtifactId)
		{
			LastRunDate = DateTime.Now;
			GetNextRunDate(FrequencyList, DateTime.Now, Time);
			Update(svcMgr, identity, workspaceArtifactId, LastRunDate, NextRunDate, "", Constant.ImagingSetSchedulerStatus.SKIPPED + " " + LastRunDate.Value);
		}

		public void RemoveRecordFromQueue(int imagingSetArtifactId, IDBContext eddsDbContext, int workspaceArtifactId)
		{
			Database.SqlQueryHelper.RemoveRecordFromQueue(eddsDbContext, Constant.Tables.IMAGING_SET_SCHEDULER_QUEUE, ArtifactId, workspaceArtifactId);
		}

		public void InsertRecordIntoQueue(IDBContext eddsDbContext, int imagingSetSchedulerArtifactId, int workspaceArtifactId)
		{
			Database.SqlQueryHelper.InsertIntoJobQueue(eddsDbContext, Constant.Tables.IMAGING_SET_SCHEDULER_QUEUE, imagingSetSchedulerArtifactId, workspaceArtifactId);
		}

		public void Update(IServicesMgr svcMgr, ExecutionIdentity identity, int workspaceArtifactId, DateTime? lastRun, DateTime? nextRun, string messages, string status)
		{
			using (IRSAPIClient client = svcMgr.CreateProxy<IRSAPIClient>(identity))
			{
				client.APIOptions.WorkspaceID = workspaceArtifactId;
				client.APIOptions.StrictMode = true;

				Response<IEnumerable<DTOs.RDO>> res = RSAPI.UpdateImagingSetScheduler(client, ArtifactId, lastRun, nextRun, messages, status, workspaceArtifactId);
				if (!res.Success)
				{
					throw new CustomExceptions.ImagingSetSchedulerException(res.Message);
				}
			}
		}

		public void GetNextRunDate(List<DayOfWeek> frequencyList, DateTime today, string scheduledTime)
		{
			TimeSpan time = DateTime.ParseExact(scheduledTime, "HH:mm", CultureInfo.InvariantCulture).TimeOfDay;
			DateTime startDate = new DateTime(today.Year, today.Month, today.Day, time.Hours, time.Minutes, 0);

			NextRunDate = GetNextOccurrence(frequencyList, startDate);

			while (NextRunDate < today)
			{
				startDate = startDate.AddDays(1);
				NextRunDate = GetNextOccurrence(frequencyList, startDate);
			}
		}

		public DateTime GetNextOccurrence(List<DayOfWeek> freqencyList, DateTime startDate)
		{
			var nextOccurrence = (freqencyList.Contains(startDate.DayOfWeek)) ? startDate : GetNextOccurrence(freqencyList, startDate.AddDays(1));
			return nextOccurrence;
		}

		public DayOfWeek ConvertStringToDayOfWeek(string dayName)
		{
			DayOfWeek day = new DayOfWeek();
			switch (dayName)
			{
				case "Sunday":
					day = DayOfWeek.Sunday;
					break;
				case "Monday":
					day = DayOfWeek.Monday;
					break;
				case "Tuesday":
					day = DayOfWeek.Tuesday;
					break;
				case "Wednesday":
					day = DayOfWeek.Wednesday;
					break;
				case "Thursday":
					day = DayOfWeek.Thursday;
					break;
				case "Friday":
					day = DayOfWeek.Friday;
					break;
				case "Saturday":
					day = DayOfWeek.Saturday;
					break;
			}
			return day;
		}
	}
}
