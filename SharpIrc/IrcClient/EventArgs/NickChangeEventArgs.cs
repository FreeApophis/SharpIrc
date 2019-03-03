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
    public class NickChangeEventArgs : IrcEventArgs
    {
        internal NickChangeEventArgs(IrcMessageData data, string oldnick, string newnick)
            : base(data)
        {
            OldNickname = oldnick;
            NewNickname = newnick;
        }

        public string OldNickname { get; private set; }

        public string NewNickname { get; private set; }
    }
}