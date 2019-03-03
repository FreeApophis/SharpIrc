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
    public class MotdEventArgs : IrcEventArgs
    {
        internal MotdEventArgs(IrcMessageData data, string motdMessage)
            : base(data)
        {
            MotdMessage = motdMessage;
        }

        public string MotdMessage { get; }
    }
}