using System;
using System.Runtime.Serialization;

namespace IS4.SFI
{
    /// <summary>
    /// Stores an exception in <see cref="Exception.InnerException"/> that
    /// should not be suppressed when the situation would normally be recoverable
    /// (such as when a format cannot be matched), indicating that a failure
    /// occured within the application itself.
    /// </summary>
    public class InternalApplicationException : Exception
    {
        /// <inheritdoc cref="Exception.Exception(string, Exception)"/>
        public InternalApplicationException(Exception innerException) : base(null, innerException)
        {

        }

        /// <inheritdoc/>
        protected InternalApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}
