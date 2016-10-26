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

		public void RemoveRecordFromQueue(int imagingSetArtifactId, SqlConnection masterDbConnection, int workspaceArtifactId)
		{
			Database.SqlQueryHelper.RemoveRecordFromQueue(masterDbConnection, Constant.Tables.IMAIGNG_SET_SCHEDULER_QUEUE, ArtifactId, workspaceArtifactId);
		}

		public void InsertRecordIntoQueue(SqlConnection dbContext, int imagingSetSchedulerArtifactId, int workspaceArtifactId)
		{
			Database.SqlQueryHelper.InsertIntoJobQueue(dbContext, Constant.Tables.IMAIGNG_SET_SCHEDULER_QUEUE, imagingSetSchedulerArtifactId, workspaceArtifactId);
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
