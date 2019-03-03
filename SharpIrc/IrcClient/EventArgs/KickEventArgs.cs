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
    public class KickEventArgs : IrcEventArgs
    {
        internal KickEventArgs(IrcMessageData data, string channel, string who, string whom, string kickReason)
            : base(data)
        {
            Channel = channel;
            Who = who;
            Whom = whom;
            KickReason = kickReason;
        }

        public string Channel { get; }

        public string Who { get; }

        public string Whom { get; }

        public string KickReason { get; }
    }
}