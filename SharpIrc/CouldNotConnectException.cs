/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using System.Runtime.Serialization;

namespace SharpIrc
{
    /// <threadsafety static="true" instance="true" />
    [Serializable]
    public class CouldNotConnectException : ConnectionException
    {
        public CouldNotConnectException()
        {
        }

        public CouldNotConnectException(string message)
            : base(message)
        {
        }

        public CouldNotConnectException(string message, Exception e)
            : base(message, e)
        {
        }

        protected CouldNotConnectException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}