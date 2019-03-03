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
    public class PingEventArgs : IrcEventArgs
    {
        internal PingEventArgs(IrcMessageData data, string pingdata)
            : base(data)
        {
            PingData = pingdata;
        }

        public string PingData { get; private set; }
    }
}