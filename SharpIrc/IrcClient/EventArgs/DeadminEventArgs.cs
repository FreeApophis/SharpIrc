/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;

namespace SharpIrc.IrcClient.EventArgs
{
    [Serializable]
    public class DeadminEventArgs : IrcEventArgs
    {
        public DeadminEventArgs(IrcMessageData data, string channel, string who, string whom)
            : base(data)
        {
            Channel = channel;
            Who = who;
            Whom = whom;
        }

        public string Channel { get; }

        public string Who { get; }

        public string Whom { get; }
    }
}