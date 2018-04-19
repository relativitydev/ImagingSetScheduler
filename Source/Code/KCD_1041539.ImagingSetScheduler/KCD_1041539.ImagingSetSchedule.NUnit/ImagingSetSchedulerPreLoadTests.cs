using NUnit.Framework;
using KCD_1041539.ImagingSetScheduler.Helper;

namespace KCD_1041539.ImagingSetSchedule.NUnit
{
	[TestFixture]
	class ImagingSetSchedulerPreLoadTests
	{
		[Test]
		public void GetSingularOrPluralTest()
		{
			//arrange

			//act
			var noResults = ImagingSetHandlerQueries.GetPluralOrSingular(0);
			var oneResult = ImagingSetHandlerQueries.GetPluralOrSingular(1);
			var manyResults = ImagingSetHandlerQueries.GetPluralOrSingular(10);

			//assert
			Assert.AreEqual("documents", noResults);
			Assert.AreEqual("document", oneResult);
			Assert.AreEqual("documents", manyResults);
		}
	}
}
