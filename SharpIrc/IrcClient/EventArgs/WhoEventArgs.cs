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
        internal WhoEventArgs(IrcMessageData data, WhoInfo whoInfo)
            : base(data)
        {
            WhoInfo = whoInfo;
        }

        public WhoInfo WhoInfo { get; }
    }
}