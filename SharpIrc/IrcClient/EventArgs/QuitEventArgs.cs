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
        internal QuitEventArgs(IrcMessageData data, string who, string quitmessage)
            : base(data)
        {
            Who = who;
            QuitMessage = quitmessage;
        }

        public string Who { get; private set; }

        public string QuitMessage { get; private set; }
    }
}