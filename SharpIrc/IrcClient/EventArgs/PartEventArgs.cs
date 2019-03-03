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
    public class PartEventArgs : IrcEventArgs
    {
        internal PartEventArgs(IrcMessageData data, string channel, string who, string partMessage)
            : base(data)
        {
            Channel = channel;
            Who = who;
            PartMessage = partMessage;
        }

        public string Channel { get; }

        public string Who { get; }

        public string PartMessage { get; }
    }
}