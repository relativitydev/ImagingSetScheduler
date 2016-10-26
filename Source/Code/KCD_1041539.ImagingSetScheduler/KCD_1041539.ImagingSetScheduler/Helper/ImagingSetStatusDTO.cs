using System;

namespace KCD_1041539.ImagingSetScheduler.Helper
{
	public class ImagingSetStatusDTO
	{
		public Int32 DocumentsWaiting;
		public Int32 DocumentsDone;
		public Int32 DocumentsErrored;
		public Int32 DocumentsSkipped;

		public Int32 DocumentsTotal
		{
			get
			{
				return DocumentsWaiting + DocumentsDone + DocumentsErrored + DocumentsSkipped;
			}
		}
	}
}
