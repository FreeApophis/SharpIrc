/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;

namespace SharpIrc.IrcClient
{
    [Serializable]
    public class ChannelInfo
    {
        internal ChannelInfo(string channel, int userCount, string topic)
        {
            Channel = channel;
            UserCount = userCount;
            Topic = topic;
        }

        public string Channel { get; private set; }

        public int UserCount { get; private set; }

        public string Topic { get; private set; }
    }
}