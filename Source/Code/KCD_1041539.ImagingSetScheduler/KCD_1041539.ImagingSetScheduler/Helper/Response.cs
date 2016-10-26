namespace KCD_1041539.ImagingSetScheduler.Helper
{
	public class Response<ResultType>
	{
		public string Message { get; set; }
		public bool Success { get; set; }
		public ResultType Results { get; set; }
	}
}
