using System;
using System.Threading;
using System.Threading.Tasks;

namespace IS4.SFI.Application.Plugins
{
    /// <summary>
    /// Provides support for resolving plugin identifiers.
    /// </summary>
    public interface IPluginResolver
    {
        /// <summary>
        /// Retrieves a new <see cref="Plugin"/> instance from its identifier.
        /// </summary>
        /// <param name="id">The identifier of the plugin.</param>
        /// <param name="cancellationToken">The token to cancel the operation.</param>
        /// <returns>A plugin instance based on <paramref name="id"/>.</returns>
        /// <exception cref="ArgumentException">The identifier could not be resolved.</exception>
        ValueTask<Plugin> GetPluginAsync(string id, CancellationToken cancellationToken);
    }
}
