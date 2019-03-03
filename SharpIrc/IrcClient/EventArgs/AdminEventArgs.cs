/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;

namespace SharpIrc.IrcClient.EventArgs
{
    [Serializable]
    public class AdminEventArgs : IrcEventArgs
    {
        internal AdminEventArgs(IrcMessageData data, string channel, string who, string whom)
            : base(data)
        {
            Channel = channel;
            Who = who;
            Whom = whom;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }

        public string Whom { get; private set; }
    }
}