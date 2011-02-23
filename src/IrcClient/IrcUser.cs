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

using System.Linq;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// This class manages the user information.
    /// </summary>
    /// <remarks>
    /// only used with channel sync
    /// <seealso cref="IrcClient.ActiveChannelSyncing">
    ///   IrcClient.ActiveChannelSyncing
    /// </seealso>
    /// </remarks>
    /// <threadsafety static="true" instance="true" />
    public class IrcUser
    {
        private readonly IrcClient ircClient;

        internal IrcUser(string nickname, IrcClient ircclient)
        {
            HopCount = -1;
            ircClient = ircclient;
            Nick = nickname;
        }

#if LOG4NET
        ~IrcUser()
        {
            Logger.ChannelSyncing.Debug("IrcUser (" + Nick + ") destroyed");
        }
#endif

        /// <summary>
        /// Gets or sets the nickname of the user.
        /// </summary>
        /// <remarks>
        /// Do _not_ set this value, it will break channel sync!
        /// </remarks>
        public string Nick { get; set; }

        /// <summary>
        /// Gets or sets the identity (username) of the user which is used by some IRC networks for authentication. 
        /// </summary>
        /// <remarks>
        /// Do _not_ set this value, it will break channel sync!
        /// </remarks>
        public string Ident { get; set; }

        /// <summary>
        /// Gets or sets the hostname of the user. 
        /// </summary>
        /// <remarks>
        /// Do _not_ set this value, it will break channel sync!
        /// </remarks>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the supposed real name of the user.
        /// </summary>
        /// <remarks>
        /// Do _not_ set this value, it will break channel sync!
        /// </remarks>
        public string Realname { get; set; }

        /// <summary>
        /// Gets or sets the server operator status of the user
        /// </summary>
        /// <remarks>
        /// Do _not_ set this value, it will break channel sync!
        /// </remarks>
        public bool IsIrcOp { get; set; }

        /// <summary>
        /// Gets or sets the registered status of the user
        /// </summary>
        public bool IsRegistered { get; internal set; }

        /// <summary>
        /// Gets or sets away status of the user
        /// </summary>
        /// <remarks>
        /// Do _not_ set this value, it will break channel sync!
        /// </remarks>
        public bool IsAway { get; set; }

        /// <summary>
        /// Gets or sets the server the user is connected to
        /// </summary>
        /// <remarks>
        /// Do _not_ set this value, it will break channel sync!
        /// </remarks>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the count of hops between you and the user's server
        /// </summary>
        /// <remarks>
        /// Do _not_ set this value, it will break channel sync!
        /// </remarks>
        public int HopCount { get; set; }

        /// <summary>
        /// Gets the list of channels the user has joined
        /// </summary>
        public string[] JoinedChannels
        {
            get
            {
                string[] channels = ircClient.GetChannels();

                return channels
                    .Select(c => ircClient.GetChannel(c))
                    .Where(c => c.UnsafeUsers.ContainsKey(Nick))
                    .Select(c => c.Name).ToArray();
            }
        }
    }
}