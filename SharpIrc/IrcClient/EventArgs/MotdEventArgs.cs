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
        internal MotdEventArgs(IrcMessageData data, string motdmsg)
            : base(data)
        {
            MotdMessage = motdmsg;
        }

        public string MotdMessage { get; private set; }
    }
}