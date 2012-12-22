/*
 *
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 *
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

namespace apophis.SharpIRC.IrcFeatures
{
    /// <summary>
    /// Base DCC Event Arguments
    /// </summary>
    public class DccEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dcc">If there are multiple streams on a DCC (a channel DCC) this identifies the stream</param>
        internal DccEventArgs(DccConnection dcc)
        {
            Dcc = dcc;
        }

        public DccConnection Dcc { get; private set; }
    }

    /// <summary>
    /// Dcc Event Args Involving Lines of Text
    /// </summary>
    public class DccChatEventArgs : DccEventArgs
    {
        internal DccChatEventArgs(DccConnection dcc, string messageLine)
            : base(dcc)
        {
            Message = messageLine;
            MessageArray = messageLine.Split(new[] { ' ' });
        }

        public string Message { get; private set; }
        public string[] MessageArray { get; private set; }
    }

    /// <summary>
    /// Dcc Event Args involving Packets of Bytes
    /// </summary>
    public class DccSendEventArgs : DccEventArgs
    {
        internal DccSendEventArgs(DccConnection dcc, byte[] package, int packageSize)
            : base(dcc)
        {
            Package = package;
            PackageSize = packageSize;
        }

        public byte[] Package { get; private set; }

        public int PackageSize { get; private set; }
    }

    /// <summary>
    /// Special DCC Event Arg for Receiving File Requests
    /// </summary>
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