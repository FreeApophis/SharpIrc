/**
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
 *
 * Copyright (c) 2003 Mirco 'meebey' Bauer <mail@meebey.net> <http://www.meebey.net>
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
using System.Collections.Specialized;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    ///
    /// </summary>
    public class ReadLineEventArgs : EventArgs
    {
        private string _Line;
        
        public string Line
        {
            get {
                return _Line;
            }
        }

        internal ReadLineEventArgs(string line)
        {
            _Line = line;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class WriteLineEventArgs : EventArgs
    {
        private string _Line;
        
        public string Line
        {
            get {
                return _Line;
            }
        }

        internal WriteLineEventArgs(string line)
        {
            _Line = line;
        }
    }
}
