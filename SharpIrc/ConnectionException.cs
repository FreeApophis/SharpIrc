/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using System.Runtime.Serialization;

namespace SharpIrc
{
    /// <threadsafety static="true" instance="true" />
    [Serializable]
    public class ConnectionException : SharpIrcException
    {
        public ConnectionException()
        {
        }

        public ConnectionException(string message)
            : base(message)
        {
        }

        public ConnectionException(string message, Exception e)
            : base(message, e)
        {
        }

        protected ConnectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}