using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCD_1041539.ImagingSetScheduler.Context
{
	public interface IContextContainerFactory
	{
		/// <summary>
		/// Builds a <see cref="IContextContainer"/>.
		/// </summary>
		/// <returns>A <see cref="IContextContainer"/>.</returns>
		IContextContainer BuildContextContainer();
	}
}
