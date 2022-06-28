using System.Threading.Tasks;
using Relativity.API;

namespace KCD_1041539.ImagingSetScheduler.Helper
{
    public class InstanceSettingManager : IInstanceSettingManager
    {
		private readonly IInstanceSettingsBundle _instanceSettingsBundle;

		public InstanceSettingManager(IInstanceSettingsBundle instanceSettingsBundle)
		{
			_instanceSettingsBundle = instanceSettingsBundle;
		}

		public async Task<int> GetIntegerValueAsync(string section, string name, int defaultValue)
		{
			int? result = await GetIntegerValueAsync(section, name).ConfigureAwait(false);
			return result ?? defaultValue;
		}

		public async Task<int?> GetIntegerValueAsync(string section, string name)
		{
			int? result = null;
			result = await _instanceSettingsBundle.GetIntAsync(section, name).ConfigureAwait(false);

			return result;
		}
	}
}
