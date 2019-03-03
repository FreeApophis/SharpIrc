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
        internal PartEventArgs(IrcMessageData data, string channel, string who, string partmessage)
            : base(data)
        {
            Channel = channel;
            Who = who;
            PartMessage = partmessage;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }

        public string PartMessage { get; private set; }
    }
}