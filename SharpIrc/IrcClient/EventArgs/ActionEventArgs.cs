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
        internal ActionEventArgs(IrcMessageData data, string actionmsg)
            : base(data, "ACTION", actionmsg)
        {
            ActionMessage = actionmsg;
        }

        public string ActionMessage { get; private set; }
    }
}