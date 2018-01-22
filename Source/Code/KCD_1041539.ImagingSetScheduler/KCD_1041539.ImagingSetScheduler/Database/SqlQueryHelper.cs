using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using KCD_1041539.ImagingSetScheduler.Helper;
using Relativity.API;

namespace KCD_1041539.ImagingSetScheduler.Database
{
	public class SqlQueryHelper
	{
		public static DataTable RetrieveApplicationWorkspaces(SqlConnection dbContext)
		{
			DataTable dt = new DataTable();
			string sql = @"
				DECLARE @appArtifactID INT 
				SET @appArtifactID = (SELECT ArtifactId FROM [EDDSDBO].[ArtifactGuid] WITH(NOLOCK) WHERE ArtifactGuid = @appGuid)

				SELECT  C.ArtifactId, C.Name
				FROM [EDDSDBO].[CaseApplication] CA WITH(NOLOCK)
					INNER JOIN [EDDSDBO].[Case] C WITH(NOLOCK) ON CA.CaseID = C.ArtifactId
					INNER JOIN [EDDSDBO].[ResourceServer] RS WITH(NOLOCK) ON C.ServerID = RS.ArtifactId
					INNER JOIN [EDDSDBO].[Artifact] A WITH(NOLOCK) ON C.ArtifactId = A.ArtifactId
					INNER JOIN [EDDSDBO].[ApplicationInstall] as AI WITH(NOLOCK) on CA.CurrentApplicationInstallID = AI.ApplicationInstallID
				WHERE CA.ApplicationID = @appArtifactId
					AND CA.CaseID != -1
					AND AI.[Status] = 6 --Installed
				ORDER BY A.CreatedOn
				";

			var command = dbContext.CreateCommand();
			command.CommandText = sql;
			command.Parameters.Add(new SqlParameter("@appGuid", Constant.Guids.Application.IMAGING_SET_SCHEDULER));
			var dataAdapter = new SqlDataAdapter(command);
			dataAdapter.Fill(dt);

			return dt;
		}

		public static void SetErrorMessage(SqlConnection dbContext, string errorMessage, string status, int imagingSetSchedulerArtifactId)
		{
			string sql = @"UPDATE ImagingSetScheduler 
						SET Messages = @error,
							Status = @status
						WHERE ArtifactId = @imagingSetSchedulerArtifactId
						";
			var command = dbContext.CreateCommand();
			command.CommandText = sql;
			command.Parameters.Add(new SqlParameter("@error", errorMessage));
			command.Parameters.Add(new SqlParameter("@status", status));
			command.Parameters.Add(new SqlParameter("@imagingSetSchedulerArtifactId", imagingSetSchedulerArtifactId));
			command.ExecuteNonQuery();
		}

