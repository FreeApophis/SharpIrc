/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;

namespace SharpIrc.IrcClient.EventArgs
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class CtcpEventArgs : IrcEventArgs
    {
        internal CtcpEventArgs(IrcMessageData data, string ctcpCommand, string ctcpParameter)
            : base(data)
        {
            CtcpCommand = ctcpCommand;
            CtcpParameter = ctcpParameter;
        }

        public string CtcpCommand { get; }

        public string CtcpParameter { get; }
    }
}