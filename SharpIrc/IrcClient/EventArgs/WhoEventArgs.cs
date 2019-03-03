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
    public class WhoEventArgs : IrcEventArgs
    {
        private readonly WhoInfo whoInfo;

        internal WhoEventArgs(IrcMessageData data, WhoInfo whoInfo)
            : base(data)
        {
            this.whoInfo = whoInfo;
        }

        public WhoInfo WhoInfo
        {
            get { return whoInfo; }
        }
    }
}