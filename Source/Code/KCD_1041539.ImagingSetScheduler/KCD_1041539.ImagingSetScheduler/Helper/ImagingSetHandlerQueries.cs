using System;
using System.Data;
using System.Xml.Linq;
using Relativity.API;
using KCD_1041539.ImagingSetScheduler.Helper;

namespace KCD_1041539.ImagingSetScheduler.Helper
{
	public class ImagingSetHandlerQueries
	{
		public static string GetStatusInstanced(IDBContext dbContext, Int32 imagingSetArtifactId)
		{
			return GetStatus(dbContext, imagingSetArtifactId);
		}

		private static string GetStatus(IDBContext dbContext, Int32 imagingSetArtifactId)
		{
			var commandStr = new XElement("sql", "SELECT [Status] FROM [EDDSDBO].[ImagingSet] WITH(NOLOCK) WHERE ArtifactId = ", imagingSetArtifactId).Value;
			var dt = dbContext.ExecuteSqlStatementAsDataTable(commandStr);
			if (dt.Rows.Count > 0)
			{
				return dt.Rows[0]["Status"].ToString();
			}
			else
			{
				return "";
			}
		}

		public static ImagingSetStatusDTO GetImageCompletion(IDBContext caseDbContext, Int32 setId)
		{
			var setStatusDto = new ImagingSetStatusDTO();
			DataTable dt;
			var commandStr = String.Format("SELECT object_id FROM sys.objects WHERE name = 'IMAGING_POP_{0}' AND type = 'U'", setId);

			Int32 numberOfTries = default(Int32);
			do
			{
				try
				{
					dt = caseDbContext.ExecuteSqlStatementAsDataTable(commandStr);
					break;
				}
				catch (System.Data.SqlClient.SqlException ex)
				{
					switch (ex.Number)
					{
						case 1205:
						case 2755:
						case 3635:
						case 3928:
						case 5231:
						case 5252:
						case 17888:
							//Numbers from sys.messages that contain the word deadlock
							if (numberOfTries < 2)
							{
								numberOfTries += 1;
								System.Threading.Thread.CurrentThread.Join(50);
							}
							else
							{
								throw;
							}
							break;
						default:
							throw;
					}
				}
			} while (true);

			if (dt.Rows.Count > 0)
			{
				commandStr = new XElement("sql", "SELECT COUNT(1) statusCount, SA.SystemArtifactIdentifier FROM [EDDSDBO].[IMAGING_POP_", setId.ToString(), "] Pop, SystemArtifact SA WHERE SA.ArtifactId = Pop.Status GROUP BY SA.SystemArtifactIdentifier").Value;
				dt = caseDbContext.ExecuteSqlStatementAsDataTable(commandStr);

				Int32 documentsWaiting = 0;
				Int32 documentsWithErrors = 0;
				Int32 documentsCompleted = 0;
				Int32 documentsSkipped = 0;
				foreach (DataRow row in dt.Rows)
				{
					int count = Convert.ToInt32(row["statusCount"]);
					switch (row["SystemArtifactIdentifier"].ToString())
					{
						case "ImagingStatusCodeWaiting":
							documentsWaiting = documentsWaiting + count;
							break;
						case "ImagingStatusCodeCompleted":
							documentsCompleted = documentsCompleted + count;
							break;
						case "ImagingStatusCodeError":
							documentsWithErrors = documentsWithErrors + count;
							break;
						case "ImagingStatusCodePopulated":
							documentsWaiting = documentsWaiting + count;
							break;
						case "ImagingStatusCodeDocumentUpdated":
							documentsCompleted = documentsCompleted + count;
							break;
						case "ImagingStatusCodeSkipped":
							documentsSkipped = documentsSkipped + count;
							break;
					}
				}
				setStatusDto.DocumentsDone = documentsCompleted;
				setStatusDto.DocumentsErrored = documentsWithErrors;
				setStatusDto.DocumentsSkipped = documentsSkipped;
				setStatusDto.DocumentsWaiting = documentsWaiting;
			}
			return setStatusDto;
		}

