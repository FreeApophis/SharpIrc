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
        private readonly string[] userList;

        internal NamesEventArgs(IrcMessageData data, string channel, string[] userlist)
            : base(data)
        {
            Channel = channel;
            userList = userlist;
        }

        public string Channel { get; private set; }

        public string[] UserList
        {
            get { return userList; }
        }
    }
}