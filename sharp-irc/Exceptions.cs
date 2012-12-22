/*
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
 *
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
using System.Runtime.Serialization;

namespace apophis.SharpIRC
{
    /// <threadsafety static="true" instance="true" />
    [Serializable]
    public class SharpIrcException : ApplicationException
    {
        public SharpIrcException()
        {
        }

        public SharpIrcException(string message)
            : base(message)
        {
        }

        public SharpIrcException(string message, Exception e)
            : base(message, e)
        {
        }

        protected SharpIrcException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <threadsafety static="true" instance="true" />
    [Serializable]
    public class ConnectionException : SharpIrcException
    {
        public ConnectionException()
        {
        }

        public ConnectionException(string message)
            : base(message)
        {
        }

        public ConnectionException(string message, Exception e)
            : base(message, e)
        {
        }

        protected ConnectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <threadsafety static="true" instance="true" />
    [Serializable]
    public class CouldNotConnectException : ConnectionException
    {
        public CouldNotConnectException()
        {
        }

        public CouldNotConnectException(string message)
            : base(message)
        {
        }

        public CouldNotConnectException(string message, Exception e)
            : base(message, e)
        {
        }

        protected CouldNotConnectException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <threadsafety static="true" instance="true" />
    [Serializable]
    public class NotConnectedException : ConnectionException
    {
        public NotConnectedException()
        {
        }

        public NotConnectedException(string message)
            : base(message)
        {
        }

        public NotConnectedException(string message, Exception e)
            : base(message, e)
        {
        }

        protected NotConnectedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <threadsafety static="true" instance="true" />
    [Serializable]
    public class AlreadyConnectedException : ConnectionException
    {
        public AlreadyConnectedException()
        {
        }

        public AlreadyConnectedException(string message)
            : base(message)
        {
        }

        public AlreadyConnectedException(string message, Exception e)
            : base(message, e)
        {
        }

        protected AlreadyConnectedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}