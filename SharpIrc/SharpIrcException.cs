/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using System.Runtime.Serialization;

namespace SharpIrc
{
    /// <threadsafety static="true" instance="true" />
    [Serializable]
    public class SharpIrcException : ApplicationException
    {
        public SharpIrcException()
        {
        }

        public SharpIrcException(string message)
            : base(message)
        {
        }

        public SharpIrcException(string message, Exception e)
            : base(message, e)
        {
        }

        protected SharpIrcException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}