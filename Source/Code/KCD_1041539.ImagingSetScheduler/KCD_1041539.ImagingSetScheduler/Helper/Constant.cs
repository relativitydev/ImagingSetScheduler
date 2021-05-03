using System;

namespace KCD_1041539.ImagingSetScheduler.Helper
{
	public class Constant
	{
		public const String RESOURCE_SERVER_TYPE = "ResourceServerType";
		public const String AGENT_RESOURCE_SERVER_TYPE = "AgentResourceServerType";
		public const String WEB_BACKGROUND_PROCESSING_SERVER_TYPE = "WebBackgroundProcessingServerType";
		public const string IMAGING_SET_SCHEDULER_SUPPORTED_RELATIVITY_VERSION = "9.5.350.16";
		public const string IMAGING_SET_SCHEDULER_SUPPORTED_IMAGING_APPLICATION_VERSION = "9.5.350.16";
		public const string IMAGING_SET_SCHEDULER_APPLICATION_NAME = "Imaging Set Scheduler";
		public const string IMAGING_APPLICATION_NAME = "Imaging";

		public class Guids
		{
			public static readonly Guid FIELD_DOCUMENT_IMAGINGSET = new Guid("B881C469-B0A8-476D-B04F-3E6D8C80EEAC");

			public class Application
			{
				public static readonly Guid IMAGING_SET_SCHEDULER = new Guid("6BE2880A-D951-4A98-A6FE-4A84835D3D06");
				public static readonly Guid IMAGING = new Guid("C9E4322E-6BD8-4A37-AE9E-C3C9BE31776B");
			}

			public class ObjectType
			{
				public static readonly Guid IMAGING_SET_SCHEDULER = new Guid("45C9FEB9-43D8-43FE-A216-85B1F062B0A7");
				public static readonly Guid IMAGING_SET = new Guid("BA574E88-7408-4434-A688-2324ECFC769E");
				public static readonly Guid IMAGING_PROFILE = new Guid("C6FAC105-3493-4551-B956-4757066E622F");
			}

			public class Field
			{
				public class ImagingSetScheduler
				{
					public static readonly Guid TIME = new Guid("DF57565B-3685-439D-91C1-091678F507F9");
					public static readonly Guid FREQUENCY = new Guid("8E0278D2-93A8-40DE-A82E-C4D3E96A2C2E");
					public static readonly Guid IMAGING_SET = new Guid("5DE74965-B185-46EF-984A-D1B18F1781C0");
					public static readonly Guid LAST_RUN = new Guid("107355C0-0625-48C2-B477-225A74DD9A7D");
					public static readonly Guid NEXT_RUN = new Guid("284C4AD2-A4CA-4415-9247-09618C68D0AD");
					public static readonly Guid LOCK_IMAGES_FOR_QC = new Guid("11FDD822-CE72-4540-BEF9-78D9E7B8A673");
					public static readonly Guid MESSAGES = new Guid("43EF59A3-11AC-465A-86F3-4412B1B97099");
					public static readonly Guid NAME = new Guid("BED4644C-3144-44B1-A3BB-C7808182B417");
					public static readonly Guid STATUS = new Guid("83900565-62A0-48FB-88B0-BBF08EC351E1");
					public static readonly Guid IMAGINGSET_STATUS = new Guid("B88FAB5E-9068-4580-AD9D-39CF37282B36");
					public static readonly Guid IMAGINGSET_IMAGE_QC_REVIEW_STATUS = new Guid("623EE11C-983A-4DCD-B930-00677C4D3692");
					public static readonly Guid IMAGINGSET_IMAGE_COMPLETION = new Guid("0DD44F75-5D5D-409F-A840-7301FAA5FC74");
					public static readonly Guid IMAGINGSET_LAST_RUN_ERROR = new Guid("4EF1ABFA-A28D-4E76-8DD0-35610AC1D366");
				}

				public class ImagingSet
				{
					public static readonly Guid NAME = new Guid("0876B4E6-8BF7-48A4-851B-A6A2A208D6CC");
					public static readonly Guid DATA_SOURCE = new Guid("A539FF48-8418-44FC-B9DD-26152C01F112");
					public static readonly Guid IMAGING_PROFILE = new Guid("4011A9DB-625F-4553-9E9E-3CFC42488B5F");
					public static readonly Guid STATUS = new Guid("030747E3-E154-4DF1-BD10-CF6C9734D10A");
				}
			}

			public class AgentType
			{
				public static readonly Guid MANAGER = new Guid("F5E3FD48-38C2-457F-ACBF-5618A30E2957");
				public static readonly Guid WORKER = new Guid("F596E9E3-5A28-408B-9FAA-334F35DE94A0");
			}
		}

