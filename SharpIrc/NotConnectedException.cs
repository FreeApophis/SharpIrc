/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using System.Runtime.Serialization;

namespace SharpIrc
{
    /// <threadsafety static="true" instance="true" />
    [Serializable]
    public class NotConnectedException : ConnectionException
    {
        public NotConnectedException()
        {
        }

        public NotConnectedException(string message)
            : base(message)
        {
        }

        public NotConnectedException(string message, Exception e)
            : base(message, e)
        {
        }

        protected NotConnectedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}