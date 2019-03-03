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
        internal TopicChangeEventArgs(IrcMessageData data, string channel, string who, string newTopic)
            : base(data)
        {
            Channel = channel;
            Who = who;
            NewTopic = newTopic;
        }

        public string Channel { get; }

        public string Who { get; }

        public string NewTopic { get; }
    }
}