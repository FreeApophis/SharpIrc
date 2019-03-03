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
    public class ErrorEventArgs : IrcEventArgs
    {
        internal ErrorEventArgs(IrcMessageData data, string errormsg)
            : base(data)
        {
            ErrorMessage = errormsg;
        }

        public string ErrorMessage { get; private set; }
    }
}