/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;

namespace SharpIrc.IrcFeatures.EventArgs
{
    /// <summary>
    /// Base DCC Event Arguments
    /// </summary>
    [Serializable]
    public class DccEventArgs : System.EventArgs
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="dcc">If there are multiple streams on a DCC (a channel DCC) this identifies the stream</param>
        internal DccEventArgs(DccConnection dcc)
        {
            Dcc = dcc;
        }

        public DccConnection Dcc { get; }
    }
}