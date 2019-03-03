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
        internal AwayEventArgs(IrcMessageData data, string who, string awaymessage)
            : base(data)
        {
            Who = who;
            AwayMessage = awaymessage;
        }

        public string Who { get; private set; }

        public string AwayMessage { get; private set; }
    }
}