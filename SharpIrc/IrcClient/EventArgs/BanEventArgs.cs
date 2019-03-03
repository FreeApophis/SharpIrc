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
        internal BanEventArgs(IrcMessageData data, string channel, string who, string hostMask)
            : base(data)
        {
            Channel = channel;
            Who = who;
            HostMask = hostMask;
        }

        public string Channel { get; }

        public string Who { get; }

        public string HostMask { get; }
    }
}