using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Relativity.API;

namespace KCD_1041539.ImagingSetSchedule.NUnit.Helper
{
	class Query
	{
		public static string GetImagingSetStatus(int imagingSetArtifactId, SqlConnection dbContext)
		{
			string retVal = "";
			DataTable dt = new DataTable();
			string sql = @"SELECT [Status] FROM  [EDDSDBO].ImagingSet WHERE [ArtifactId] = @imagingSetArtifactId
						";

			var command = dbContext.CreateCommand();
			command.CommandText = sql;
			command.Parameters.Add(new SqlParameter("@imagingSetArtifactId", imagingSetArtifactId));
			var dataAdapter = new SqlDataAdapter(command);
			dataAdapter.Fill(dt);

			if (dt.Rows.Count > 0 && (string)dt.Rows[0][0] != "")
			{
				retVal = (string)dt.Rows[0][0];
			}
			return retVal;
		}

		public static DataTable GetImagingSetQueueRecord(SqlConnection dbContext, int imagingSetArtifactId)
		{
			DataTable dt = new DataTable();
			string sql = @"SELECT *
						FROM ImagingSetQueue
						WHERE SetArtifactId = @imagingSetArtifactId
						";
			var command = dbContext.CreateCommand();
			command.CommandText = sql;
			command.Parameters.Add(new SqlParameter("@imagingSetArtifactId", imagingSetArtifactId));
			var dataAdapter = new SqlDataAdapter(command);
			dataAdapter.Fill(dt);

			return dt;
		}

		public static DataTable GetImagingSetSchedulerRecord(IDBContext dbContext, int imagingSetSchedulerArtifactId)
		{
			string sql = @"SELECT LastRun, NextRun, Messages, Status 
						FROM ImagingSetScheduler 
						WHERE ArtifactId = @imagingSetSchedulerArtifactId";
			
			IEnumerable<SqlParameter> sqlParameters = new List<SqlParameter>
			{
				new SqlParameter("@imagingSetSchedulerArtifactId", imagingSetSchedulerArtifactId)
			};
			return dbContext.ExecuteSqlStatementAsDataTable(sql, sqlParameters);
		}

		public static void RemoveImagingSetQueueRecords(SqlConnection dbContext)
		{
			string sql = @"DELETE FROM ImagingSetQueue";

			var command = dbContext.CreateCommand();
			command.CommandText = sql;
			command.ExecuteNonQuery();
		}

		public static DataTable GetTableRecordsFromQueue(IDBContext dbContext, string tableName)
		{
			string sql = String.Format(@"SELECT * FROM {0}", tableName);

			return dbContext.ExecuteSqlStatementAsDataTable(sql);
		}

		public static void RemoveKcdQueueRecords(IDBContext dbContext)
		{
			string sql = string.Format(@"DELETE FROM {0}", KCD_1041539.ImagingSetScheduler.Helper.Constant.Tables.IMAGING_SET_SCHEDULER_QUEUE);

			dbContext.ExecuteNonQuerySQLStatement(sql);
		}
	}
}
