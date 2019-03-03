/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;

namespace SharpIrc.IrcFeatures.EventArgs
{
    /// <summary>
    /// Dcc Event Args involving Packets of Bytes
    /// </summary>
    [Serializable]
    public class DccSendEventArgs : DccEventArgs
    {
        internal DccSendEventArgs(DccConnection dcc, byte[] package, int packageSize)
            : base(dcc)
        {
            Package = package;
            PackageSize = packageSize;
        }

        public byte[] Package { get; }

        public int PackageSize { get; }
    }
}