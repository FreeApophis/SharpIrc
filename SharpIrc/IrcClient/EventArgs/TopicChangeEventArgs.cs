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
    public class TopicChangeEventArgs : IrcEventArgs
    {
        internal TopicChangeEventArgs(IrcMessageData data, string channel, string who, string newtopic)
            : base(data)
        {
            Channel = channel;
            Who = who;
            NewTopic = newtopic;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }

        public string NewTopic { get; private set; }
    }
}