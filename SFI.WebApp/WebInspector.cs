using IS4.SFI.Analyzers;
using IS4.SFI.Application;
using IS4.SFI.Application.Plugins;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IS4.SFI.WebApp
{
    /// <summary>
    /// The specific implementation of <see cref="Inspector"/> for the web application.
    /// </summary>
    public class WebInspector : ExtensibleInspector
    {
        /// <inheritdoc/>
        public WebInspector()
        {
            DataAnalyzer.FileSizeToWriteToDisk = Int64.MaxValue;
        }

        /// <inheritdoc/>
        public async override ValueTask AddDefault()
        {
            await LoadAssembly(BaseFormats.Assembly);
            await LoadAssembly(ExternalFormats.Assembly);
            await LoadAssembly(RdfFormats.Assembly);

            Plugins.Clear();
            await LoadPlugins();

            await base.AddDefault();
        }

        async ValueTask LoadPlugins()
        {
            var files = Pages.Index.PluginFiles.ToList();
            foreach(var (name, file) in files)
            {
                ArraySegment<byte> data;
                using(var stream = file.OpenReadStream(Int64.MaxValue))
                {
                    var buffer = new MemoryStream();
                    await stream.CopyToAsync(buffer);
                    if(!buffer.TryGetBuffer(out data))
                    {
                        data = new ArraySegment<byte>(buffer.ToArray());
                    }
                }
                using var archiveStream = new MemoryStream(data.Array!, data.Offset, data.Count, false);
                Plugins.Add(PluginResolvers.GetPluginFromZip(name, archiveStream));
            }
        }
    }
}
