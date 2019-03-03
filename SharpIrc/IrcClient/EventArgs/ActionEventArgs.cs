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
    public class ActionEventArgs : CtcpEventArgs
    {
        internal ActionEventArgs(IrcMessageData data, string actionMessage)
            : base(data, "ACTION", actionMessage)
        {
            ActionMessage = actionMessage;
        }

        public string ActionMessage { get; }
    }
}