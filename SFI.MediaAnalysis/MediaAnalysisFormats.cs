using IS4.SFI.Formats;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace IS4.SFI
{
    /// <inheritdoc cref="BaseFormats"/>
    public class MediaAnalysisFormats : IEnumerable<IBinaryFileFormat>
    {
        /// <inheritdoc cref="BaseFormats.Assembly"/>
        public static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

        IBinaryFileFormat[] formats = new[] {
            new WasapiFormat(false),
            new WasapiFormat(true)
        };

        public IEnumerator<IBinaryFileFormat> GetEnumerator()
        {
            return ((IEnumerable<IBinaryFileFormat>)formats).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}