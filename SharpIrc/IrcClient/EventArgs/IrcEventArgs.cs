/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using SharpIrc.IrcClient;

namespace SharpIrc
{
    /// <summary>
    ///
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    [Serializable]
    public class IrcEventArgs : EventArgs
    {
        internal IrcEventArgs(IrcMessageData data)
        {
            Data = data;
        }

        /// <summary>
        ///
        /// </summary>
        public IrcMessageData Data { get; private set; }
    }
}