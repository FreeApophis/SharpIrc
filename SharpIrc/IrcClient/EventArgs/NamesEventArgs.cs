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
    public class NamesEventArgs : IrcEventArgs
    {
        internal NamesEventArgs(IrcMessageData data, string channel, string[] users)
            : base(data)
        {
            Channel = channel;
            Users = users;
        }

        public string Channel { get; }

        public string[] Users { get; }
    }
}