		public class Messages
		{
			public const string MESSAGE_TIME_FORMAT = "Please enter a Time in the 24-hour format [HH:mm].";
			public const string MESSAGE_POST_INSTALL_FAILED = "Post-Install failed with message: ";
			public const string MESSAGE_IMAGING_SET_DELETED = "There is no associated imaging set.";
			public const string MESSAGE_IMAGES_RELEASED = "Images set to be released.  Please check the imaging set's QC Review Status for more information.";
			public const string ERROR_NO_PROCESSING_MUST_BE_BASIC = "The <b>Imaging method</b> value must be set to <b>Basic only</b> because the processing server is not specified in the resource pool.";
		}

		public class SystemArtifactIdentifiers
		{
			public const string SYS_ID_IMAGING_SET_STATUS_WAITING = "ImagingStatusCodeWaiting";
			public const string SYS_ID_IMAGING_SET_JOB_TYPE_FULL = "ImagingJobTypeCodeFullRun";
			public const string SYS_ID_IMAGING_SET_JOB_TYPE_QC_HOLD = "ImagingJobTypeCodeQCHold";
			public const string SYS_ID_PROCESSING_RESOURCE_SERVER_TYPE = "ResourceServerTypeProcessing";
		}

		public class Tables
		{
			public const string IMAGING_SET_SCHEDULER_QUEUE = "KCD_1041539_ImagingSetScheduler_Queue";
			public const string ERROR_LOG = "KCD_1041539_ImagingSetScheduler_ErrorLog";
			public const string IMAGING_JOB_TOKEN = "CustomApplicationAuthenticationToken";
		}

		public class ImagingMethods
		{
			public const int IMAGING_METHOD_BASIC_ONLY = 0;
			public const int IMAGING_METHOD_BASIC_AND_NATIVE = 1;
		}

		public class Console
		{
			public const string BUTTON_RELEASE_IMAGES_NAME = "_releaseImages";
			public const string BUTTON_RELEASE_IMAGES_TEXT = "Release Images";
			public const string BUTTON_RELEASE_IMAGES_TOOLTIP = "Release all images in the associated imaging set from QC review hold";
			public const string CONSOLE_NAME = "Imaging Set Scheduler Console";
		}

		public class ImagingSetSchedulerStatus
		{
			public const string WAITING = "Waiting";
			public const string COMPLETED_AT = "Imaging set was submitted to run at ";
			public const string COMPLETE_WITH_ERRORS = "Completed with errors";
			public const string MANAGER_ERROR = "Error in manager agent.  Please check the messages for more details.";
			public const string SKIPPED = "Imaging set is currently running. Skipped current execution.";
		}

		public class ImagingSetStatus
		{
			public const string WAITING = "Waiting";
			public const string SUBMITTING = "Submitting";
			public const string IMAGING = "Imaging";
			public const string FINALIZING = "Finalizing";
			public const string COMPLETED = "Completed";
			public const string COMPLETED_WITH_ERRORS = "Completed with errors";
			public const string STOPPED_BY_USER = "Stopped by user";
			public const string ERROR_JOB_FAILED = "Error - job failed";
			public const string STAGING = "Staging";
			public const string STOPPING = "Stopping";
			public const string PROCESSING_BUILDING_TABLES = "Processing - Building Tables";
			public const string HIDING_IMAGES = "Hiding Images";
			public const string RELEASING_IMAGES = "Releasing Images";

			//below statuses are for legacy imaging set code.
			public const string INITIALIZING = "initializing";
		}

		public class ErrorMessages
		{
			public const string IMAGING_SET_ERROR = "An error occured when submitting Imaging Set to run.";
			public const string IMAGING_SET_FAIL = "Running Imaging Set request failed.";
			public const string WORKSPACE_ARTIFACT_ID_CANNOT_BE_NEGATIVE = "WorkspaceArtifactId cannot be negative.";
			public const string IMAGING_SET_ARTIFACT_ID_CANNOT_BE_NEGATIVE = "ImagingSetArtifactId cannot be negative.";
			public const string IMAGING_SET_SCHEDULER_ARTIFACT_ID_CANNOT_BE_NEGATIVE = "ImagingSetSchedulerArtifactId cannot be negative.";
			public const string USER_ARTIFACT_ID_CANNOT_BE_NEGATIVE = "UserArtifactId cannot be negative.";
			public const string IMAGING_SET_DOES_NOT_EXIST = "Imaging Set does not exist";
			public const string IMAGING_SET_SCHEDULER_DOES_NOT_EXIST = "Imaging Set Scheduler does not exist";
			public const string WORKSPACE_DOES_NOT_EXIST = "Workspace does not exist";
		}

		public class Version
		{
			public const string PRAIRIE_SMOKE_VERSION = "12.2";
		}
	}
}
