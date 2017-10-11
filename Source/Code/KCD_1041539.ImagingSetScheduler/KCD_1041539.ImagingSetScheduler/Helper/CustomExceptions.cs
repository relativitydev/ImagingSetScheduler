namespace KCD_1041539.ImagingSetScheduler.Helper
{
	public class CustomExceptions
	{
		[System.Serializable]
		public class ImagingSetSchedulerException : System.Exception
		{
			public ImagingSetSchedulerException(string message)
				: base(message)
			{
			}

			public ImagingSetSchedulerException(string message, System.Exception inner)
				: base(message, inner)
			{
			}
		}
	}
}
