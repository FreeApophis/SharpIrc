/*
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2003-2008 Mirco Bauer <meebey@meebey.net>
 * Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// This layer is an event driven high-level API with all features you could need for IRC programming.
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    public class IrcClient : IrcCommands
    {
        private string nickname = string.Empty;
        private string[] nicknameList;
        private int currentNickname;
        private string realname = string.Empty;
        private string usermode = string.Empty;
        private int iUsermode;
        private string username = string.Empty;
        private bool passiveChannelSyncing;
        private readonly Dictionary<string, string> autoRejoinChannels = new Dictionary<string, string>();
        private bool autoRejoinOnKick;
        private bool autoNickHandling = true;
        private bool supportNonRfc;
        private bool supportNonRfcLocked;
        private bool motdReceived;
        private Array replyCodes = Enum.GetValues(typeof(ReplyCode));

        private readonly Dictionary<string, Channel> channels = new Dictionary<string, Channel>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, IrcUser> ircUsers = new Dictionary<string, IrcUser>(StringComparer.InvariantCultureIgnoreCase);

        private List<ChannelInfo> channelList;
        private readonly Object channelListSyncRoot = new Object();
        private AutoResetEvent channelListReceivedEvent;
        private List<WhoInfo> whoList;
        private readonly Object whoListSyncRoot = new Object();
        private AutoResetEvent whoListReceivedEvent;
        private List<BanInfo> banList;
        private AutoResetEvent banListReceivedEvent;

        public event EventHandler<EventArgs> OnRegistered;
        public event EventHandler<PingEventArgs> OnPing;
        public event EventHandler<PongEventArgs> OnPong;
        public event EventHandler<IrcEventArgs> OnRawMessage;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<IrcEventArgs> OnErrorMessage;
        public event EventHandler<JoinEventArgs> OnJoin;
        public event EventHandler<NamesEventArgs> OnNames;
        public event EventHandler<ListEventArgs> OnList;
        public event EventHandler<PartEventArgs> OnPart;
        public event EventHandler<QuitEventArgs> OnQuit;
        public event EventHandler<KickEventArgs> OnKick;
        public event EventHandler<AwayEventArgs> OnAway;
        public event EventHandler<IrcEventArgs> OnUnAway;
        public event EventHandler<IrcEventArgs> OnNowAway;
        public event EventHandler<InviteEventArgs> OnInvite;
        public event EventHandler<BanEventArgs> OnBan;
        public event EventHandler<UnbanEventArgs> OnUnban;
        public event EventHandler<OpEventArgs> OnOp;
        public event EventHandler<DeopEventArgs> OnDeop;
        public event EventHandler<HalfopEventArgs> OnHalfop;
        public event EventHandler<DehalfopEventArgs> OnDehalfop;
        public event EventHandler<VoiceEventArgs> OnVoice;
        public event EventHandler<DevoiceEventArgs> OnDevoice;
        public event EventHandler<WhoEventArgs> OnWho;
        public event EventHandler<MotdEventArgs> OnMotd;
        public event EventHandler<TopicEventArgs> OnTopic;
        public event EventHandler<TopicChangeEventArgs> OnTopicChange;
        public event EventHandler<NickChangeEventArgs> OnNickChange;
        public event EventHandler<IrcEventArgs> OnModeChange;
        public event EventHandler<IrcEventArgs> OnUserModeChange;
        public event EventHandler<IrcEventArgs> OnChannelModeChange;
        public event EventHandler<IrcEventArgs> OnChannelMessage;
        public event EventHandler<ActionEventArgs> OnChannelAction;
        public event EventHandler<IrcEventArgs> OnChannelNotice;
        public event EventHandler<IrcEventArgs> OnChannelActiveSynced;
        public event EventHandler<IrcEventArgs> OnChannelPassiveSynced;
        public event EventHandler<IrcEventArgs> OnQueryMessage;
        public event EventHandler<ActionEventArgs> OnQueryAction;
        public event EventHandler<IrcEventArgs> OnQueryNotice;
        public event EventHandler<CtcpEventArgs> OnCtcpRequest;
        public event EventHandler<CtcpEventArgs> OnCtcpReply;

        protected void DispatchEvent<T>(object sender, EventHandler<T> handler, T eventArgs) where T : EventArgs
        {
            if (handler == null) return;

            ThreadPool.QueueUserWorkItem(state => handler.Invoke(sender, eventArgs));
        }


        private readonly ServerProperties properties = new ServerProperties();
        public ServerProperties Properties
        {
            get
            {
                return properties;
            }
        }


        /// <summary>
        /// Enables/disables the active channel sync feature.
        /// Default: false
        /// </summary>
        public bool ActiveChannelSyncing { get; set; }

        /// <summary>
        /// Enables/disables the passive channel sync feature. Not implemented yet!
        /// </summary>
        public bool PassiveChannelSyncing
        {
            get { return passiveChannelSyncing; }
            /*
            set {
#if LOG4NET
                if (value) {
                    Logger.ChannelSyncing.Info("Passive channel syncing enabled");
                } else {
                    Logger.ChannelSyncing.Info("Passive channel syncing disabled");
                }
#endif
                passiveChannelSyncing = value;
            }
            */
        }

        /// <summary>
        /// Sets the ctcp version that should be replied on ctcp version request.
        /// </summary>
        public string CtcpVersion { get; set; }

        /// <summary>
        /// Enables/disables auto joining of channels when invited.
        /// Default: false
        /// </summary>
        public bool AutoJoinOnInvite { get; set; }

        /// <summary>
        /// Enables/disables automatic rejoining of channels when a connection to the server is lost.
        /// Default: false
        /// </summary>
        public bool AutoRejoin { get; set; }

        /// <summary>
        /// Enables/disables auto rejoining of channels when kicked.
        /// Default: false
        /// </summary>
        public bool AutoRejoinOnKick
        {
            get { return autoRejoinOnKick; }
            set
            {
#if LOG4NET
                Logger.ChannelSyncing.Info(value ? "AutoRejoinOnKick enabled" : "AutoRejoinOnKick disabled");
#endif
                autoRejoinOnKick = value;
            }
        }

        /// <summary>
        /// Enables/disables auto relogin to the server after a reconnect.
        /// Default: false
        /// </summary>
        public bool AutoRelogin { get; set; }

        /// <summary>
        /// Enables/disables auto nick handling on nick collisions
        /// Default: true
        /// </summary>
        public bool AutoNickHandling
        {
            get { return autoNickHandling; }
            set
            {
#if LOG4NET
                Logger.ChannelSyncing.Info(value ? "AutoNickHandling enabled" : "AutoNickHandling disabled");
#endif
                autoNickHandling = value;
            }
        }

        /// <summary>
        /// Enables/disables support for non rfc features.
        /// Default: false
        /// </summary>
        public bool SupportNonRfc
        {
            get { return supportNonRfc; }
            set
            {
                if (supportNonRfcLocked)
                {
                    return;
                }
#if LOG4NET

                Logger.ChannelSyncing.Info(value ? "SupportNonRfc enabled" : "SupportNonRfc disabled");
#endif
                supportNonRfc = value;
            }
        }

        /// <summary>
        /// Gets the nickname of us.
        /// </summary>
        public string Nickname
        {
            get { return nickname; }
        }

        /// <summary>
        /// Gets the list of nicknames of us.
        /// </summary>
        public string[] NicknameList
        {
            get { return nicknameList; }
        }

        /// <summary>
        /// Gets the supposed real name of us.
        /// </summary>
        public string Realname
        {
            get { return realname; }
        }

        /// <summary>
        /// Gets the username for the server.
        /// </summary>
        /// <remarks>
        /// System username is set by default 
        /// </remarks>
        public string Username
        {
            get { return username; }
        }

        /// <summary>
        /// Gets the alphanumeric mode mask of us.
        /// </summary>
        public string Usermode
        {
            get { return usermode; }
        }

        /// <summary>
        /// Gets the numeric mode mask of us.
        /// </summary>
        public int IUsermode
        {
            get { return iUsermode; }
        }

        /// <summary>
        /// Gets SASL account name
        /// </summary>
        public string SaslAccount { get; set; }

        /// <summary>
        /// Gets SASL password
        /// </summary>
        public string SaslPassword { get; set; }

        /// <summary>
        /// Returns if we are away on this connection
        /// </summary>
        public bool IsAway { get; private set; }

        /// <summary>
        /// Gets the password for the server.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Gets the list of channels we are joined.
        /// </summary>
        public StringCollection JoinedChannels { get; private set; }

        /// <summary>
        /// Gets the server message of the day.
        /// </summary>
        public StringCollection Motd { get; private set; }

        public object BanListSyncRoot { get; private set; }

        /// <summary>
        /// This class manages the connection server and provides access to all the objects needed to send and receive messages.
        /// </summary>
        public IrcClient()
        {
            BanListSyncRoot = new Object();
            Motd = new StringCollection();
            JoinedChannels = new StringCollection();
            Password = string.Empty;
            SaslPassword = string.Empty;
            SaslAccount = string.Empty;
            passiveChannelSyncing = false;
#if LOG4NET
            Logger.Main.Debug("IrcClient created");
#endif
            OnReadLine += Worker;
            OnDisconnected += Disconnected;
            OnConnectionError += ConnectionError;
        }

