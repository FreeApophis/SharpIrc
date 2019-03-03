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
        internal NickChangeEventArgs(IrcMessageData data, string oldNickname, string newNickname)
            : base(data)
        {
            OldNickname = oldNickname;
            NewNickname = newNickname;
        }

        public string OldNickname { get; }

        public string NewNickname { get; }
    }
}