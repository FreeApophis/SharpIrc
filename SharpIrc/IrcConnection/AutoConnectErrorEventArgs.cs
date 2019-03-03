/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;

namespace SharpIrc.IrcConnection
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class AutoConnectErrorEventArgs : EventArgs
    {
        internal AutoConnectErrorEventArgs(string address, int port, Exception ex)
        {
            Address = address;
            Port = port;
            Exception = ex;
        }

        public Exception Exception { get; }

        public string Address { get; }

        public int Port { get; }
    }
}