#if LOG4NET
        ~IrcClient()
        {
            Logger.Main.Debug("IrcClient destroyed");
        }
#endif

        /// <summary>
        /// Connection parameters required to establish an server connection.
        /// </summary>
        /// <param name="addresslist">The list of server hostnames.</param>
        /// <param name="port">The TCP port the server listens on.</param>
        public new void Connect(string[] addresslist, int port)
        {
            supportNonRfcLocked = true;
            base.Connect(addresslist, port);
        }

        /// <overloads>
        /// Reconnects to the current server.
        /// </overloads>
        /// <param name="login">If the login data should be sent, after successful connect.</param>
        /// <param name="channels">If the channels should be rejoined, after successful connect.</param>
        public void Reconnect(bool login, bool channels)
        {
            if (channels)
            {
                StoreChannelsToRejoin();
            }
            Reconnect();
            if (login)
            {
                //reset the nick to the original nicklist
                currentNickname = 0;
                // FIXME: honor _Nickname (last used nickname)
                Login(nicknameList, Realname, IUsermode, Username, Password);
            }
            if (channels)
            {
                RejoinChannels();
            }
        }

        /// <param name="login">If the login data should be sent, after successful connect.</param>
        public void Reconnect(bool login)
        {
            Reconnect(login, true);
        }

        /// <summary>
        /// Login parameters required identify with server connection
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realname of a new user.</remark>
        /// <param name="nicklist">The users list of 'nick' names which may NOT contain spaces</param>
        /// <param name="realname">The users 'real' name which may contain space characters</param>
        /// <param name="usermode">A numeric mode parameter.  
        ///   <remark>
        ///     Set to 0 to recieve wallops and be invisible. 
        ///     Set to 4 to be invisible and not receive wallops.
        ///   </remark>
        /// </param>
        /// <param name="username">The user's machine logon name</param>        
        /// <param name="password">The optional password can and MUST be set before any attempt to register
        ///  the connection is made.</param>        
        public void Login(string[] nicklist, string realname, int usermode, string username, string password)
        {
#if LOG4NET
            Logger.Connection.Info("logging in");
#endif
            nicknameList = (string[])nicklist.Clone();
            // here we set the nickname which we will try first
            nickname = nicknameList[0].Replace(" ", "");
            this.realname = realname;
            iUsermode = usermode;

            this.username = !string.IsNullOrEmpty(username) ? username.Replace(" ", "") : Environment.UserName.Replace(" ", "");

            if (SaslAccount != "")
            {
                Cap(CapabilitySubcommand.LS, Priority.Critical);
            }

            if (!string.IsNullOrEmpty(password))
            {
                Password = password;
                RfcPass(Password, Priority.Critical);
            }

            RfcNick(Nickname, Priority.Critical);
            RfcUser(Username, IUsermode, Realname, Priority.Critical);
        }

        /// <summary>
        /// Login parameters required identify with server connection
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realname of a new user.</remark>
        /// <param name="nicklist">The users list of 'nick' names which may NOT contain spaces</param>
        /// <param name="realname">The users 'real' name which may contain space characters</param>
        /// <param name="usermode">A numeric mode parameter.  
        /// Set to 0 to recieve wallops and be invisible. 
        /// Set to 4 to be invisible and not receive wallops.</param>        
        /// <param name="username">The user's machine logon name</param>        
        public void Login(string[] nicklist, string realname, int usermode, string username)
        {
            Login(nicklist, realname, usermode, username, "");
        }

        /// <summary>
        /// Login parameters required identify with server connection
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realname of a new user.</remark>
        /// <param name="nicklist">The users list of 'nick' names which may NOT contain spaces</param>
        /// <param name="realname">The users 'real' name which may contain space characters</param>
        /// <param name="usermode">A numeric mode parameter.  
        /// Set to 0 to recieve wallops and be invisible. 
        /// Set to 4 to be invisible and not receive wallops.</param>        
        public void Login(string[] nicklist, string realname, int usermode)
        {
            Login(nicklist, realname, usermode, "", "");
        }

        /// <summary>
        /// Login parameters required identify with server connection
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realname of a new user.</remark>
        /// <param name="nicklist">The users list of 'nick' names which may NOT contain spaces</param>
        /// <param name="realname">The users 'real' name which may contain space characters</param> 
        public void Login(string[] nicklist, string realname)
        {
            Login(nicklist, realname, 0, "", "");
        }

        /// <summary>
        /// Login parameters required identify with server connection
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realname of a new user.</remark>
        /// <param name="nick">The users 'nick' name which may NOT contain spaces</param>
        /// <param name="realname">The users 'real' name which may contain space characters</param>
        /// <param name="usermode">A numeric mode parameter.  
        /// Set to 0 to recieve wallops and be invisible. 
        /// Set to 4 to be invisible and not receive wallops.</param>        
        /// <param name="username">The user's machine logon name</param>        
        /// <param name="password">The optional password can and MUST be set before any attempt to register
        ///  the connection is made.</param>   
        public void Login(string nick, string realname, int usermode, string username, string password)
        {
            Login(new[] { nick, nick + "_", nick + "__" }, realname, usermode, username, password);
        }

        /// <summary>
        /// Login parameters required identify with server connection
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realname of a new user.</remark>
        /// <param name="nick">The users 'nick' name which may NOT contain spaces</param>
        /// <param name="realname">The users 'real' name which may contain space characters</param>
        /// <param name="usermode">A numeric mode parameter.  
        /// Set to 0 to recieve wallops and be invisible. 
        /// Set to 4 to be invisible and not receive wallops.</param>        
        /// <param name="username">The user's machine logon name</param>        
        public void Login(string nick, string realname, int usermode, string username)
        {
            Login(new[] { nick, nick + "_", nick + "__" }, realname, usermode, username, "");
        }

        /// <summary>
        /// Login parameters required identify with server connection
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realname of a new user.</remark>
        /// <param name="nick">The users 'nick' name which may NOT contain spaces</param>
        /// <param name="realname">The users 'real' name which may contain space characters</param>
        /// <param name="usermode">A numeric mode parameter.  
        /// Set to 0 to recieve wallops and be invisible. 
        /// Set to 4 to be invisible and not receive wallops.</param>        
        public void Login(string nick, string realname, int usermode)
        {
            Login(new[] { nick, nick + "_", nick + "__" }, realname, usermode, "", "");
        }

        /// <summary>
        /// Login parameters required identify with server connection
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realname of a new user.</remark>
        /// <param name="nick">The users 'nick' name which may NOT contain spaces</param>
        /// <param name="realname">The users 'real' name which may contain space characters</param>
        public void Login(string nick, string realname)
        {
            Login(new[] { nick, nick + "_", nick + "__" }, realname, 0, "", "");
        }

        /// <summary>
        /// Determine if a specifier nickname is you
        /// </summary>
        /// <param name="nickname">The users 'nick' name which may NOT contain spaces</param>
        /// <returns>True if nickname belongs to you</returns>
        public bool IsMe(string nickname)
        {
            return (Nickname == nickname);
        }

        /// <summary>
        /// Determines if your nickname can be found in a specified channel
        /// </summary>
        /// <param name="channelname">The name of the channel you wish to query</param>
        /// <returns>True if you are found in channel</returns>
        public bool IsJoined(string channelname)
        {
            return IsJoined(channelname, Nickname);
        }

        /// <summary>
        /// Determine if a specified nickname can be found in a specified channel
        /// </summary>
        /// <param name="channelname">The name of the channel you wish to query</param>
        /// <param name="nickname">The users 'nick' name which may NOT contain spaces</param>
        /// <returns>True if nickname is found in channel</returns>
        public bool IsJoined(string channelname, string nickname)
        {
            if (channelname == null)
            {
                throw new ArgumentNullException("channelname");
            }

            if (nickname == null)
            {
                throw new ArgumentNullException("nickname");
            }

            Channel channel = GetChannel(channelname);
            if (channel != null &&
                channel.UnsafeUsers != null &&
                channel.UnsafeUsers.ContainsKey(nickname))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns user information
        /// </summary>
        /// <param name="nickname">The users 'nick' name which may NOT contain spaces</param>
        /// <returns>IrcUser object of requested nickname</returns>
        public IrcUser GetIrcUser(string nickname)
        {
            if (nickname == null)
            {
                throw new ArgumentNullException("nickname");
            }

            return ircUsers[nickname];
        }

        /// <summary>
        /// Returns extended user information including channel information
        /// </summary>
        /// <param name="channelname">The name of the channel you wish to query</param>
        /// <param name="nickname">The users 'nick' name which may NOT contain spaces</param>
        /// <returns>ChannelUser object of requested channelname/nickname</returns>
        public ChannelUser GetChannelUser(string channelname, string nickname)
        {
            if (channelname == null)
            {
                throw new ArgumentNullException("channelname");
            }

            if (nickname == null)
            {
                throw new ArgumentNullException("nickname");
            }

            Channel channel = GetChannel(channelname);
            return channel != null ? (ChannelUser)channel.UnsafeUsers[nickname] : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelname">The name of the channel you wish to query</param>
        /// <returns>Channel object of requested channel</returns>
        public Channel GetChannel(string channelname)
        {
            if (channelname == null)
            {
                throw new ArgumentNullException("channelname");
            }

            return channels[channelname];
        }

        /// <summary>
        /// Gets a list of all joined channels on server
        /// </summary>
        /// <returns>String array of all joined channel names</returns>
        public string[] GetChannels()
        {
            return channels.Keys.ToArray();
        }

        /// <summary>
        /// Fetches a fresh list of all available channels that match the passed mask
        /// </summary>
        /// <returns>List of ListInfo</returns>
        public IList<ChannelInfo> GetChannelList(string mask)
        {
            var list = new List<ChannelInfo>();
            lock (channelListSyncRoot)
            {
                channelList = list;
                channelListReceivedEvent = new AutoResetEvent(false);

                // request list
                RfcList(mask);
                // wait till we have the complete list
                channelListReceivedEvent.WaitOne();

                channelListReceivedEvent = null;
                channelList = null;
            }

            return list;
        }

        /// <summary>
        /// Fetches a fresh list of users that matches the passed mask
        /// </summary>
        /// <returns>List of ListInfo</returns>
        public IList<WhoInfo> GetWhoList(string mask)
        {
            var list = new List<WhoInfo>();
            lock (whoListSyncRoot)
            {
                whoList = list;
                whoListReceivedEvent = new AutoResetEvent(false);

                // request list
                RfcWho(mask);
                // wait till we have the complete list
                whoListReceivedEvent.WaitOne();

                whoListReceivedEvent = null;
                whoList = null;
            }

            return list;
        }

        /// <summary>
        /// Fetches a fresh ban list of the specified channel
        /// </summary>
        /// <returns>List of ListInfo</returns>
        public IList<BanInfo> GetBanList(string channel)
        {
            var list = new List<BanInfo>();
            lock (BanListSyncRoot)
            {
                banList = list;
                banListReceivedEvent = new AutoResetEvent(false);

                // request list
                Ban(channel);
                // wait till we have the complete list
                banListReceivedEvent.WaitOne();

                banListReceivedEvent = null;
                banList = null;
            }

            return list;
        }

        protected virtual IrcUser CreateIrcUser(string nickname)
        {
            return new IrcUser(nickname, this);
        }

        protected virtual Channel CreateChannel(string name)
        {
            return supportNonRfc ? new NonRfcChannel(name) : new Channel(name);
        }

        protected virtual ChannelUser CreateChannelUser(string channel, IrcUser ircUser)
        {
            return supportNonRfc ? new NonRfcChannelUser(channel, ircUser) : new ChannelUser(channel, ircUser);
        }

        private void Worker(object sender, ReadLineEventArgs e)
        {
            var msg = new IrcMessageData(this, e.Line);

#if LOG4NET
            if (msg.Type == ReceiveType.Unknown)
            {
                Logger.MessageTypes.Warn("Message not understood: \"" + e.Line + "\"");
            }
#endif

            // lets see if we have events or internal messagehandler for it
            HandleEvents(msg);
        }

        private void Disconnected(object sender, EventArgs e)
        {
            if (AutoRejoin)
            {
                StoreChannelsToRejoin();
            }
            SyncingCleanup();
        }

        private void ConnectionError(object sender, EventArgs e)
        {
            try
            {
                // AutoReconnect is handled in IrcConnection.ConnectionError
                if (AutoReconnect && AutoRelogin)
                {
                    Login(nicknameList, Realname, IUsermode, Username, Password);
                }
                if (AutoReconnect && AutoRejoin)
                {
                    RejoinChannels();
                }
            }
            catch (NotConnectedException)
            {
                // HACK: this is hacky, we don't know if the Reconnect was actually successful
                // means sending IRC commands without a connection throws NotConnectedExceptions 
            }
        }

        private void StoreChannelsToRejoin()
        {
#if LOG4NET
            Logger.Connection.Info("Storing channels for rejoin...");
#endif
            lock (autoRejoinChannels)
            {
                autoRejoinChannels.Clear();
                if (ActiveChannelSyncing || PassiveChannelSyncing)
                {
                    // store the key using channel sync
                    foreach (Channel channel in channels.Values)
                    {
                        autoRejoinChannels.Add(channel.Name, channel.Key);
                    }
                }
                else
                {
                    foreach (string channel in JoinedChannels)
                    {
                        autoRejoinChannels.Add(channel, null);
                    }
                }
            }
        }

        private void RejoinChannels()
        {
#if LOG4NET
            Logger.Connection.Info("Rejoining channels...");
#endif
            lock (autoRejoinChannels)
            {
                RfcJoin(autoRejoinChannels.Keys.ToArray(), autoRejoinChannels.Values.ToArray(), Priority.High);
                autoRejoinChannels.Clear();
            }
        }

        private void SyncingCleanup()
        {
            // lets clean it baby, powered by Mr. Proper
#if LOG4NET
            Logger.ChannelSyncing.Debug("Mr. Proper action, cleaning good...");
#endif
            JoinedChannels.Clear();
            if (ActiveChannelSyncing)
            {
                channels.Clear();
                ircUsers.Clear();
            }

            IsAway = false;

            motdReceived = false;
            Motd.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        private string NextNickname()
        {
            currentNickname++;
            //if we reach the end stay there
            if (currentNickname >= nicknameList.Length)
            {
                currentNickname--;
            }
            return NicknameList[currentNickname];
        }

        private void HandleEvents(IrcMessageData ircdata)
        {
            DispatchEvent(this, OnRawMessage, new IrcEventArgs(ircdata));

            switch (ircdata.Command)
            {
                case "PING":
                    EventPing(ircdata);
                    break;
                case "ERROR":
                    EventError(ircdata);
                    break;
                case "PRIVMSG":
                    EventPrivmsg(ircdata);
                    break;
                case "NOTICE":
                    EventNotice(ircdata);
                    break;
                case "JOIN":
                    EventJoin(ircdata);
                    break;
                case "PART":
                    EventPart(ircdata);
                    break;
                case "KICK":
                    EventKick(ircdata);
                    break;
                case "QUIT":
                    EventQuit(ircdata);
                    break;
                case "TOPIC":
                    EventTopic(ircdata);
                    break;
                case "NICK":
                    EventNick(ircdata);
                    break;
                case "INVITE":
                    EventInvite(ircdata);
                    break;
                case "MODE":
                    EventMode(ircdata);
                    break;
                case "PONG":
                    EventPong(ircdata);
                    break;
                case "CAP":
                    EventCap(ircdata);
                    break;
                case "AUTHENTICATE":
                    EventAuth(ircdata);
                    break;
            }

            if (ircdata.ReplyCode != ReplyCode.Null)
            {
                switch (ircdata.ReplyCode)
                {
                    case ReplyCode.Welcome:
                        EventRplWelcome(ircdata);
                        break;
                    case ReplyCode.Topic:
                        EventRplTopic(ircdata);
                        break;
                    case ReplyCode.NoTopic:
                        EventRplNoTopic(ircdata);
                        break;
                    case ReplyCode.NamesReply:
                        EventRplNamreply(ircdata);
                        break;
                    case ReplyCode.EndOfNames:
                        EventRplEndOfNames(ircdata);
                        break;
                    case ReplyCode.List:
                        EventRplList(ircdata);
                        break;
                    case ReplyCode.ListEnd:
                        EventRplListEnd(ircdata);
                        break;
                    case ReplyCode.WhoReply:
                        EventRplWhoreply(ircdata);
                        break;
                    case ReplyCode.EndOfWho:
                        EventRplEndofwho(ircdata);
                        break;
                    case ReplyCode.ChannelModeIs:
                        EventRplChannelmodeis(ircdata);
                        break;
                    case ReplyCode.BanList:
                        EventRplBanList(ircdata);
                        break;
                    case ReplyCode.EndOfBanList:
                        EventRplEndOfBanList(ircdata);
                        break;
                    case ReplyCode.ErrorNoChannelModes:
                        EventErrNoChanModes(ircdata);
                        break;
                    case ReplyCode.Motd:
                        EventRplMotd(ircdata);
                        break;
                    case ReplyCode.EndOfMotd:
                        EventRplEndOfMotd(ircdata);
                        break;
                    case ReplyCode.Away:
                        EventRplAway(ircdata);
                        break;
                    case ReplyCode.UnAway:
                        EventRplUnaway(ircdata);
                        break;
                    case ReplyCode.NowAway:
                        EventRplNowAway(ircdata);
                        break;
                    case ReplyCode.TryAgain:
                        EventRplTryAgain(ircdata);
                        break;
                    case ReplyCode.ErrorNicknameInUse:
                        EventErrNickNameInUse(ircdata);
                        break;
                    case ReplyCode.SaslSuccess:
                    case ReplyCode.SaslFailure1:
                    case ReplyCode.SaslFailure2:
                    case ReplyCode.SaslAbort:
                        EventRplSasl(ircdata);
                        break;
                }
            }

            if (ircdata.Type == ReceiveType.ErrorMessage)
            {
                EventErr(ircdata);
            }
        }

        /// <summary>
        /// Removes a specified user from all channel lists
        /// </summary>
        /// <param name="nickname">The users 'nick' name which may NOT contain spaces</param>
        private bool RemoveIrcUser(string nickname)
        {
            if (GetIrcUser(nickname).JoinedChannels.Length == 0)
            {
                // he is nowhere else, lets kill him
                ircUsers.Remove(nickname);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a specified user from a specified channel list
        /// </summary>
        /// <param name="channelname">The name of the channel you wish to query</param>
        /// <param name="nickname">The users 'nick' name which may NOT contain spaces</param>
        private void RemoveChannelUser(string channelname, string nickname)
        {
            Channel chan = GetChannel(channelname);
            chan.UnsafeUsers.Remove(nickname);
            chan.UnsafeOps.Remove(nickname);
            chan.UnsafeVoices.Remove(nickname);
            if (SupportNonRfc)
            {
                var nchan = (NonRfcChannel)chan;
                nchan.UnsafeHalfops.Remove(nickname);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ircdata">Message data containing channel mode information</param>
        /// <param name="mode">Channel mode</param>
        /// <param name="parameter">List of supplied paramaters</param>
        private void InterpretChannelMode(IrcMessageData ircdata, string mode, string parameter)
        {
            string[] parameters = parameter.Split(new[] { ' ' });
            bool add = false;
            bool remove = false;
            int modelength = mode.Length;
            Channel channel = null;
            if (ActiveChannelSyncing)
            {
                channel = GetChannel(ircdata.Channel);
            }

            IEnumerator parametersEnumerator = parameters.GetEnumerator();
            // bring the enumerator to the 1. element
            parametersEnumerator.MoveNext();
            for (int i = 0; i < modelength; i++)
            {
                string temp;
                switch (mode[i])
                {
                    case '-':
                        add = false;
                        remove = true;
                        break;
                    case '+':
                        add = true;
                        remove = false;
                        break;
                    case 'o':
                        temp = (string)parametersEnumerator.Current;
                        parametersEnumerator.MoveNext();

                        if (add)
                        {
                            if (ActiveChannelSyncing)
                            {
                                // sanity check
                                if (GetChannelUser(ircdata.Channel, temp) != null)
                                {
                                    // update the op list
                                    try
                                    {
                                        channel.UnsafeOps.Add(temp, GetIrcUser(temp));
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("added op: " + temp + " to: " + ircdata.Channel);
#endif
                                    }
                                    catch (ArgumentException)
                                    {
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("duplicate op: " + temp + " in: " + ircdata.Channel + " not added");
#endif
                                    }

                                    // update the user op status
                                    ChannelUser cuser = GetChannelUser(ircdata.Channel, temp);
                                    cuser.IsOp = true;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("set op status: " + temp + " for: " + ircdata.Channel);
#endif
                                }
                                else
                                {
#if LOG4NET
                                    Logger.ChannelSyncing.Error("InterpretChannelMode(): GetChannelUser(" + ircdata.Channel + "," + temp + ") returned null! Ignoring...");
#endif
                                }
                            }

                            DispatchEvent(this, OnOp, new OpEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                        }
                        if (remove)
                        {
                            if (ActiveChannelSyncing)
                            {
                                // sanity check
                                if (GetChannelUser(ircdata.Channel, temp) != null)
                                {
                                    // update the op list
                                    channel.UnsafeOps.Remove(temp);
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("removed op: " + temp + " from: " + ircdata.Channel);
#endif
                                    // update the user op status
                                    ChannelUser cuser = GetChannelUser(ircdata.Channel, temp);
                                    cuser.IsOp = false;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("unset op status: " + temp + " for: " + ircdata.Channel);
#endif
                                }
                                else
                                {
#if LOG4NET
                                    Logger.ChannelSyncing.Error("InterpretChannelMode(): GetChannelUser(" + ircdata.Channel + "," + temp + ") returned null! Ignoring...");
#endif
                                }
                            }

                            DispatchEvent(this, OnDeop, new DeopEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                        }
                        break;
                    case 'h':
                        if (SupportNonRfc)
                        {
                            temp = (string)parametersEnumerator.Current;
                            parametersEnumerator.MoveNext();

                            if (add)
                            {
                                if (ActiveChannelSyncing)
                                {
                                    // sanity check
                                    if (GetChannelUser(ircdata.Channel, temp) != null)
                                    {
                                        // update the halfop list
                                        try
                                        {
                                            ((NonRfcChannel)channel).UnsafeHalfops.Add(temp, GetIrcUser(temp));
#if LOG4NET
                                            Logger.ChannelSyncing.Debug("added halfop: " + temp + " to: " + ircdata.Channel);
#endif
                                        }
                                        catch (ArgumentException)
                                        {
#if LOG4NET
                                            Logger.ChannelSyncing.Debug("duplicate halfop: " + temp + " in: " + ircdata.Channel + " not added");
#endif
                                        }

                                        // update the user halfop status
                                        var cuser = (NonRfcChannelUser)GetChannelUser(ircdata.Channel, temp);
                                        cuser.IsHalfop = true;
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("set halfop status: " + temp + " for: " + ircdata.Channel);
#endif
                                    }
                                    else
                                    {
#if LOG4NET
                                        Logger.ChannelSyncing.Error("InterpretChannelMode(): GetChannelUser(" + ircdata.Channel + "," + temp + ") returned null! Ignoring...");
#endif
                                    }
                                }

                                DispatchEvent(this, OnHalfop, new HalfopEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                            }
                            if (remove)
                            {
                                if (ActiveChannelSyncing)
                                {
                                    // sanity check
                                    if (GetChannelUser(ircdata.Channel, temp) != null)
                                    {
                                        // update the halfop list
                                        ((NonRfcChannel)channel).UnsafeHalfops.Remove(temp);
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("removed halfop: " + temp + " from: " + ircdata.Channel);
#endif
                                        // update the user halfop status
                                        var cuser = (NonRfcChannelUser)GetChannelUser(ircdata.Channel, temp);
                                        cuser.IsHalfop = false;
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("unset halfop status: " + temp + " for: " + ircdata.Channel);
#endif
                                    }
                                    else
                                    {
#if LOG4NET
                                        Logger.ChannelSyncing.Error("InterpretChannelMode(): GetChannelUser(" + ircdata.Channel + "," + temp + ") returned null! Ignoring...");
#endif
                                    }
                                }

                                DispatchEvent(this, OnDehalfop, new DehalfopEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                            }
                        }
                        break;
                    case 'v':
                        temp = (string)parametersEnumerator.Current;
                        parametersEnumerator.MoveNext();

                        if (add)
                        {
                            if (ActiveChannelSyncing)
                            {
                                // sanity check
                                if (GetChannelUser(ircdata.Channel, temp) != null)
                                {
                                    // update the voice list
                                    try
                                    {
                                        channel.UnsafeVoices.Add(temp, GetIrcUser(temp));
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("added voice: " + temp + " to: " + ircdata.Channel);
#endif
                                    }
                                    catch (ArgumentException)
                                    {
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("duplicate voice: " + temp + " in: " + ircdata.Channel + " not added");
#endif
                                    }

                                    // update the user voice status
                                    ChannelUser cuser = GetChannelUser(ircdata.Channel, temp);
                                    cuser.IsVoice = true;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("set voice status: " + temp + " for: " + ircdata.Channel);
#endif
                                }
                                else
                                {
#if LOG4NET
                                    Logger.ChannelSyncing.Error("InterpretChannelMode(): GetChannelUser(" + ircdata.Channel + "," + temp + ") returned null! Ignoring...");
#endif
                                }
                            }

                            DispatchEvent(this, OnVoice, new VoiceEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                        }
                        if (remove)
                        {
                            if (ActiveChannelSyncing)
                            {
                                // sanity check
                                if (GetChannelUser(ircdata.Channel, temp) != null)
                                {
                                    // update the voice list
                                    channel.UnsafeVoices.Remove(temp);
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("removed voice: " + temp + " from: " + ircdata.Channel);
#endif
                                    // update the user voice status
                                    ChannelUser cuser = GetChannelUser(ircdata.Channel, temp);
                                    cuser.IsVoice = false;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("unset voice status: " + temp + " for: " + ircdata.Channel);
#endif
                                }
                                else
                                {
#if LOG4NET
                                    Logger.ChannelSyncing.Error("InterpretChannelMode(): GetChannelUser(" + ircdata.Channel + "," + temp + ") returned null! Ignoring...");
#endif
                                }
                            }

                            DispatchEvent(this, OnDevoice, new DevoiceEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                        }
                        break;
                    case 'b':
                        temp = (string)parametersEnumerator.Current;
                        parametersEnumerator.MoveNext();
                        if (add)
                        {
                            if (ActiveChannelSyncing)
                            {
                                try
                                {
                                    channel.Bans.Add(temp);
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("added ban: " + temp + " to: " + ircdata.Channel);
#endif
                                }
                                catch (ArgumentException)
                                {
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("duplicate ban: " + temp + " in: " + ircdata.Channel + " not added");
#endif
                                }
                            }
                            DispatchEvent(this, OnBan, new BanEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                        }
                        if (remove)
                        {
                            if (ActiveChannelSyncing)
                            {
                                channel.Bans.Remove(temp);
#if LOG4NET
                                Logger.ChannelSyncing.Debug("removed ban: " + temp + " from: " + ircdata.Channel);
#endif
                            }
                            DispatchEvent(this, OnUnban, new UnbanEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                        }
                        break;
                    case 'l':
                        temp = (string)parametersEnumerator.Current;
                        parametersEnumerator.MoveNext();
                        if (add)
                        {
                            if (ActiveChannelSyncing)
                            {
                                try
                                {
                                    channel.UserLimit = int.Parse(temp);
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("stored user limit for: " + ircdata.Channel);
#endif
                                }
                                catch (FormatException)
                                {
#if LOG4NET
                                    Logger.ChannelSyncing.Error("could not parse user limit: " + temp + " channel: " + ircdata.Channel);
#endif
                                }
                            }
                        }
                        if (remove)
                        {
                            if (ActiveChannelSyncing)
                            {
                                channel.UserLimit = 0;
#if LOG4NET
                                Logger.ChannelSyncing.Debug("removed user limit for: " + ircdata.Channel);
#endif
                            }
                        }
                        break;
                    case 'k':
                        temp = (string)parametersEnumerator.Current;
                        parametersEnumerator.MoveNext();
                        if (add)
                        {
                            if (ActiveChannelSyncing)
                            {
                                channel.Key = temp;
#if LOG4NET
                                Logger.ChannelSyncing.Debug("stored channel key for: " + ircdata.Channel);
#endif
                            }
                        }
                        if (remove)
                        {
                            if (ActiveChannelSyncing)
                            {
                                channel.Key = "";
#if LOG4NET
                                Logger.ChannelSyncing.Debug("removed channel key for: " + ircdata.Channel);
#endif
                            }
                        }
                        break;
                    default:
                        if (add)
                        {
                            if (ActiveChannelSyncing)
                            {
                                if (channel.Mode.IndexOf(mode[i]) == -1)
                                {
                                    channel.Mode += mode[i];
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("added channel mode (" + mode[i] + ") for: " + ircdata.Channel);
#endif
                                }
                            }
                        }
                        if (remove)
                        {
                            if (ActiveChannelSyncing)
                            {
                                channel.Mode = channel.Mode.Replace(mode[i].ToString(), String.Empty);
#if LOG4NET
                                Logger.ChannelSyncing.Debug("removed channel mode (" + mode[i] + ") for: " + ircdata.Channel);
#endif
                            }
                        }
                        break;
                }
            }
        }

        #region Internal Messagehandlers

        /// <summary>
        /// Event handler for ping messages
        /// </summary>
        /// <param name="ircdata">Message data containing ping information</param>
        private void EventPing(IrcMessageData ircdata)
        {
            string server = ircdata.RawMessageArray[1].Substring(1);
#if LOG4NET
            Logger.Connection.Debug("Ping? Pong!");
#endif
            RfcPong(server, Priority.Critical);

            DispatchEvent(this, OnPing, new PingEventArgs(ircdata, server));
        }

        /// <summary>
        /// Event handler for PONG messages
        /// </summary>
        /// <param name="ircdata">Message data containing pong information</param>
        private void EventPong(IrcMessageData ircdata)
        {
            DispatchEvent(this, OnPong, new PongEventArgs(ircdata, ircdata.Irc.Lag));
        }

        /// <summary>
        /// Event handler for error messages
        /// </summary>
        /// <param name="ircdata">Message data containing error information</param>
        private void EventError(IrcMessageData ircdata)
        {
            string message = ircdata.Message;
#if LOG4NET
            Logger.Connection.Info("received ERROR from IRC server");
#endif
            DispatchEvent(this, OnError, new ErrorEventArgs(ircdata, message));
        }

        /// <summary>
        /// Event handler for join messages
        /// </summary>
        /// <param name="ircdata">Message data containing join information</param>
        private void EventJoin(IrcMessageData ircdata)
        {
            string who = ircdata.Nick;
            string channelname = ircdata.Channel;

            if (IsMe(who))
            {
                JoinedChannels.Add(channelname);
            }

            if (ActiveChannelSyncing)
            {
                Channel channel;
                if (IsMe(who))
                {
                    // we joined the channel
#if LOG4NET
                    Logger.ChannelSyncing.Debug("joining channel: " + channelname);
#endif
                    channel = CreateChannel(channelname);
                    channels.Add(channelname, channel);
                    // request channel mode
                    RfcMode(channelname);
                    // request wholist
                    RfcWho(channelname);
                    // request banlist
                    Ban(channelname);
                }
                else
                {
                    // someone else joined the channel
                    // request the who data
                    RfcWho(who);
                }

#if LOG4NET
                Logger.ChannelSyncing.Debug(who + " joins channel: " + channelname);
#endif
                channel = GetChannel(channelname);
                IrcUser ircuser = GetIrcUser(who);

                if (ircuser == null)
                {
                    ircuser = new IrcUser(who, this) { Ident = ircdata.Ident, Host = ircdata.Host };
                    ircUsers.Add(who, ircuser);
                }

                // HACK: IRCnet's anonymous channel mode feature breaks our
                // channnel sync here as they use the same nick for ALL channel
                // users!
                // Example: :anonymous!anonymous@anonymous. JOIN :$channel
                if (who == "anonymous" && ircdata.Ident == "anonymous" && ircdata.Host == "anonymous." && IsJoined(channelname, who))
                {
                    // ignore
                }
                else
                {
                    ChannelUser channeluser = CreateChannelUser(channelname, ircuser);
                    channel.UnsafeUsers.Add(who, channeluser);
                }
            }

            DispatchEvent(this, OnJoin, new JoinEventArgs(ircdata, channelname, who));
        }

        /// <summary>
        /// Event handler for part messages
        /// </summary>
        /// <param name="ircdata">Message data containing part information</param>
        private void EventPart(IrcMessageData ircdata)
        {
            string who = ircdata.Nick;
            string channel = ircdata.Channel;
            string partmessage = ircdata.Message;

            if (IsMe(who))
            {
                JoinedChannels.Remove(channel);
            }

            if (ActiveChannelSyncing)
            {
                if (IsMe(who))
                {
#if LOG4NET
                    Logger.ChannelSyncing.Debug("parting channel: " + channel);
#endif
                    channels.Remove(channel);
                }
                else
                {
#if LOG4NET
                    Logger.ChannelSyncing.Debug(who + " parts channel: " + channel);
#endif
                    // HACK: IRCnet's anonymous channel mode feature breaks our
                    // channnel sync here as they use the same nick for ALL channel
                    // users!
                    // Example: :anonymous!anonymous@anonymous. PART $channel :$msg
                    if (who == "anonymous" &&
                        ircdata.Ident == "anonymous" &&
                        ircdata.Host == "anonymous." &&
                        !IsJoined(channel, who))
                    {
                        // ignore
                    }
                    else
                    {
                        RemoveChannelUser(channel, who);
                        RemoveIrcUser(who);
                    }
                }
            }

            DispatchEvent(this, OnPart, new PartEventArgs(ircdata, channel, who, partmessage));
        }

        /// <summary>
        /// Event handler for kick messages
        /// </summary>
        /// <param name="ircdata">Message data containing kick information</param>
        private void EventKick(IrcMessageData ircdata)
        {
            string channelname = ircdata.Channel;
            string who = ircdata.Nick;
            string whom = ircdata.RawMessageArray[3];
            string reason = ircdata.Message;
            bool isme = IsMe(whom);

            if (isme)
            {
                JoinedChannels.Remove(channelname);
            }

            if (ActiveChannelSyncing)
            {
                if (isme)
                {
                    Channel channel = GetChannel(channelname);
                    channels.Remove(channelname);
                    if (autoRejoinOnKick)
                    {
                        RfcJoin(channel.Name, channel.Key);
                    }
                }
                else
                {
                    RemoveChannelUser(channelname, whom);
                    RemoveIrcUser(whom);
                }
            }
            else
            {
                if (isme && AutoRejoinOnKick)
                {
                    RfcJoin(channelname);
                }
            }

            DispatchEvent(this, OnKick, new KickEventArgs(ircdata, channelname, who, whom, reason));
        }

        /// <summary>
        /// Event handler for quit messages
        /// </summary>
        /// <param name="ircdata">Message data containing quit information</param>
        private void EventQuit(IrcMessageData ircdata)
        {
            string who = ircdata.Nick;
            string reason = ircdata.Message;

            // no need to handle if we quit, disconnect event will take care

            if (ActiveChannelSyncing)
            {
                // sanity checks, freshirc is very broken about RFC
                IrcUser user = GetIrcUser(who);
                if (user != null)
                {
                    string[] joinedChannels = user.JoinedChannels;
                    if (joinedChannels != null)
                    {
                        foreach (string channel in joinedChannels)
                        {
                            RemoveChannelUser(channel, who);
                        }
                        RemoveIrcUser(who);
#if LOG4NET
                    }
                    else
                    {
                        Logger.ChannelSyncing.Error("user.JoinedChannels (for: '" + who + "') returned null in _Event_QUIT! Ignoring...");
#endif
                    }
#if LOG4NET
                }
                else
                {
                    Logger.ChannelSyncing.Error("GetIrcUser(" + who + ") returned null in _Event_QUIT! Ignoring...");
#endif
                }
            }

            DispatchEvent(this, OnQuit, new QuitEventArgs(ircdata, who, reason));
        }

        /// <summary>
        /// Event handler for private messages
        /// </summary>
        /// <param name="ircdata">Message data containing private message information</param>
        private void EventPrivmsg(IrcMessageData ircdata)
        {
            switch (ircdata.Type)
            {
                case ReceiveType.ChannelMessage:
                    DispatchEvent(this, OnChannelMessage, new IrcEventArgs(ircdata));
                    break;
                case ReceiveType.ChannelAction:
                    DispatchEvent(this, OnChannelAction, new ActionEventArgs(ircdata, ircdata.Message.Substring(8, ircdata.Message.Length - 9)));
                    break;
                case ReceiveType.QueryMessage:
                    DispatchEvent(this, OnQueryMessage, new IrcEventArgs(ircdata));
                    break;
                case ReceiveType.QueryAction:
                    DispatchEvent(this, OnQueryAction, new ActionEventArgs(ircdata, ircdata.Message.Substring(8, ircdata.Message.Length - 9)));
                    break;
                case ReceiveType.CtcpRequest:
                    int spacePos = ircdata.Message.IndexOf(' ');
                    string cmd;
                    string param = "";
                    if (spacePos != -1)
                    {
                        cmd = ircdata.Message.Substring(1, spacePos - 1);
                        param = ircdata.Message.Substring(spacePos + 1, ircdata.Message.Length - spacePos - 2);
                    }
                    else
                    {
                        cmd = ircdata.Message.Substring(1, ircdata.Message.Length - 2);
                    }
                    DispatchEvent(this, OnCtcpRequest, new CtcpEventArgs(ircdata, cmd, param));
                    break;
            }
        }

        /// <summary>
        /// Event handler for notice messages
        /// </summary>
        /// <param name="ircdata">Message data containing notice information</param>
        private void EventNotice(IrcMessageData ircdata)
        {
            switch (ircdata.Type)
            {
                case ReceiveType.ChannelNotice:
                    DispatchEvent(this, OnChannelNotice, new IrcEventArgs(ircdata));
                    break;
                case ReceiveType.QueryNotice:
                    DispatchEvent(this, OnQueryNotice, new IrcEventArgs(ircdata));
                    break;
                case ReceiveType.CtcpReply:
                    int spacePos = ircdata.Message.IndexOf(' ');
                    string cmd;
                    string param = "";
                    if (spacePos != -1)
                    {
                        cmd = ircdata.Message.Substring(1, spacePos - 1);
                        param = ircdata.Message.Substring(spacePos + 1, ircdata.Message.Length - spacePos - 2);
                    }
                    else
                    {
                        cmd = ircdata.Message.Substring(1, ircdata.Message.Length - 2);
                    }
                    DispatchEvent(this, OnCtcpReply, new CtcpEventArgs(ircdata, cmd, param));
                    break;
            }
        }

        /// <summary>
        /// Event handler for topic messages
        /// </summary>
        /// <param name="ircdata">Message data containing topic information</param>
        private void EventTopic(IrcMessageData ircdata)
        {
            string who = ircdata.Nick;
            string channel = ircdata.Channel;
            string newtopic = ircdata.Message;

            if (ActiveChannelSyncing &&
                IsJoined(channel))
            {
                GetChannel(channel).Topic = newtopic;
#if LOG4NET
                Logger.ChannelSyncing.Debug("stored topic for channel: " + channel);
#endif
            }

            DispatchEvent(this, OnTopicChange, new TopicChangeEventArgs(ircdata, channel, who, newtopic));
        }

        /// <summary>
        /// Event handler for nickname messages
        /// </summary>
        /// <param name="ircdata">Message data containing nickname information</param>
        private void EventNick(IrcMessageData ircdata)
        {
            string oldnickname = ircdata.Nick;
            //string newnickname = ircdata.Message;
            // the colon in the NICK message is optional, thus we can't rely on Message
            string newnickname = ircdata.RawMessageArray[2];

            // so let's strip the colon if it's there
            if (newnickname.StartsWith(":"))
            {
                newnickname = newnickname.Substring(1);
            }

            if (IsMe(ircdata.Nick))
            {
                // nickname change is your own
                nickname = newnickname;
            }

            if (ActiveChannelSyncing)
            {
                IrcUser ircuser = GetIrcUser(oldnickname);

                // if we don't have any info about him, don't update him!
                // (only queries or ourself in no channels)
                if (ircuser != null)
                {
                    string[] joinedchannels = ircuser.JoinedChannels;

                    // update his nickname
                    ircuser.Nick = newnickname;
                    // remove the old entry 
                    // remove first to avoid duplication, Foo -> foo
                    ircUsers.Remove(oldnickname);
                    // add him as new entry and new nickname as key
                    ircUsers.Add(newnickname, ircuser);
#if LOG4NET
                    Logger.ChannelSyncing.Debug("updated nickname of: " + oldnickname + " to: " + newnickname);
#endif
                    // now the same for all channels he is joined
                    Channel channel;
                    ChannelUser channeluser;
                    foreach (string channelname in joinedchannels)
                    {
                        channel = GetChannel(channelname);
                        channeluser = GetChannelUser(channelname, oldnickname);
                        // remove first to avoid duplication, Foo -> foo
                        channel.UnsafeUsers.Remove(oldnickname);
                        channel.UnsafeUsers.Add(newnickname, channeluser);
                        if (channeluser.IsOp)
                        {
                            channel.UnsafeOps.Remove(oldnickname);
                            channel.UnsafeOps.Add(newnickname, channeluser);
                        }
                        if (SupportNonRfc && ((NonRfcChannelUser)channeluser).IsHalfop)
                        {
                            var nchannel = (NonRfcChannel)channel;
                            nchannel.UnsafeHalfops.Remove(oldnickname);
                            nchannel.UnsafeHalfops.Add(newnickname, channeluser);
                        }
                        if (channeluser.IsVoice)
                        {
                            channel.UnsafeVoices.Remove(oldnickname);
                            channel.UnsafeVoices.Add(newnickname, channeluser);
                        }
                    }
                }
            }

            DispatchEvent(this, OnNickChange, new NickChangeEventArgs(ircdata, oldnickname, newnickname));
        }

        /// <summary>
        /// Event handler for invite messages
        /// </summary>
        /// <param name="ircdata">Message data containing invite information</param>
        private void EventInvite(IrcMessageData ircdata)
        {
            string channel = ircdata.Channel;
            string inviter = ircdata.Nick;

            if (AutoJoinOnInvite)
            {
                if (channel.Trim() != "0")
                {
                    RfcJoin(channel);
                }
            }

            DispatchEvent(this, OnInvite, new InviteEventArgs(ircdata, channel, inviter));
        }

        /// <summary>
        /// Event handler for mode messages
        /// </summary>
        /// <param name="ircdata">Message data containing mode information</param>
        private void EventMode(IrcMessageData ircdata)
        {
            if (IsMe(ircdata.RawMessageArray[2]))
            {
                // my user mode changed
                usermode = ircdata.RawMessageArray[3].Substring(1);
            }
            else
            {
                // channel mode changed
                string mode = ircdata.RawMessageArray[3];
                string parameter = String.Join(" ", ircdata.RawMessageArray, 4, ircdata.RawMessageArray.Length - 4);
                InterpretChannelMode(ircdata, mode, parameter);
            }

            switch (ircdata.Type)
            {
                case ReceiveType.UserModeChange:
                    DispatchEvent(this, OnUserModeChange, new IrcEventArgs(ircdata));
                    break;
                case ReceiveType.ChannelModeChange:
                    DispatchEvent(this, OnChannelModeChange, new IrcEventArgs(ircdata));
                    break;
            }

            DispatchEvent(this, OnModeChange, new IrcEventArgs(ircdata));
        }

        /// <summary>
        /// Event handler for SASL authentication
        /// </summary>
        /// <param name="ircdata">Message data containing authentication subcommand </param>
        private void EventAuth(IrcMessageData ircdata)
        {
            byte[] src = Encoding.UTF8.GetBytes(String.Format("{0}\0{0}\0{1}", SaslAccount, SaslPassword));
            Authenticate(Convert.ToBase64String(src), Priority.Critical);
        }

        /// <summary>
        /// Event handler for capability negotiation
        /// </summary>
        /// <param name="ircdata">Message data containing capability subcommand </param>
        private void EventCap(IrcMessageData ircdata)
        {
            if (ircdata.Args.Length > 1)
            {
                switch (ircdata.Args[1])
                {
                    case "LS":
                        if (ircdata.MessageArray.Contains("sasl"))
                        {
                            // sasl supported, request use
                            if (SaslAccount != "")
                                CapReq(new[] { "sasl" }, Priority.Critical);
                        }
                        break;

                    case "ACK":
                        if (ircdata.MessageArray.Contains("sasl"))
                        {
                            // sasl negotiated, authenticate
                            Authenticate(SaslAuthMethod.Plain, Priority.Critical);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Event handler for SASL authentication
        /// </summary>
        /// <param name="ircdata">Message data containing authentication subcommand </param>
        private void EventRplSasl(IrcMessageData ircdata)
        {
            Cap(CapabilitySubcommand.END, Priority.Critical);
            //check ircdata.ReplyCode for success
        }

        /// <summary>
        /// Event handler for channel mode reply messages
        /// </summary>
        /// <param name="ircdata">Message data containing reply information</param>
        private void EventRplChannelmodeis(IrcMessageData ircdata)
        {
            if (ActiveChannelSyncing &&
                IsJoined(ircdata.Channel))
            {
                // reset stored mode first, as this is the complete mode
                Channel chan = GetChannel(ircdata.Channel);
                chan.Mode = String.Empty;
                string mode = ircdata.RawMessageArray[4];
                string parameter = String.Join(" ", ircdata.RawMessageArray, 5, ircdata.RawMessageArray.Length - 5);
                InterpretChannelMode(ircdata, mode, parameter);
            }
        }

        /// <summary>
        /// Event handler for welcome reply messages
        /// </summary>
        /// <remark>
        /// Upon success, the client will receive an RPL_WELCOME (for users) or
        /// RPL_YOURESERVICE (for services) message indicating that the
        /// connection is now registered and known the to the entire IRC network.
        /// The reply message MUST contain the full client identifier upon which
        /// it was registered.
        /// </remark>
        /// <param name="ircdata">Message data containing reply information</param>
        private void EventRplWelcome(IrcMessageData ircdata)
        {
            // updating our nickname, that we got (maybe cutted...)
            nickname = ircdata.RawMessageArray[2];

            DispatchEvent(this, OnRegistered, EventArgs.Empty);
        }

        private void EventRplTopic(IrcMessageData ircdata)
        {
            string topic = ircdata.Message;
            string channel = ircdata.Channel;

            if (ActiveChannelSyncing &&
                IsJoined(channel))
            {
                GetChannel(channel).Topic = topic;
#if LOG4NET
                Logger.ChannelSyncing.Debug("stored topic for channel: " + channel);
#endif
            }

            DispatchEvent(this, OnTopic, new TopicEventArgs(ircdata, channel, topic));
        }

        private void EventRplNoTopic(IrcMessageData ircdata)
        {
            string channel = ircdata.Channel;

            if (ActiveChannelSyncing && IsJoined(channel))
            {
                GetChannel(channel).Topic = "";
#if LOG4NET
                Logger.ChannelSyncing.Debug("stored empty topic for channel: " + channel);
#endif
            }

            DispatchEvent(this, OnTopic, new TopicEventArgs(ircdata, channel, string.Empty));
        }

        private void EventRplNamreply(IrcMessageData ircdata)
        {
            string channelname = ircdata.Channel;
            string[] userlist = ircdata.MessageArray ?? (ircdata.RawMessageArray.Length > 5 ? new[] { ircdata.RawMessageArray[5] } : new string[] { });
            // HACK: BIP skips the colon after the channel name even though
            // RFC 1459 and 2812 says it's mandantory in RPL_NAMREPLY

            if (ActiveChannelSyncing && IsJoined(channelname))
            {
                string nickname;
                bool op;
                bool halfop;
                bool voice;
                foreach (string user in userlist)
                {
                    if (user.Length <= 0)
                    {
                        continue;
                    }

                    op = false;
                    halfop = false;
                    voice = false;
                    switch (user[0])
                    {
                        case '@':
                            op = true;
                            nickname = user.Substring(1);
                            break;
                        case '+':
                            voice = true;
                            nickname = user.Substring(1);
                            break;
                        // RFC VIOLATION
                        // some IRC network do this and break our channel sync...
                        case '&':
                            nickname = user.Substring(1);
                            break;
                        case '%':
                            halfop = true;
                            nickname = user.Substring(1);
                            break;
                        case '~':
                            nickname = user.Substring(1);
                            break;
                        default:
                            nickname = user;
                            break;
                    }

                    IrcUser ircuser = GetIrcUser(nickname);
                    ChannelUser channeluser = GetChannelUser(channelname, nickname);

                    if (ircuser == null)
                    {
#if LOG4NET
                        Logger.ChannelSyncing.Debug("creating IrcUser: " + nickname + " because he doesn't exist yet");
#endif
                        ircuser = new IrcUser(nickname, this);
                        ircUsers.Add(nickname, ircuser);
                    }

                    if (channeluser == null)
                    {
#if LOG4NET
                        Logger.ChannelSyncing.Debug("creating ChannelUser: " + nickname + " for Channel: " + channelname + " because he doesn't exist yet");
#endif

                        channeluser = CreateChannelUser(channelname, ircuser);
                        Channel channel = GetChannel(channelname);

                        channel.UnsafeUsers.Add(nickname, channeluser);
                        if (op)
                        {
                            channel.UnsafeOps.Add(nickname, channeluser);
#if LOG4NET
                            Logger.ChannelSyncing.Debug("added op: " + nickname + " to: " + channelname);
#endif
                        }
                        if (SupportNonRfc && halfop)
                        {
                            ((NonRfcChannel)channel).UnsafeHalfops.Add(nickname, channeluser);
#if LOG4NET
                            Logger.ChannelSyncing.Debug("added halfop: " + nickname + " to: " + channelname);
#endif
                        }
                        if (voice)
                        {
                            channel.UnsafeVoices.Add(nickname, channeluser);
#if LOG4NET
                            Logger.ChannelSyncing.Debug("added voice: " + nickname + " to: " + channelname);
#endif
                        }
                    }

                    channeluser.IsOp = op;
                    channeluser.IsVoice = voice;
                    if (SupportNonRfc)
                    {
                        ((NonRfcChannelUser)channeluser).IsHalfop = halfop;
                    }
                }
            }

            var filteredUserlist = new List<string>(userlist.Length);
            // filter user modes from nicknames
            foreach (string user in userlist)
            {
                if (String.IsNullOrEmpty(user))
                {
                    continue;
                }

                switch (user[0])
                {
                    case '@':
                    case '+':
                    case '&':
                    case '%':
                    case '~':
                        filteredUserlist.Add(user.Substring(1));
                        break;
                    default:
                        filteredUserlist.Add(user);
                        break;
                }
            }

            DispatchEvent(this, OnNames, new NamesEventArgs(ircdata, channelname, filteredUserlist.ToArray()));
        }

        private void EventRplList(IrcMessageData ircdata)
        {
            string channelName = ircdata.Channel;
            int userCount = Int32.Parse(ircdata.RawMessageArray[4]);
            string topic = ircdata.Message;

            ChannelInfo info = null;
            if (OnList != null || channelList != null)
            {
                info = new ChannelInfo(channelName, userCount, topic);
            }

            if (channelList != null)
            {
                channelList.Add(info);
            }

            DispatchEvent(this, OnList, new ListEventArgs(ircdata, info));
        }

        private void EventRplListEnd(IrcMessageData ircdata)
        {
            if (channelListReceivedEvent != null)
            {
                channelListReceivedEvent.Set();
            }
        }

        private void EventRplTryAgain(IrcMessageData ircdata)
        {
            if (channelListReceivedEvent != null)
            {
                channelListReceivedEvent.Set();
            }
        }

        /*
        // BUG: RFC2812 says LIST and WHO might return ERR_TOOMANYMATCHES which
        // is not defined :(
        private void _Event_ERR_TOOMANYMATCHES(IrcMessageData ircdata)
        {
            if (ListInfosReceivedEvent != null) 
            {
                ListInfosReceivedEvent.Set();
            }
        }
        */

        private void EventRplEndOfNames(IrcMessageData ircdata)
        {
            string channelname = ircdata.RawMessageArray[3];
            if (ActiveChannelSyncing &&
                IsJoined(channelname))
            {
#if LOG4NET
                Logger.ChannelSyncing.Debug("passive synced: " + channelname);
#endif
                DispatchEvent(this, OnChannelPassiveSynced, new IrcEventArgs(ircdata));
            }
        }

        private void EventRplAway(IrcMessageData ircdata)
        {
            string who = ircdata.RawMessageArray[3];
            string awaymessage = ircdata.Message;

            if (ActiveChannelSyncing)
            {
                IrcUser ircuser = GetIrcUser(who);
                if (ircuser != null)
                {
#if LOG4NET
                    Logger.ChannelSyncing.Debug("setting away flag for user: " + who);
#endif
                    ircuser.IsAway = true;
                }
            }

            DispatchEvent(this, OnAway, new AwayEventArgs(ircdata, who, awaymessage));
        }

        private void EventRplUnaway(IrcMessageData ircdata)
        {
            IsAway = false;

            DispatchEvent(this, OnUnAway, new IrcEventArgs(ircdata));
        }

        private void EventRplNowAway(IrcMessageData ircdata)
        {
            IsAway = true;

            DispatchEvent(this, OnNowAway, new IrcEventArgs(ircdata));
        }

        private void EventRplWhoreply(IrcMessageData ircdata)
        {
            WhoInfo info = WhoInfo.Parse(ircdata);
            string channel = info.Channel;
            string nick = info.Nick;

            if (whoList != null)
            {
                whoList.Add(info);
            }

            if (ActiveChannelSyncing &&
                IsJoined(channel))
            {
                // checking the irc and channel user I only do for sanity!
                // according to RFC they must be known to us already via RPL_NAMREPLY
                // psyBNC is not very correct with this... maybe other bouncers too
                IrcUser ircuser = GetIrcUser(nick);
                ChannelUser channeluser = GetChannelUser(channel, nick);
#if LOG4NET
                if (ircuser == null)
                {
                    Logger.ChannelSyncing.Error("GetIrcUser(" + nick + ") returned null in _Event_WHOREPLY! Ignoring...");
                }
#endif

#if LOG4NET
                if (channeluser == null)
                {
                    Logger.ChannelSyncing.Error("GetChannelUser(" + nick + ") returned null in _Event_WHOREPLY! Ignoring...");
                }
#endif

                if (ircuser != null)
                {
#if LOG4NET
                    Logger.ChannelSyncing.Debug("updating userinfo (from whoreply) for user: " + nick + " channel: " + channel);
#endif

                    ircuser.Ident = info.Ident;
                    ircuser.Host = info.Host;
                    ircuser.Server = info.Server;
                    ircuser.Nick = info.Nick;
                    ircuser.HopCount = info.HopCount;
                    ircuser.Realname = info.Realname;
                    ircuser.IsAway = info.IsAway;
                    ircuser.IsIrcOp = info.IsIrcOp;
                    ircuser.IsRegistered = info.IsRegistered;

                    switch (channel[0])
                    {
                        case '#':
                        case '!':
                        case '&':
                        case '+':
                            // this channel may not be where we are joined!
                            // see RFC 1459 and RFC 2812, it must return a channelname
                            // we use this channel info when possible...
                            if (channeluser != null)
                            {
                                channeluser.IsOp = info.IsOp;
                                channeluser.IsVoice = info.IsVoice;
                            }
                            break;
                    }
                }
            }

            DispatchEvent(this, OnWho, new WhoEventArgs(ircdata, info));
        }

        private void EventRplEndofwho(IrcMessageData ircdata)
        {
            if (whoListReceivedEvent != null)
            {
                whoListReceivedEvent.Set();
            }
        }

        private void EventRplMotd(IrcMessageData ircdata)
        {
            if (!motdReceived)
            {
                Motd.Add(ircdata.Message);
            }

            DispatchEvent(this, OnMotd, new MotdEventArgs(ircdata, ircdata.Message));
        }

        private void EventRplEndOfMotd(IrcMessageData ircdata)
        {
            motdReceived = true;
        }

        private void EventRplBanList(IrcMessageData ircdata)
        {
            string channelname = ircdata.Channel;

            BanInfo info = BanInfo.Parse(ircdata);
            if (banList != null)
            {
                banList.Add(info);
            }

            if (ActiveChannelSyncing &&
                IsJoined(channelname))
            {
                Channel channel = GetChannel(channelname);
                if (channel.IsSycned)
                {
                    return;
                }

                channel.Bans.Add(info.Mask);
            }
        }

        private void EventRplEndOfBanList(IrcMessageData ircdata)
        {
            string channelname = ircdata.Channel;

            if (banListReceivedEvent != null)
            {
                banListReceivedEvent.Set();
            }

            if (ActiveChannelSyncing && IsJoined(channelname))
            {
                Channel channel = GetChannel(channelname);
                if (channel.IsSycned)
                {
                    // only fire the event once
                    return;
                }

                channel.ActiveSyncStop = DateTime.Now;
                channel.IsSycned = true;
#if LOG4NET
                Logger.ChannelSyncing.Debug("active synced: " + channelname +
                    " (in " + channel.ActiveSyncTime.TotalSeconds + " sec)");
#endif
                DispatchEvent(this, OnChannelActiveSynced, new IrcEventArgs(ircdata));
            }
        }

        // MODE +b might return ERR_NOCHANMODES for mode-less channels (like +chan) 
        private void EventErrNoChanModes(IrcMessageData ircdata)
        {
            string channelname = ircdata.RawMessageArray[3];
            if (ActiveChannelSyncing && IsJoined(channelname))
            {
                Channel channel = GetChannel(channelname);
                if (channel.IsSycned)
                {
                    // only fire the event once
                    return;
                }

                channel.ActiveSyncStop = DateTime.Now;
                channel.IsSycned = true;
#if LOG4NET
                Logger.ChannelSyncing.Debug("active synced: " + channelname +
                    " (in " + channel.ActiveSyncTime.TotalSeconds + " sec)");
#endif
                DispatchEvent(this, OnChannelActiveSynced, new IrcEventArgs(ircdata));
            }
        }

        private void EventErr(IrcMessageData ircdata)
        {
            DispatchEvent(this, OnErrorMessage, new IrcEventArgs(ircdata));
        }

        private void EventErrNickNameInUse(IrcMessageData ircdata)
        {
#if LOG4NET
            Logger.Connection.Warn("nickname collision detected, changing nickname");
#endif
            if (!AutoNickHandling)
            {
                return;
            }

            string nickname;
            // if a nicklist has been given loop through the nicknames
            // if the upper limit of this list has been reached and still no nickname has registered
            // then generate a random nick
            if (currentNickname == NicknameList.Length - 1)
            {
                var rand = new Random();
                int number = rand.Next(999);
                if (Nickname.Length > 5)
                {
                    nickname = Nickname.Substring(0, 5) + number;
                }
                else
                {
                    nickname = Nickname.Substring(0, Nickname.Length - 1) + number;
                }
            }
            else
            {
                nickname = NextNickname();
            }
            // change the nickname
            RfcNick(nickname, Priority.Critical);
        }

        #endregion
    }
}