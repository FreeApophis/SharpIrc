/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;

namespace SharpIrc.IrcFeatures.EventArgs
{
    /// <summary>
    /// Dcc Event Args Involving Lines of Text
    /// </summary>
    [Serializable]
    public class DccChatEventArgs : DccEventArgs
    {
        internal DccChatEventArgs(DccConnection dcc, string messageLine)
            : base(dcc)
        {
            Message = messageLine;
            MessageArray = messageLine.Split(' ');
        }

        public string Message { get; }
        public string[] MessageArray { get; }
    }
}