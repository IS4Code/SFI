using System;
using System.Runtime.Serialization;

namespace IS4.MultiArchiver
{
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
