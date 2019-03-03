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
    public class TopicEventArgs : IrcEventArgs
    {
        internal TopicEventArgs(IrcMessageData data, string channel, string topic)
            : base(data)
        {
            Channel = channel;
            Topic = topic;
        }

        public string Channel { get; }

        public string Topic { get; }
    }
}