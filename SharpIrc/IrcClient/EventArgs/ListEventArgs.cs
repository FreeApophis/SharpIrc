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
    public class ListEventArgs : IrcEventArgs
    {
        internal ListEventArgs(IrcMessageData data, ChannelInfo listInfo)
            : base(data)
        {
            ListInfo = listInfo;
        }

        public ChannelInfo ListInfo { get; private set; }
    }
}