		public static DataSet GetQcCounts(IDBContext caseDbContext, Int32 imageSetArtifactId)
		{
			string sql = new XElement("sql", "DECLARE @imagingSetArtifactID INT = " + imageSetArtifactId +
					" DECLARE @qcChoiceID INT = (SELECT ArtifactId FROM SystemArtifact WITH(NOLOCK) WHERE SystemArtifactIdentifier = 'DocumentQCStatusCodeImage') " +
					"DECLARE @qcFieldCodeTypeID INT = (SELECT CodeTypeID FROM SystemArtifact WITH(NOLOCK) INNER JOIN Field ON Field.ArtifactId = SystemArtifact.ArtifactId WHERE SystemArtifactIdentifier = 'DocumentQCStatusField') " +
					"DECLARE @imagingSetFieldID INT = (SELECT ArtifactId FROM [EDDSDBO].[ArtifactGuid] WITH(NOLOCK) WHERE ArtifactGuid = '" + Constant.Guids.FIELD_DOCUMENT_IMAGINGSET + "') " +
					"DECLARE @relationalTableName NVARCHAR(200) " +
					"DECLARE @relationalTableDocumentColumn NVARCHAR(200) " +
					"DECLARE @relationalTableImagingSetColumn NVARCHAR(200) " +
					"SELECT " +
					"@relationalTableName = RelationalTableSchemaName, " +
					"@relationalTableDocumentColumn = RelationalTableFieldColumnName1, " +
					"@relationalTableImagingSetColumn = RelationalTableFieldColumnName2 " +
					"FROM " +
					"ObjectsFieldRelation  WITH(NOLOCK) " +
					"WHERE FieldArtifactId1 = @imagingSetFieldID " +
					"DECLARE @sql NVARCHAR(MAX) SET @sql = N' " +
					"declare @totalCtr int, " +
					"@holdCtr int " +
					"SELECT @totalCtr = COUNT(*) " +
					"FROM ' + @relationalTableName + ' REL WITH(NOLOCK) " +
					"WHERE REL.' + @relationalTableImagingSetColumn + ' = ' + CAST(@imagingSetArtifactID AS NVARCHAR(20)) + ' " +
					"SELECT @holdCtr = COUNT(*) FROM ' + @relationalTableName + ' REL WITH (NOLOCK) " +
					"INNER JOIN " +
					"ZCodeArtifact_' + CAST(@qcFieldCodeTypeID AS NVARCHAR(20)) + ' CA WITH (NOLOCK) " +
					"ON CA.AssociatedArtifactID = REL.' + @relationalTableDocumentColumn + ' " +
					"WHERE " +
					"CA.CodeArtifactID = ' + CAST(@qcChoiceID AS NVARCHAR(20)) + ' AND REL.' + @relationalTableImagingSetColumn + ' = ' + CAST(@imagingSetArtifactID AS NVARCHAR(20)) + ' " +
					"SELECT @totalCtr - @holdCtr ReleaseCount, @holdCtr HoldCount' " +
					"EXEC sp_executesql @sql").Value;

			return caseDbContext.ExecuteSqlStatementAsDataSet(sql);
		}

		public static string GetImagingSetLastRunError(IDBContext caseDbContext, Int32 imagingSetArtifactId)
		{
			var commandStr = new XElement("sql", "SELECT [LastRunError] FROM [EDDSDBO].[ImagingSet] WITH(NOLOCK) WHERE ArtifactId = ", imagingSetArtifactId).Value;
			DataTable dt = caseDbContext.ExecuteSqlStatementAsDataTable(commandStr);

			if (dt.Rows.Count > 0)
			{
				return dt.Rows[0]["LastRunError"].ToString();
			}
			else
			{
				return "";
			}
		}

		public static string GetPluralOrSingular(Int32 items)
		{
			if ((items == 1))
			{
				return "document";
			}
			return "documents";
		}
	}
}
