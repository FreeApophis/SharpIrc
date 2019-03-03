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
    public class InviteEventArgs : IrcEventArgs
    {
        internal InviteEventArgs(IrcMessageData data, string channel, string who)
            : base(data)
        {
            Channel = channel;
            Who = who;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }
    }
}