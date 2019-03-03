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
    public class QuitEventArgs : IrcEventArgs
    {
        internal QuitEventArgs(IrcMessageData data, string who, string quitMessage)
            : base(data)
        {
            Who = who;
            QuitMessage = quitMessage;
        }

        public string Who { get; }

        public string QuitMessage { get; }
    }
}