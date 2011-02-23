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

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// This class manages the information of a user within a channel.
    /// </summary>
    /// <remarks>
    /// only used with channel sync
    /// </remarks>
    /// <threadsafety static="true" instance="true" />
    public class ChannelUser
    {
        private readonly IrcUser ircUser;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"> </param>
        /// <param name="ircuser"> </param>
        internal ChannelUser(string channel, IrcUser ircuser)
        {
            Channel = channel;
            ircUser = ircuser;
        }

#if LOG4NET
        ~ChannelUser()
        {
            Logger.ChannelSyncing.Debug("ChannelUser ("+Channel+":"+IrcUser.Nick+") destroyed");
        }
#endif

        /// <summary>
        /// Gets the channel name
        /// </summary>
        public string Channel { get; private set; }

        /// <summary>
        /// Gets the server operator status of the user
        /// </summary>
        public bool IsIrcOp
        {
            get { return ircUser.IsIrcOp; }
        }

        /// <summary>
        /// Gets or sets the op flag of the user (+o)
        /// </summary>
        /// <remarks>
        /// only used with channel sync
        /// </remarks>
        public bool IsOp { get; set; }

        /// <summary>
        /// Gets or sets the voice flag of the user (+v)
        /// </summary>
        /// <remarks>
        /// only used with channel sync
        /// </remarks>
        public bool IsVoice { get; set; }

        /// <summary>
        /// Gets the away status of the user
        /// </summary>
        public bool IsAway
        {
            get { return ircUser.IsAway; }
        }

        /// <summary>
        /// Gets the underlaying IrcUser object
        /// </summary>
        public IrcUser IrcUser
        {
            get { return ircUser; }
        }

        /// <summary>
        /// Gets the nickname of the user
        /// </summary>
        public string Nick
        {
            get { return ircUser.Nick; }
        }

        /// <summary>
        /// Gets the identity (username) of the user, which is used by some IRC networks for authentication.
        /// </summary>
        public string Ident
        {
            get { return ircUser.Ident; }
        }

        /// <summary>
        /// Gets the hostname of the user,
        /// </summary>
        public string Host
        {
            get { return ircUser.Host; }
        }

        /// <summary>
        /// Gets the supposed real name of the user.
        /// </summary>
        public string Realname
        {
            get { return ircUser.Realname; }
        }

        /// <summary>
        /// Gets the server the user is connected to.
        /// </summary>
        /// <value> </value>
        public string Server
        {
            get { return ircUser.Server; }
        }

        /// <summary>
        /// Gets or sets the count of hops between you and the user's server
        /// </summary>
        public int HopCount
        {
            get { return ircUser.HopCount; }
        }

        /// <summary>
        /// Gets the list of channels the user has joined
        /// </summary>
        public string[] JoinedChannels
        {
            get { return ircUser.JoinedChannels; }
        }
    }
}