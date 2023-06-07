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
        
        readonly IBinaryFileFormat[] formats = new[] {
            new WasapiFormat(false),
            new WasapiFormat(true)
        };

        /// <summary>
        /// Enumerates the additional formats provided by this assembly.
        /// </summary>
        /// <returns>Two instances of <see cref="WasapiFormat"/>.</returns>
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