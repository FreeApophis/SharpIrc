/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 *
 * Copyright (c) 2003-2011 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
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
using System.Collections;

namespace apophis.SharpIRC.IrcClient
{
    /// <summary>
    /// 
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    public class NonRfcChannel : Channel
    {
        private readonly Hashtable halfops = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
        private readonly Hashtable admins = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
        private readonly Hashtable owners = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"> </param>
        internal NonRfcChannel(string name)
            : base(name)
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public Hashtable Halfops
        {
            get
            {
                return (Hashtable)halfops.Clone();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeHalfops
        {
            get
            {
                return halfops;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public Hashtable Admins
        {
            get
            {
                return (Hashtable)admins.Clone();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeAdmins
        {
            get
            {
                return admins;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public Hashtable Owners
        {
            get
            {
                return (Hashtable)owners.Clone();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeOwners
        {
            get
            {
                return owners;
            }
        }
    }
}