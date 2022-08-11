using System;
using System.Runtime.Serialization;

namespace IS4.MultiArchiver
{
    /// <summary>
    /// Stores contains an exception in <see cref="Exception.InnerException"/> that
    /// should not be suppressed when the situation would normally be recoverable
    /// (such as when a format cannot be matched), indicating that a failure
    /// occured within the application itself.
    /// </summary>
    public class InternalArchiverException : Exception
    {
        public InternalArchiverException(Exception innerException) : base(null, innerException)
        {

        }

        protected InternalArchiverException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}
