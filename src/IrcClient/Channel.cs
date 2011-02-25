/*
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2003-2005 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
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
using System.Collections.Specialized;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// 
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    public class Channel
    {
        private DateTime activeSyncStop;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"> </param>
        internal Channel(string name)
        {
            UnsafeOps = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
            UnsafeUsers = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
            UnsafeVoices = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));

            Mode = String.Empty;
            Topic = String.Empty;
            Bans = new StringCollection();
            Key = String.Empty;
            Name = name;
            ActiveSyncStart = DateTime.Now;
        }

#if LOG4NET
        ~Channel()
        {
            Logger.ChannelSyncing.Debug("Channel ("+Name+") destroyed");
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Name { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Key { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public Hashtable Users
        {
            get { return (Hashtable)UnsafeUsers.Clone(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeUsers { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public Hashtable Ops
        {
            get { return (Hashtable)UnsafeOps.Clone(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeOps { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public Hashtable Voices
        {
            get { return (Hashtable)UnsafeVoices.Clone(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeVoices { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public StringCollection Bans { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Topic { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public int UserLimit { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Mode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public DateTime ActiveSyncStart { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public DateTime ActiveSyncStop
        {
            get { return activeSyncStop; }
            set
            {
                activeSyncStop = value;
                ActiveSyncTime = activeSyncStop.Subtract(ActiveSyncStart);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public TimeSpan ActiveSyncTime { get; private set; }

        public bool IsSycned { get; set; }
    }
}