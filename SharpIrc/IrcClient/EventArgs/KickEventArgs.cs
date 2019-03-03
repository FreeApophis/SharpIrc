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
        internal KickEventArgs(IrcMessageData data, string channel, string who, string whom, string kickreason)
            : base(data)
        {
            Channel = channel;
            Who = who;
            Whom = whom;
            KickReason = kickreason;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }

        public string Whom { get; private set; }

        public string KickReason { get; private set; }
    }
}