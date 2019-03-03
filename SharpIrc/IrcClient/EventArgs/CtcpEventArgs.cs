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
        internal CtcpEventArgs(IrcMessageData data, string ctcpcmd, string ctcpparam)
            : base(data)
        {
            CtcpCommand = ctcpcmd;
            CtcpParameter = ctcpparam;
        }

        public string CtcpCommand { get; private set; }

        public string CtcpParameter { get; private set; }
    }
}