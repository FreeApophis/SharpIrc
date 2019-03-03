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
    public class PongEventArgs : IrcEventArgs
    {
        internal PongEventArgs(IrcMessageData data, TimeSpan lag)
            : base(data)
        {
            Lag = lag;
        }

        public TimeSpan Lag { get; }
    }
}