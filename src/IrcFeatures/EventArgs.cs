/*
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch> <http://www.apophis.ch>
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

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// Base DCC Event Arguments
    /// </summary>
    public class DccEventArgs : EventArgs
    {
        private readonly DccConnection _dcc;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dccClient"></param>
        /// <param name="stream">If there are multiple streams on a DCC (a channel DCC) this identifies the stream</param>
        internal DccEventArgs(DccConnection dcc)
        {
            _dcc = dcc;
        }

        public DccConnection dcc
        {
            get { return _dcc; }
        }
    }

    /// <summary>
    /// Dcc Event Args Involving Lines of Text
    /// </summary>
    public class DccChatEventArgs : DccEventArgs
    {
        private readonly string _Message;

        private readonly string[] _MessageArray;

        internal DccChatEventArgs(DccConnection dcc, string messageLine) : base(dcc)
        {
            char[] whiteSpace = {' '};
            _Message = messageLine;
            _MessageArray = messageLine.Split(new[] {' '});
        }

        public string Message
        {
            get { return _Message; }
        }

        public string[] MessageArray
        {
            get { return _MessageArray; }
        }
    }

    /// <summary>
    /// Dcc Event Args involving Packets of Bytes
    /// </summary>
    public class DccSendEventArgs : DccEventArgs
    {
        private readonly byte[] _Package;

        private readonly int _PackageSize;

        internal DccSendEventArgs(DccConnection dcc, byte[] package, int packageSize) : base(dcc)
        {
            _Package = package;
            _PackageSize = packageSize;
        }

        public byte[] Package
        {
            get { return _Package; }
        }

        public int PackageSize
        {
            get { return _PackageSize; }
        }
    }

    /// <summary>
    /// Special DCC Event Arg for Receiving File Requests
    /// </summary>
    public class DccSendRequestEventArgs : DccEventArgs
    {
        private readonly string _Filename;

        private readonly long _Filesize;

        internal DccSendRequestEventArgs(DccConnection dcc, string filename, long filesize) : base(dcc)
        {
            _Filename = filename;
            _Filesize = filesize;
        }

        public string Filename
        {
            get { return _Filename; }
        }

        public long Filesize
        {
            get { return _Filesize; }
        }
    }
}