/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 *
 * Copyright (c) 2003-2005 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
 * Copyright (c) 2008-2013 Thomas Bruderer <apophis@apophis.ch> <http://www.apophis.ch>
 * 
 * Full LGPL License: <http://www.gnu.org/licenses/lgpl.txt>
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;

namespace apophis.SharpIRC.IrcConnection
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