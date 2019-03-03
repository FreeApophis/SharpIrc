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
    public class BanEventArgs : IrcEventArgs
    {
        internal BanEventArgs(IrcMessageData data, string channel, string who, string hostmask)
            : base(data)
        {
            Channel = channel;
            Who = who;
            Hostmask = hostmask;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }

        public string Hostmask { get; private set; }
    }
}