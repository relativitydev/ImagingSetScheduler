using System;
using System.Data;
using System.Data.SqlClient;

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

		public static DataTable GetImagingSetSchedulerRecord(SqlConnection dbContext, int imagingSetSchedulerArtifactId)
		{
			DataTable dt = new DataTable();
			string sql = @"SELECT LastRun, NextRun, Messages, Status 
						FROM ImagingSetScheduler 
						WHERE ArtifactId = @imagingSetSchedulerArtifactId
						";
			var command = dbContext.CreateCommand();
			command.CommandText = sql;
			command.Parameters.Add(new SqlParameter("@imagingSetSchedulerArtifactId", imagingSetSchedulerArtifactId));
			var dataAdapter = new SqlDataAdapter(command);
			dataAdapter.Fill(dt);

			return dt;
		}

		public static void RemoveImagingSetQueueRecords(SqlConnection dbContext)
		{
			string sql = @"DELETE FROM ImagingSetQueue";

			var command = dbContext.CreateCommand();
			command.CommandText = sql;
			command.ExecuteNonQuery();
		}

		public static DataTable GetTableRecordsFromQueue(SqlConnection dbContext, string tableName)
		{
			DataTable dt = new DataTable();
			string sql = String.Format(@"SELECT * FROM {0}", tableName);
			var command = dbContext.CreateCommand();
			command.CommandText = sql;
			var dataAdapter = new SqlDataAdapter(command);
			dataAdapter.Fill(dt);

			return dt;
		}

		public static void RemoveKcdQueueRecords(SqlConnection dbContext)
		{
			string sql = string.Format(@"DELETE FROM {0}", KCD_1041539.ImagingSetScheduler.Helper.Constant.Tables.IMAIGNG_SET_SCHEDULER_QUEUE);

			var command = dbContext.CreateCommand();
			command.CommandText = sql;
			command.ExecuteNonQuery();
		}
	}
}
