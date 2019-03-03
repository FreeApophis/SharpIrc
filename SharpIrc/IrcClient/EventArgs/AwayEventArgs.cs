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
    public class AwayEventArgs : IrcEventArgs
    {
        internal AwayEventArgs(IrcMessageData data, string who, string awayMessage)
            : base(data)
        {
            Who = who;
            AwayMessage = awayMessage;
        }

        public string Who { get; }

        public string AwayMessage { get; }
    }
}