		public void CreateQueueTable(IDBContext eddsDbContext, string tableName)
		{
			string sql = String.Format(@"IF OBJECT_ID('[EDDSDBO].[{0}]') IS NULL BEGIN
					CREATE TABLE [EDDSDBO].[{0}]
					(
						[ImagingSetSchedulerArtifactId] INT
						,[WorkspaceArtifactID] INT
						,[Priority] INT
						,[CreatedOn] DATETIME
						,[Status] BIT
					)
					END
						", tableName);
			eddsDbContext.ExecuteNonQuerySQLStatement(sql);
		}

		public static DataTable RetrieveNextJobInQueue(SqlConnection dbContext, string tableName)
		{
			DataTable dt = new DataTable();
			string sql = String.Format(@"SET NOCOUNT ON
										BEGIN TRAN
											DECLARE @ImagingSetSchedulerArtifactId INT
											DECLARE @workspaceArtifactID INT
	
											SELECT TOP 1 
												@ImagingSetSchedulerArtifactId = ImagingSetSchedulerArtifactId, 
												@workspaceArtifactID = WorkspaceArtifactID 
											FROM EDDSDBO.{0} WITH(UPDLOCK,READPAST)  
											WHERE [Status] IS NULL 
											ORDER BY Priority, CreatedOn
	
											UPDATE EDDSDBO.{0} 
											SET [Status] = 1 
											WHERE ImagingSetSchedulerArtifactId = @ImagingSetSchedulerArtifactId 
												AND WorkspaceArtifactID = @workspaceArtifactID
										COMMIT
										SET NOCOUNT OFF 

										SELECT 
											@ImagingSetSchedulerArtifactId ImagingSetSchedulerArtifactId, 
											@workspaceArtifactID WorkspaceArtifactID
						", tableName);
			var command = dbContext.CreateCommand();
			command.CommandText = sql;
			var dataAdapter = new SqlDataAdapter(command);
			dataAdapter.Fill(dt);

			return dt;
		}

		public static void RemoveRecordFromQueue(SqlConnection dbContext, string tableName, int imagingSetSchedulerArtifactId, int workspaceArtifactId)
		{
			string sql = String.Format(@"IF NOT OBJECT_ID('EDDSDBO.{0}') IS NULL BEGIN
									DELETE FROM EDDSDBO.{0} 
									WHERE ImagingSetSchedulerArtifactId = @ImagingSetSchedulerArtifactId  
										AND WorkspaceArtifactID = @workspaceArtifactID
								END
						", tableName);
			var command = dbContext.CreateCommand();
			command.CommandText = sql;
			command.Parameters.Add(new SqlParameter("@ImagingSetSchedulerArtifactId", imagingSetSchedulerArtifactId));
			command.Parameters.Add(new SqlParameter("@workspaceArtifactID", workspaceArtifactId));
			command.ExecuteNonQuery();
		}

		public static void InsertIntoJobQueue(SqlConnection dbContext, string tableName, int imagingSetSchedulerArtifactId, int workspaceArtifactId)
		{
			string sql = String.Format(@"INSERT INTO EDDSDBO.{0}
									(ImagingSetSchedulerArtifactId, WorkspaceArtifactID, Priority, CreatedOn) 
									VALUES(@ImagingSetSchedulerArtifactId,@workspaceArtifactID,1,GetUTCDate())
						", tableName);
			var command = dbContext.CreateCommand();
			command.CommandText = sql;
			command.Parameters.Add(new SqlParameter("@ImagingSetSchedulerArtifactId", imagingSetSchedulerArtifactId));
			command.Parameters.Add(new SqlParameter("@workspaceArtifactID", workspaceArtifactId));
			command.ExecuteNonQuery();
		}

		public static int RetrieveSystemArtifactId(SqlConnection dbContext, string systemIdentifier)
		{
			DataTable dt = new DataTable();
			string sql = @"SELECT ArtifactId 
							FROM eddsdbo.SystemArtifact 
							WHERE SystemArtifactIdentifier = @systemIdentifier
						";

			var command = dbContext.CreateCommand();
			command.CommandText = sql;
			command.Parameters.Add(new SqlParameter("@systemIdentifier", systemIdentifier));
			var dataAdapter = new SqlDataAdapter(command);
			dataAdapter.Fill(dt);

			var retVal = (dt.Rows.Count > 0) ? (int)dt.Rows[0][0] : 0;
			return retVal;
		}

		public void UpdateAgentsToBeOnlyCreatedOnWebServer(IDBContext eddsDbContext)
		{
			//Get CodeTypeID for RESOURCE_SERVER_TYPE
			String sql = string.Format(@"								
				SELECT [CodeTypeID] FROM [EDDSDBO].[CodeType] WITH(NOLOCK) WHERE [Name] = '{0}'
				", Constant.RESOURCE_SERVER_TYPE);
			var resourceServerTypeCodeTypeId = eddsDbContext.ExecuteSqlStatementAsScalar<int>(sql);

			sql = string.Format(@"
				DECLARE @AgentTypeArtifactId AS NVARCHAR(MAX)
				SELECT @AgentTypeArtifactId = [ArtifactID] FROM [EDDSDBO].[SystemArtifact] WITH(NOLOCK) WHERE [SystemArtifactIdentifier] = '{1}'

				DECLARE @WebProcessingTypeArtifactId AS NVARCHAR(MAX)
				SELECT @WebProcessingTypeArtifactId = [ArtifactID] FROM [EDDSDBO].[SystemArtifact] WITH(NOLOCK) WHERE [SystemArtifactIdentifier] = '{2}'			

				DECLARE @ManagerAgentArtifactId AS NVARCHAR(MAX)
				SELECT @ManagerAgentArtifactId = [ArtifactID] FROM [EDDSDBO].[ArtifactGuid] WITH(NOLOCK) WHERE [ArtifactGuid] = @ManagerAgentGuid

				DECLARE @WorkerAgentArtifactId AS NVARCHAR(MAX)
				SELECT @WorkerAgentArtifactId = [ArtifactID] FROM [EDDSDBO].[ArtifactGuid] WITH(NOLOCK) WHERE [ArtifactGuid] = @WorkerAgentGuid

				UPDATE [EDDSDBO].[ZCodeArtifact_{0}]
				SET [CodeArtifactID] = @WebProcessingTypeArtifactId
				WHERE [AssociatedArtifactID] IN (@ManagerAgentArtifactId, @WorkerAgentArtifactId)	AND [CodeArtifactID] = @AgentTypeArtifactId
				", resourceServerTypeCodeTypeId, Constant.AGENT_RESOURCE_SERVER_TYPE, Constant.WEB_BACKGROUND_PROCESSING_SERVER_TYPE);

			var sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@ManagerAgentGuid", SqlDbType.NVarChar) { Value = Constant.Guids.AgentType.MANAGER.ToString() },
				new SqlParameter("@WorkerAgentGuid", SqlDbType.NVarChar) { Value = Constant.Guids.AgentType.WORKER.ToString() }
			};

			eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams);
		}

		public string GetRelativityVersion(IDBContext eddsDbContext)
		{
			var sql = @"
			DECLARE @COUNTER INT
			SELECT @COUNTER = COUNT(0) FROM [EDDSDBO].[Relativity] WITH(NOLOCK) WHERE [KEY] = 'Version'
			IF(@COUNTER = 0)
				BEGIN
					SELECT '-1' [Version]
				END
			ELSE
				BEGIN
					SELECT [Value] [Version] FROM [EDDSDBO].[Relativity] WITH(NOLOCK) WHERE [KEY] = 'Version'
				END
			";

			return eddsDbContext.ExecuteSqlStatementAsScalar<String>(sql);
		}

		public string GetImagingApplicationVersion(IDBContext workspaceDbContext)
		{
			var sql = @"
			DECLARE @ImagingApplicationArtifactId INT
			SELECT @ImagingApplicationArtifactId = [ArtifactID] FROM [EDDSDBO].[ArtifactGuid] WITH(NOLOCK) WHERE [ArtifactGuid] = @ImagingApplicationGuid

			DECLARE @COUNTER INT
			SELECT @COUNTER = COUNT(0) FROM [EDDSDBO].[RelativityApplication] WITH(NOLOCK) WHERE [ArtifactID] = @ImagingApplicationArtifactId
			IF(@COUNTER = 0)
				BEGIN
					SELECT '-1' [Version]
				END
			ELSE
				BEGIN
					SELECT [Version] FROM [EDDSDBO].[RelativityApplication] WITH(NOLOCK) WHERE [ArtifactID] = @ImagingApplicationArtifactId
				END
			";

			var sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@ImagingApplicationGuid", SqlDbType.NVarChar) { Value = Constant.Guids.Application.IMAGING.ToString() }
			};

			return workspaceDbContext.ExecuteSqlStatementAsScalar<String>(sql, sqlParams);
		}

		public void CreateErrorLogTable(IDBContext eddsDbContext)
		{
			var sql = String.Format(@"
				IF OBJECT_ID('EDDSDBO.{0}') IS NULL BEGIN
					CREATE TABLE EDDSDBO.{0}
					(
						ID INT IDENTITY(1,1)
						,[TimeStampUTC] DATETIME
						,WorkspaceArtifactID INT
						,ApplicationName VARCHAR(500)
						,ApplicationGuid uniqueidentifier
						,QueueTableName NVARCHAR(MAX)
						,QueueRecordID INT
						,AgentID INT
						,[Message] NVARCHAR(MAX)
					)
				END", Constant.Tables.ERROR_LOG);

			eddsDbContext.ExecuteNonQuerySQLStatement(sql);
		}

		public void InsertRowIntoErrorLog(IDBContext eddsDbContext, Int32 workspaceArtifactId, String queueTableName, Int32 queueRecordId, Int32 agentId, String errorMessage)
		{
			var sql = String.Format(@"
			INSERT INTO EDDSDBO.{0}
			(
				[TimeStampUTC]
				,WorkspaceArtifactID
				,ApplicationName
				,ApplicationGuid
				,QueueTableName
				,QueueRecordID
				,AgentID
				,[Message]
			)
			VALUES
			(
				GetUTCDate()
				,@workspaceArtifactId
				,@applicationName
				,@applicationGuid
				,@queueTableName
				,@queueRecordID
				,@agentID
				,@message
			)", Constant.Tables.ERROR_LOG);

			var sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@workspaceArtifactId", SqlDbType.Int) { Value = workspaceArtifactId }, 
				new SqlParameter("@applicationName", SqlDbType.VarChar) { Value = Constant.IMAGING_APPLICATION_NAME },
				new SqlParameter("@applicationGuid", SqlDbType.UniqueIdentifier) {  Value = Constant.Guids.Application.IMAGING_SET_SCHEDULER},
				new SqlParameter("@queueTableName", SqlDbType.VarChar) {  Value = queueTableName },
				new SqlParameter("@queueRecordID", SqlDbType.Int) { Value = queueRecordId },
				new SqlParameter("@agentID", SqlDbType.Int) { Value = agentId },
				new SqlParameter("@message", SqlDbType.NVarChar) { Value = errorMessage }
			};

			eddsDbContext.ExecuteNonQuerySQLStatement(sql, sqlParams);
		}

		public string GetRelativityServicesApiUrl(IDBContext eddsDbContext)
		{
			var sql = @"SELECT [Value] FROM [EDDSDBO].[Configuration] WITH(NOLOCK)	WHERE [Name] = 'RelativityServicesPath'";

			var retVal = eddsDbContext.ExecuteSqlStatementAsScalar<String>(sql);
			return retVal;
		}

		public string GetRelativityKeplerApiUrl(IDBContext eddsDbContext)
		{
			var sql = @"SELECT [Value] FROM [EDDSDBO].[Configuration] WITH(NOLOCK)	WHERE [Name] = 'KeplerServicesUri'";

			var retVal = eddsDbContext.ExecuteSqlStatementAsScalar<String>(sql);
			return retVal;
		}

		public Int32 GetMasterUserArtifactId(IDBContext eddsDbContext, Int32 workspaceArtifactId, Int32 workspaceUserArtifactId)
		{
			String sql = @"
				SELECT 
					[UserArtifactId] 
				FROM 
					[EDDSDBO].[UserCaseUser] WITH(NOLOCK) 
				WHERE 
					[CaseArtifactID] = @workspaceArtifactId 
					AND [CaseUserArtifactID] = @workspaceUserArtifactId
				";

			var sqlParams = new List<SqlParameter>
			{
				new SqlParameter("@workspaceArtifactId", SqlDbType.Int) { Value =  workspaceArtifactId},
				new SqlParameter("@workspaceUserArtifactId", SqlDbType.Int) { Value =  workspaceUserArtifactId}
			};

			Int32 retVal = eddsDbContext.ExecuteSqlStatementAsScalar<Int32>(sql, sqlParams);
			return retVal;
		}
	}
}
