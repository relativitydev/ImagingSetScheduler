namespace KCD_1041539.ImagingSetScheduler.Helper
{
	public class CustomExceptions
	{
		[System.Serializable]
		public class ImagingSetSchedulerException : System.Exception
		{
			public ImagingSetSchedulerException()
				: base()
			{
			}

			public ImagingSetSchedulerException(string message)
				: base(message)
			{
			}

			public ImagingSetSchedulerException(string message, System.Exception inner)
				: base(message, inner)
			{
			}

			// A constructor is needed for serialization when an
			// exception propagates from a remoting server to the client. 
			protected ImagingSetSchedulerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			{
			}
		}
	}
}
