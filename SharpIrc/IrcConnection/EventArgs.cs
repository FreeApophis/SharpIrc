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
    public class ReadLineEventArgs : EventArgs
    {
        internal ReadLineEventArgs(string line)
        {
            Line = line;
        }

        public string Line { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class WriteLineEventArgs : EventArgs
    {
        internal WriteLineEventArgs(string line)
        {
            Line = line;
        }

        public string Line { get; private set; }
    }

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

        public Exception Exception { get; private set; }

        public string Address { get; private set; }

        public int Port { get; private set; }
    }
}