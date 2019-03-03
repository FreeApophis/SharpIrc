/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;

namespace SharpIrc.IrcFeatures.EventArgs
{
    /// <summary>
    /// Special DCC Event Arg for Receiving File Requests
    /// </summary>
    [Serializable]
    public class DccSendRequestEventArgs : DccEventArgs
    {
        internal DccSendRequestEventArgs(DccConnection dcc, string filename, long filesize)
            : base(dcc)
        {
            Filename = filename;
            Filesize = filesize;
        }

        public string Filename { get; private set; }

        public long Filesize { get; private set; }
    }
}