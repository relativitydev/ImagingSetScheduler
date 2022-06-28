using System.Threading.Tasks;

namespace KCD_1041539.ImagingSetScheduler.Helper
{
    public interface IInstanceSettingManager
    {
        /// <summary>
		/// Reads an Instance Setting value as an integer.
		/// </summary>
		/// <param name="section">The Instance Setting section.</param>
		/// <param name="name">The Instance Setting name.</param>
		/// <param name="defaultValue">The default value to use if the instance setting does not exist.</param>
		/// <returns>Instance Setting as an integer. If null is returned, then default is used.</returns>
        Task<int> GetIntegerValueAsync(string section, string name, int defaultValue);

		/// <summary>
		/// Reads an Instance Setting value as an integer.
		/// </summary>
		/// <param name="section">The Instance Setting section.</param>
		/// <param name="name">The Instance Setting name.</param>
		/// <returns>Instance Setting as an integer. If null is returned, then no value exists.</returns>
		Task<int?> GetIntegerValueAsync(string section, string name);

    }
}
