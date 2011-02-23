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
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// This layer is an event driven high-level API with all features you could need for IRC programming.
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    public class IrcClient : IrcCommands
    {
        private string           _Nickname                = string.Empty;
        private string[]         _NicknameList;
        private int              _CurrentNickname;
        private string           _Realname                = string.Empty;
        private string           _Usermode                = string.Empty;
        private int              _IUsermode;
        private string           _Username                = string.Empty;
        private string           _Password                = string.Empty;
        private string           _SaslAccount             = string.Empty;
        private string           _SaslPassword            = string.Empty;
        private bool             _IsAway;
        private string           _CtcpVersion;
        private bool             _ActiveChannelSyncing;
        private bool             _PassiveChannelSyncing;
        private bool             _AutoJoinOnInvite;
        private bool             _AutoRejoin;
        private Dictionary<string, string> _AutoRejoinChannels = new Dictionary<string, string>();
        private bool             _AutoRejoinOnKick;
        private bool             _AutoRelogin;
        private bool             _AutoNickHandling        = true;
        private bool             _SupportNonRfc;
        private bool             _SupportNonRfcLocked;
        private StringCollection _Motd                    = new StringCollection();
        private bool             _MotdReceived;
        private Array            _ReplyCodes              = Enum.GetValues(typeof(ReplyCode));
        private StringCollection _JoinedChannels          = new StringCollection();
        private Hashtable        _Channels                = Hashtable.Synchronized(new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer()));
        private Hashtable        _IrcUsers                = Hashtable.Synchronized(new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer()));
        private List<ChannelInfo> _ChannelList;
        private Object            _ChannelListSyncRoot = new Object();
        private AutoResetEvent    _ChannelListReceivedEvent;
        private List<WhoInfo>    _WhoList;
        private Object           _WhoListSyncRoot = new Object();
        private AutoResetEvent   _WhoListReceivedEvent;
        private List<BanInfo>    _BanList;
        private Object           _BanListSyncRoot = new Object();
        private AutoResetEvent   _BanListReceivedEvent;

        public event EventHandler               OnRegistered;
        public event EventHandler<PingEventArgs>           OnPing;
        public event EventHandler<PongEventArgs>           OnPong;
        public event EventHandler<IrcEventArgs>            OnRawMessage;
        public event EventHandler<ErrorEventArgs>          OnError;
        public event EventHandler<IrcEventArgs>            OnErrorMessage;
        public event EventHandler<JoinEventArgs>           OnJoin;
        public event EventHandler<NamesEventArgs>          OnNames;
        public event EventHandler<ListEventArgs>           OnList;
        public event EventHandler<PartEventArgs>           OnPart;
        public event EventHandler<QuitEventArgs>           OnQuit;
        public event EventHandler<KickEventArgs>           OnKick;
        public event EventHandler<AwayEventArgs>           OnAway;
        public event EventHandler<IrcEventArgs>            OnUnAway;
        public event EventHandler<IrcEventArgs>            OnNowAway;
        public event EventHandler<InviteEventArgs>         OnInvite;
        public event EventHandler<BanEventArgs>            OnBan;
        public event EventHandler<UnbanEventArgs>          OnUnban;
        public event EventHandler<OpEventArgs>             OnOp;
        public event EventHandler<DeopEventArgs>           OnDeop;
        public event EventHandler<HalfopEventArgs>         OnHalfop;
        public event EventHandler<DehalfopEventArgs>       OnDehalfop;
        public event EventHandler<VoiceEventArgs>          OnVoice;
        public event EventHandler<DevoiceEventArgs>        OnDevoice;
        public event EventHandler<WhoEventArgs>            OnWho;
        public event EventHandler<MotdEventArgs>           OnMotd;
        public event EventHandler<TopicEventArgs>          OnTopic;
        public event EventHandler<TopicChangeEventArgs>    OnTopicChange;
        public event EventHandler<NickChangeEventArgs>     OnNickChange;
        public event EventHandler<IrcEventArgs>            OnModeChange;
        public event EventHandler<IrcEventArgs>            OnUserModeChange;
        public event EventHandler<IrcEventArgs>            OnChannelModeChange;
        public event EventHandler<IrcEventArgs>            OnChannelMessage;
        public event EventHandler<ActionEventArgs>         OnChannelAction;
        public event EventHandler<IrcEventArgs>            OnChannelNotice;
        public event EventHandler<IrcEventArgs>            OnChannelActiveSynced;
        public event EventHandler<IrcEventArgs>            OnChannelPassiveSynced;
        public event EventHandler<IrcEventArgs>            OnQueryMessage;
        public event EventHandler<ActionEventArgs>         OnQueryAction;
        public event EventHandler<IrcEventArgs>            OnQueryNotice;
        public event EventHandler<CtcpEventArgs>           OnCtcpRequest;
        public event EventHandler<CtcpEventArgs>           OnCtcpReply;

        /// <summary>
        /// Enables/disables the active channel sync feature.
        /// Default: false
        /// </summary>
        public bool ActiveChannelSyncing {
            get {
                return _ActiveChannelSyncing;
            }
            set {
#if LOG4NET
                if (value) {
                    Logger.ChannelSyncing.Info("Active channel syncing enabled");
                } else {
                    Logger.ChannelSyncing.Info("Active channel syncing disabled");
                }
#endif
                _ActiveChannelSyncing = value;
            }
        }

        /// <summary>
        /// Enables/disables the passive channel sync feature. Not implemented yet!
        /// </summary>
        public bool PassiveChannelSyncing {
            get {
                return _PassiveChannelSyncing;
            }
            /*
            set {
#if LOG4NET
                if (value) {
                    Logger.ChannelSyncing.Info("Passive channel syncing enabled");
                } else {
                    Logger.ChannelSyncing.Info("Passive channel syncing disabled");
                }
#endif
                _PassiveChannelSyncing = value;
            }
            */
        }
        
        /// <summary>
        /// Sets the ctcp version that should be replied on ctcp version request.
        /// </summary>
        public string CtcpVersion {
            get {
                return _CtcpVersion;
            }
            set {
                _CtcpVersion = value;
            }
        }

        /// <summary>
        /// Enables/disables auto joining of channels when invited.
        /// Default: false
        /// </summary>
        public bool AutoJoinOnInvite {
            get {
                return _AutoJoinOnInvite;
            }
            set {
#if LOG4NET
                if (value) {
                    Logger.ChannelSyncing.Info("AutoJoinOnInvite enabled");
                } else {
                    Logger.ChannelSyncing.Info("AutoJoinOnInvite disabled");
                }
#endif
                _AutoJoinOnInvite = value;
            }
        }

        /// <summary>
        /// Enables/disables automatic rejoining of channels when a connection to the server is lost.
        /// Default: false
        /// </summary>
        public bool AutoRejoin {
            get {
                return _AutoRejoin;
            }
            set {
#if LOG4NET
                if (value) {
                    Logger.ChannelSyncing.Info("AutoRejoin enabled");
                } else {
                    Logger.ChannelSyncing.Info("AutoRejoin disabled");
                }
#endif
                _AutoRejoin = value;
            }
        }
        
        /// <summary>
        /// Enables/disables auto rejoining of channels when kicked.
        /// Default: false
        /// </summary>
        public bool AutoRejoinOnKick {
            get {
                return _AutoRejoinOnKick;
            }
            set {
#if LOG4NET
                if (value) {
                    Logger.ChannelSyncing.Info("AutoRejoinOnKick enabled");
                } else {
                    Logger.ChannelSyncing.Info("AutoRejoinOnKick disabled");
                }
#endif
                _AutoRejoinOnKick = value;
            }
        }

        /// <summary>
        /// Enables/disables auto relogin to the server after a reconnect.
        /// Default: false
        /// </summary>
        public bool AutoRelogin {
            get {
                return _AutoRelogin;
            }
            set {
#if LOG4NET
                if (value) {
                    Logger.ChannelSyncing.Info("AutoRelogin enabled");
                } else {
                    Logger.ChannelSyncing.Info("AutoRelogin disabled");
                }
#endif
                _AutoRelogin = value;
            }
        }

        /// <summary>
        /// Enables/disables auto nick handling on nick collisions
        /// Default: true
        /// </summary>
        public bool AutoNickHandling {
            get {
                return _AutoNickHandling;
            }
            set {
#if LOG4NET
                if (value) {
                    Logger.ChannelSyncing.Info("AutoNickHandling enabled");
                } else {
                    Logger.ChannelSyncing.Info("AutoNickHandling disabled");
                }
#endif
                _AutoNickHandling = value;
            }
        }
        
        /// <summary>
        /// Enables/disables support for non rfc features.
        /// Default: false
        /// </summary>
        public bool SupportNonRfc {
            get {
                return _SupportNonRfc;
            }
            set {
                if (_SupportNonRfcLocked) {
                    return;
                }
#if LOG4NET
                
                if (value) {
                    Logger.ChannelSyncing.Info("SupportNonRfc enabled");
                } else {
                    Logger.ChannelSyncing.Info("SupportNonRfc disabled");
                }
#endif
                _SupportNonRfc = value;
            }
        }

        /// <summary>
        /// Gets the nickname of us.
        /// </summary>
        public string Nickname {
            get {
                return _Nickname;
            }
        }
        
        /// <summary>
        /// Gets the list of nicknames of us.
        /// </summary>
        public string[] NicknameList {
            get {
                return _NicknameList;
            }
        }
      
        /// <summary>
        /// Gets the supposed real name of us.
        /// </summary>
        public string Realname {
            get {
                return _Realname;
            }
        }

        /// <summary>
        /// Gets the username for the server.
        /// </summary>
        /// <remarks>
        /// System username is set by default 
        /// </remarks>
        public string Username {
            get {
                return _Username;
            }
        }

        /// <summary>
        /// Gets the alphanumeric mode mask of us.
        /// </summary>
        public string Usermode {
            get {
                return _Usermode;
            }
        }

        /// <summary>
        /// Gets the numeric mode mask of us.
        /// </summary>
        public int IUsermode {
            get {
                return _IUsermode;
            }
        }

        /// <summary>
        /// Gets SASL account name
        /// </summary>
        public string SaslAccount {
            get {
                return _SaslAccount;
            }
            set {
                _SaslAccount = value;
            }
        }

        /// <summary>
        /// Gets SASL password
        /// </summary>
        public string SaslPassword {
            get {
                return _SaslPassword;
            }
            set {
                _SaslPassword = value;
            }
        }

        /// <summary>
        /// Returns if we are away on this connection
        /// </summary>
        public bool IsAway {
            get {
                return _IsAway;
            }
        }
        
        /// <summary>
        /// Gets the password for the server.
        /// </summary>
        public string Password {
            get {
                return _Password;
            }
        }

        /// <summary>
        /// Gets the list of channels we are joined.
        /// </summary>
        public StringCollection JoinedChannels {
            get {
                return _JoinedChannels;
            }
        }
        
        /// <summary>
        /// Gets the server message of the day.
        /// </summary>
        public StringCollection Motd {
            get {
                return _Motd;
            }
        }

        public object BanListSyncRoot {
            get {
                return _BanListSyncRoot;
            }
        }
        
        /// <summary>
        /// This class manages the connection server and provides access to all the objects needed to send and receive messages.
        /// </summary>
        public IrcClient()
        {
#if LOG4NET
            Logger.Main.Debug("IrcClient created");
#endif
            OnReadLine        += new EventHandler<ReadLineEventArgs>(_Worker);
            OnDisconnected    += new EventHandler(_OnDisconnected);
            OnConnectionError += new EventHandler(_OnConnectionError);
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
            _SupportNonRfcLocked = true;
            base.Connect(addresslist, port);
        }
        
        /// <overloads>
        /// Reconnects to the current server.
        /// </overloads>
        /// <param name="login">If the login data should be sent, after successful connect.</param>
        /// <param name="channels">If the channels should be rejoined, after successful connect.</param>
        public void Reconnect(bool login, bool channels)
        {
            if (channels) {
                _StoreChannelsToRejoin();
            }
            base.Reconnect();
            if (login) {
                //reset the nick to the original nicklist
                _CurrentNickname = 0;
                // FIXME: honor _Nickname (last used nickname)
                Login(_NicknameList, Realname, IUsermode, Username, Password);
            }
            if (channels) {
                _RejoinChannels();
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
            _NicknameList = (string[])nicklist.Clone();
            // here we set the nickname which we will try first
            _Nickname = _NicknameList[0].Replace(" ", "");
            _Realname = realname;
            _IUsermode = usermode;

            if (username != null && username.Length > 0) {
                _Username = username.Replace(" ", "");
            } else {
                _Username = Environment.UserName.Replace(" ", "");
            }
            
            if (_SaslAccount != "")
                Cap (CapabilitySubcommand.LS, Priority.Critical);

            if (password != null && password.Length > 0) {
                _Password = password;
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
            Login(new string[] {nick, nick+"_", nick+"__"}, realname, usermode, username, password);
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
            Login(new string[] {nick, nick+"_", nick+"__"}, realname, usermode, username, "");
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
            Login(new string[] {nick, nick+"_", nick+"__"}, realname, usermode, "", "");
        }

        /// <summary>
        /// Login parameters required identify with server connection
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realname of a new user.</remark>
        /// <param name="nick">The users 'nick' name which may NOT contain spaces</param>
        /// <param name="realname">The users 'real' name which may contain space characters</param>
        public void Login(string nick, string realname)
        {
            Login(new string[] {nick, nick+"_", nick+"__"}, realname, 0, "", "");
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
            if (channelname == null) {
                throw new System.ArgumentNullException("channelname");
            }

            if (nickname == null) {
                throw new System.ArgumentNullException("nickname");
            }
            
            Channel channel = GetChannel(channelname);
            if (channel != null &&
                channel.UnsafeUsers != null &&
                channel.UnsafeUsers.ContainsKey(nickname)) {
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
            if (nickname == null) {
                throw new System.ArgumentNullException("nickname");
            }

            return (IrcUser)_IrcUsers[nickname];
        }

        /// <summary>
        /// Returns extended user information including channel information
        /// </summary>
        /// <param name="channelname">The name of the channel you wish to query</param>
        /// <param name="nickname">The users 'nick' name which may NOT contain spaces</param>
        /// <returns>ChannelUser object of requested channelname/nickname</returns>
        public ChannelUser GetChannelUser(string channelname, string nickname)
        {
            if (channelname == null) {
                throw new System.ArgumentNullException("channel");
            }

            if (nickname == null) {
                throw new System.ArgumentNullException("nickname");
            }
            
            Channel channel = GetChannel(channelname);
            if (channel != null) {
                return (ChannelUser)channel.UnsafeUsers[nickname];
            } else {
                return null;
            } 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channelname">The name of the channel you wish to query</param>
        /// <returns>Channel object of requested channel</returns>
        public Channel GetChannel(string channelname)
        {
            if (channelname == null) {
                throw new System.ArgumentNullException("channelname");
            }
            
            return (Channel)_Channels[channelname];
        }

        /// <summary>
        /// Gets a list of all joined channels on server
        /// </summary>
        /// <returns>String array of all joined channel names</returns>
        public string[] GetChannels()
        {
            string[] channels = new string[_Channels.Values.Count];
            int i = 0;
            foreach (Channel channel in _Channels.Values) {
                channels[i++] = channel.Name;
            }

            return channels;
        }
        
        /// <summary>
        /// Fetches a fresh list of all available channels that match the passed mask
        /// </summary>
        /// <returns>List of ListInfo</returns>
        public IList<ChannelInfo> GetChannelList(string mask)
        {
            List<ChannelInfo> list = new List<ChannelInfo>();
            lock (_ChannelListSyncRoot) {
                _ChannelList = list;
                _ChannelListReceivedEvent = new AutoResetEvent(false);
                
                // request list
                RfcList(mask);
                // wait till we have the complete list
                _ChannelListReceivedEvent.WaitOne();
                
                _ChannelListReceivedEvent = null;
                _ChannelList = null;
            }
            
            return list;
        }
        
        /// <summary>
        /// Fetches a fresh list of users that matches the passed mask
        /// </summary>
        /// <returns>List of ListInfo</returns>
        public IList<WhoInfo> GetWhoList(string mask)
        {
            List<WhoInfo> list = new List<WhoInfo>();
            lock (_WhoListSyncRoot) {
                _WhoList = list;
                _WhoListReceivedEvent = new AutoResetEvent(false);
                
                // request list
                RfcWho(mask);
                // wait till we have the complete list
                _WhoListReceivedEvent.WaitOne();
                
                _WhoListReceivedEvent = null;
                _WhoList = null;
            }
            
            return list;
        }
        
        /// <summary>
        /// Fetches a fresh ban list of the specified channel
        /// </summary>
        /// <returns>List of ListInfo</returns>
        public IList<BanInfo> GetBanList(string channel)
        {
            List<BanInfo> list = new List<BanInfo>();
            lock (_BanListSyncRoot) {
                _BanList = list;
                _BanListReceivedEvent = new AutoResetEvent(false);
                
                // request list
                Ban(channel);
                // wait till we have the complete list
                _BanListReceivedEvent.WaitOne();
                
                _BanListReceivedEvent = null;
                _BanList = null;
            }
            
            return list;
        }
        
        protected virtual IrcUser CreateIrcUser(string nickname)
        {
             return new IrcUser(nickname, this);
        }
        
        protected virtual Channel CreateChannel(string name)
        {
            if (_SupportNonRfc) {
                return new NonRfcChannel(name);
            } else {
                return new Channel(name);
            }
        }
        
        protected virtual ChannelUser CreateChannelUser(string channel, IrcUser ircUser)
        {
            if (_SupportNonRfc) {
                return new NonRfcChannelUser(channel, ircUser);
            } else {
                return new ChannelUser(channel, ircUser);
            }
        }
        
        private void _Worker(object sender, ReadLineEventArgs e)
        {
            var msg = new IrcMessageData (this, e.Line);

#if LOG4NET
            if (msg.Type == ReceiveType.Unknown)
                Logger.MessageTypes.Warn("Message not understood: \""+e.Line+"\"");
#endif
      
            // lets see if we have events or internal messagehandler for it
            _HandleEvents(msg);
        }

        private void _OnDisconnected(object sender, EventArgs e)
        {
            if (AutoRejoin) {
                _StoreChannelsToRejoin();
            }
            _SyncingCleanup();
        }
        
        private void _OnConnectionError(object sender, EventArgs e)
        {
            try {
                // AutoReconnect is handled in IrcConnection._OnConnectionError
                if (AutoReconnect && AutoRelogin) {
                    Login(_NicknameList, Realname, IUsermode, Username, Password);
                }
                if (AutoReconnect && AutoRejoin) {
                    _RejoinChannels();
                }
            } catch (NotConnectedException) {
                // HACK: this is hacky, we don't know if the Reconnect was actually successful
                // means sending IRC commands without a connection throws NotConnectedExceptions 
            }
        }
        
        private void _StoreChannelsToRejoin()
        {
#if LOG4NET
            Logger.Connection.Info("Storing channels for rejoin...");
#endif
            lock (_AutoRejoinChannels) {
                _AutoRejoinChannels.Clear();
                if (ActiveChannelSyncing || PassiveChannelSyncing) {
                    // store the key using channel sync
                    foreach (Channel channel in _Channels.Values) {
                        _AutoRejoinChannels.Add(channel.Name, channel.Key);
                    }
                } else {
                    foreach (string channel in _JoinedChannels) {
                        _AutoRejoinChannels.Add(channel, null);
                    }
                }
            }
        }
        
        private void _RejoinChannels()
        {
#if LOG4NET
            Logger.Connection.Info("Rejoining channels...");
#endif
            lock (_AutoRejoinChannels) {
                RfcJoin(_AutoRejoinChannels.Keys.ToArray(),
                        _AutoRejoinChannels.Values.ToArray(),
                        Priority.High);
                _AutoRejoinChannels.Clear();
            }
        }
        
        private void _SyncingCleanup()
        {
            // lets clean it baby, powered by Mr. Proper
#if LOG4NET
            Logger.ChannelSyncing.Debug("Mr. Proper action, cleaning good...");
#endif
            _JoinedChannels.Clear();
            if (ActiveChannelSyncing) {
                _Channels.Clear();
                _IrcUsers.Clear();
            }
            
            _IsAway = false;
            
            _MotdReceived = false;
            _Motd.Clear();
        }
       
        /// <summary>
        /// 
        /// </summary>
        private string _NextNickname()
        {
            _CurrentNickname++;
            //if we reach the end stay there
            if (_CurrentNickname >= _NicknameList.Length) {
                _CurrentNickname--;
            }
            return NicknameList[_CurrentNickname];
        }
        
        private void _HandleEvents(IrcMessageData ircdata)
        {
            if (OnRawMessage != null) {
                OnRawMessage(this, new IrcEventArgs(ircdata));
            }

            switch (ircdata.Command) {
                case "PING":
                    _Event_PING(ircdata);
                break;
                case "ERROR":
                    _Event_ERROR(ircdata);
                break;
                case "PRIVMSG":
                    _Event_PRIVMSG(ircdata);
                break;
                case "NOTICE":
                    _Event_NOTICE(ircdata);
                break;
                case "JOIN":
                    _Event_JOIN(ircdata);
                break;
                case "PART":
                    _Event_PART(ircdata);
                break;
                case "KICK":
                    _Event_KICK(ircdata);
                break;
                case "QUIT":
                    _Event_QUIT(ircdata);
                break;
                case "TOPIC":
                    _Event_TOPIC(ircdata);
                break;
                case "NICK":
                    _Event_NICK(ircdata);
                break;
                case "INVITE":
                    _Event_INVITE(ircdata);
                break;
                case "MODE":
                    _Event_MODE(ircdata);
                break;
                case "PONG":
                    _Event_PONG(ircdata);
                break;
                case "CAP":
                    _Event_CAP(ircdata);
                break;
                case "AUTHENTICATE":
                    _Event_AUTH(ircdata);
                break;
            }

            if (ircdata.ReplyCode != ReplyCode.Null) {
                switch (ircdata.ReplyCode) {
                    case ReplyCode.Welcome:
                        _Event_RPL_WELCOME(ircdata);
                        break;
                    case ReplyCode.Topic:
                        _Event_RPL_TOPIC(ircdata);
                        break;
                    case ReplyCode.NoTopic:
                        _Event_RPL_NOTOPIC(ircdata);
                        break;
                    case ReplyCode.NamesReply:
                        _Event_RPL_NAMREPLY(ircdata);
                        break;
                    case ReplyCode.EndOfNames:
                        _Event_RPL_ENDOFNAMES(ircdata);
                        break;
                    case ReplyCode.List:
                        _Event_RPL_LIST(ircdata);
                        break;
                    case ReplyCode.ListEnd:
                        _Event_RPL_LISTEND(ircdata);
                        break;
                    case ReplyCode.WhoReply:
                        _Event_RPL_WHOREPLY(ircdata);
                        break;
                    case ReplyCode.EndOfWho:
                        _Event_RPL_ENDOFWHO(ircdata);
                        break;
                    case ReplyCode.ChannelModeIs:
                        _Event_RPL_CHANNELMODEIS(ircdata);
                        break;
                    case ReplyCode.BanList:
                        _Event_RPL_BANLIST(ircdata);
                        break;
                    case ReplyCode.EndOfBanList:
                        _Event_RPL_ENDOFBANLIST(ircdata);
                        break;
                    case ReplyCode.ErrorNoChannelModes:
                        _Event_ERR_NOCHANMODES(ircdata);
                        break;
                    case ReplyCode.Motd:
                        _Event_RPL_MOTD(ircdata);
                        break;
                    case ReplyCode.EndOfMotd:
                        _Event_RPL_ENDOFMOTD(ircdata);
                        break;
                    case ReplyCode.Away:
                        _Event_RPL_AWAY(ircdata);
                        break;
                    case ReplyCode.UnAway:
                        _Event_RPL_UNAWAY(ircdata);
                        break;
                    case ReplyCode.NowAway:
                        _Event_RPL_NOWAWAY(ircdata);
                        break;
                    case ReplyCode.TryAgain:
                        _Event_RPL_TRYAGAIN(ircdata);
                        break;
                    case ReplyCode.ErrorNicknameInUse:
                        _Event_ERR_NICKNAMEINUSE(ircdata);
                        break;
                    case ReplyCode.SaslSuccess:
                    case ReplyCode.SaslFailure1:
                    case ReplyCode.SaslFailure2:
                    case ReplyCode.SaslAbort:
                        _Event_RPL_SASL(ircdata);
                        break;
                }
            }
            
            if (ircdata.Type == ReceiveType.ErrorMessage) {
                _Event_ERR(ircdata);
            }
        }

        /// <summary>
        /// Removes a specified user from all channel lists
        /// </summary>
        /// <param name="nickname">The users 'nick' name which may NOT contain spaces</param>
        private bool _RemoveIrcUser(string nickname)
        {
            if (GetIrcUser(nickname).JoinedChannels.Length == 0) {
                // he is nowhere else, lets kill him
                _IrcUsers.Remove(nickname);
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Removes a specified user from a specified channel list
        /// </summary>
        /// <param name="channelname">The name of the channel you wish to query</param>
        /// <param name="nickname">The users 'nick' name which may NOT contain spaces</param>
        private void _RemoveChannelUser(string channelname, string nickname)
        {
            Channel chan = GetChannel(channelname);
            chan.UnsafeUsers.Remove(nickname);
            chan.UnsafeOps.Remove(nickname);
            chan.UnsafeVoices.Remove(nickname);
            if (SupportNonRfc) {
                NonRfcChannel nchan = (NonRfcChannel)chan;
                nchan.UnsafeHalfops.Remove(nickname);
            } 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ircdata">Message data containing channel mode information</param>
        /// <param name="mode">Channel mode</param>
        /// <param name="parameter">List of supplied paramaters</param>
        private void _InterpretChannelMode(IrcMessageData ircdata, string mode, string parameter)
        {
            string[] parameters = parameter.Split(new char[] {' '});
            bool add       = false;
            bool remove    = false;
            int modelength = mode.Length;
            string temp;
            Channel channel = null;
            if (ActiveChannelSyncing) {
                channel = GetChannel(ircdata.Channel);
            }
            IEnumerator parametersEnumerator = parameters.GetEnumerator();
            // bring the enumerator to the 1. element
            parametersEnumerator.MoveNext();
            for (int i = 0; i < modelength; i++) {
                switch(mode[i]) {
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
                        
                        if (add) {
                            if (ActiveChannelSyncing) {
                                // sanity check
                                if (GetChannelUser(ircdata.Channel, temp) != null) {
                                    // update the op list
                                    try {
                                        channel.UnsafeOps.Add(temp, GetIrcUser(temp));
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("added op: "+temp+" to: "+ircdata.Channel);
#endif
                                    } catch (ArgumentException) {
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("duplicate op: "+temp+" in: "+ircdata.Channel+" not added");
#endif
                                    }
                                    
                                    // update the user op status
                                    ChannelUser cuser = GetChannelUser(ircdata.Channel, temp);
                                    cuser.IsOp = true;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("set op status: " + temp + " for: "+ircdata.Channel);
#endif
                                } else {
#if LOG4NET
                                    Logger.ChannelSyncing.Error("_InterpretChannelMode(): GetChannelUser(" + ircdata.Channel + "," + temp + ") returned null! Ignoring...");
#endif
                                }
                            }
                            
                            if (OnOp != null) {
                                OnOp(this, new OpEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                            }
                        }
                        if (remove) {
                            if (ActiveChannelSyncing) {
                                // sanity check
                                if (GetChannelUser(ircdata.Channel, temp) != null) {
                                    // update the op list
                                    channel.UnsafeOps.Remove(temp);
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("removed op: "+temp+" from: "+ircdata.Channel);
#endif
                                    // update the user op status
                                    ChannelUser cuser = GetChannelUser(ircdata.Channel, temp);
                                    cuser.IsOp = false;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("unset op status: " + temp + " for: "+ircdata.Channel);
#endif
                                } else {
#if LOG4NET
                                    Logger.ChannelSyncing.Error("_InterpretChannelMode(): GetChannelUser(" + ircdata.Channel + "," + temp + ") returned null! Ignoring...");
#endif
                                }
                            }
                            
                            if (OnDeop != null) {
                                OnDeop(this, new DeopEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                            }
                        }
                    break;
                    case 'h':
                        if (SupportNonRfc) {
                            temp = (string)parametersEnumerator.Current;
                            parametersEnumerator.MoveNext();
                            
                            if (add) {
                                if (ActiveChannelSyncing) {
                                    // sanity check
                                    if (GetChannelUser(ircdata.Channel, temp) != null) {
                                        // update the halfop list
                                        try {
                                            ((NonRfcChannel)channel).UnsafeHalfops.Add(temp, GetIrcUser(temp));
#if LOG4NET
                                            Logger.ChannelSyncing.Debug("added halfop: "+temp+" to: "+ircdata.Channel);
#endif
                                        } catch (ArgumentException) {
#if LOG4NET
                                            Logger.ChannelSyncing.Debug("duplicate halfop: "+temp+" in: "+ircdata.Channel+" not added");
#endif
                                        }
                                        
                                        // update the user halfop status
                                        NonRfcChannelUser cuser = (NonRfcChannelUser)GetChannelUser(ircdata.Channel, temp);
                                        cuser.IsHalfop = true;
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("set halfop status: " + temp + " for: "+ircdata.Channel);
#endif
                                    } else {
#if LOG4NET
                                        Logger.ChannelSyncing.Error("_InterpretChannelMode(): GetChannelUser(" + ircdata.Channel + "," + temp + ") returned null! Ignoring...");
#endif
                                    }
                                }
                                
                                if (OnHalfop != null) {
                                    OnHalfop(this, new HalfopEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                                }
                            }
                            if (remove) {
                                if (ActiveChannelSyncing) {
                                    // sanity check
                                    if (GetChannelUser(ircdata.Channel, temp) != null) {
                                        // update the halfop list
                                        ((NonRfcChannel)channel).UnsafeHalfops.Remove(temp);
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("removed halfop: "+temp+" from: "+ircdata.Channel);
#endif
                                        // update the user halfop status
                                        NonRfcChannelUser cuser = (NonRfcChannelUser)GetChannelUser(ircdata.Channel, temp);
                                        cuser.IsHalfop = false;
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("unset halfop status: " + temp + " for: "+ircdata.Channel);
#endif
                                    } else {
#if LOG4NET
                                        Logger.ChannelSyncing.Error("_InterpretChannelMode(): GetChannelUser(" + ircdata.Channel + "," + temp + ") returned null! Ignoring...");
#endif
                                    }
                                }
                                
                                if (OnDehalfop != null) {
                                    OnDehalfop(this, new DehalfopEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                                }
                            }
                        }
                    break;
                    case 'v':
                        temp = (string)parametersEnumerator.Current;
                        parametersEnumerator.MoveNext();
                        
                        if (add) {
                            if (ActiveChannelSyncing) {
                                // sanity check
                                if (GetChannelUser(ircdata.Channel, temp) != null) {
                                    // update the voice list
                                    try {
                                        channel.UnsafeVoices.Add(temp, GetIrcUser(temp));
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("added voice: "+temp+" to: "+ircdata.Channel);
#endif
                                    } catch (ArgumentException) {
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("duplicate voice: "+temp+" in: "+ircdata.Channel+" not added");
#endif
                                    }
                                    
                                    // update the user voice status
                                    ChannelUser cuser = GetChannelUser(ircdata.Channel, temp);
                                    cuser.IsVoice = true;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("set voice status: " + temp + " for: "+ircdata.Channel);
#endif
                                } else {
#if LOG4NET
                                    Logger.ChannelSyncing.Error("_InterpretChannelMode(): GetChannelUser(" + ircdata.Channel + "," + temp + ") returned null! Ignoring...");
#endif
                                }
                            }
                            
                            if (OnVoice != null) {
                                OnVoice(this, new VoiceEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                            }
                        }
                        if (remove) {
                            if (ActiveChannelSyncing) {
                                // sanity check
                                if (GetChannelUser(ircdata.Channel, temp) != null) {
                                    // update the voice list
                                    channel.UnsafeVoices.Remove(temp);
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("removed voice: "+temp+" from: "+ircdata.Channel);
#endif
                                    // update the user voice status
                                    ChannelUser cuser = GetChannelUser(ircdata.Channel, temp);
                                    cuser.IsVoice = false;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("unset voice status: " + temp + " for: "+ircdata.Channel);
#endif
                                } else {
#if LOG4NET
                                    Logger.ChannelSyncing.Error("_InterpretChannelMode(): GetChannelUser(" + ircdata.Channel + "," + temp + ") returned null! Ignoring...");
#endif
                                }
                            }
                            
                            if (OnDevoice != null) {
                                OnDevoice(this, new DevoiceEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                            }
                        }
                    break;
                    case 'b':
                        temp = (string)parametersEnumerator.Current;
                        parametersEnumerator.MoveNext();
                        if (add) {
                            if (ActiveChannelSyncing) {
                                try {
                                    channel.Bans.Add(temp);
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("added ban: "+temp+" to: "+ircdata.Channel);
#endif
                                } catch (ArgumentException) {
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("duplicate ban: "+temp+" in: "+ircdata.Channel+" not added");
#endif
                                }
                            }
                            if (OnBan != null) {
                               OnBan(this, new BanEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                            }
                        }
                        if (remove) {
                            if (ActiveChannelSyncing) {
                                channel.Bans.Remove(temp);
#if LOG4NET
                                Logger.ChannelSyncing.Debug("removed ban: "+temp+" from: "+ircdata.Channel);
#endif
                            }
                            if (OnUnban != null) {
                                OnUnban(this, new UnbanEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                            }
                        }
                    break;
                    case 'l':
                        temp = (string)parametersEnumerator.Current;
                        parametersEnumerator.MoveNext();
                        if (add) {
                            if (ActiveChannelSyncing) {
                                try {
                                    channel.UserLimit = int.Parse(temp);
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("stored user limit for: "+ircdata.Channel);
#endif
                                } catch (FormatException) {
#if LOG4NET
                                    Logger.ChannelSyncing.Error("could not parse user limit: "+temp+" channel: "+ircdata.Channel);
#endif
                                }
                            }
                        }
                        if (remove) {
                            if (ActiveChannelSyncing) {
                                channel.UserLimit = 0;
#if LOG4NET
                                Logger.ChannelSyncing.Debug("removed user limit for: "+ircdata.Channel);
#endif
                            }
                        }
                    break;
                        case 'k':
                            temp = (string)parametersEnumerator.Current;
                            parametersEnumerator.MoveNext();
                            if (add) {
                                if (ActiveChannelSyncing) {
                                    channel.Key = temp;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("stored channel key for: "+ircdata.Channel);
#endif
                                }
                            }
                            if (remove) {
                                if (ActiveChannelSyncing) {
                                    channel.Key = "";
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("removed channel key for: "+ircdata.Channel);
#endif
                                }
                            }
                        break;
                        default:
                            if (add) {
                                if (ActiveChannelSyncing) {
                                    if (channel.Mode.IndexOf(mode[i]) == -1) {
                                        channel.Mode += mode[i];
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("added channel mode ("+mode[i]+") for: "+ircdata.Channel);
#endif
                                    }
                                }
                            }
                            if (remove) {
                                if (ActiveChannelSyncing) {
                                    channel.Mode = channel.Mode.Replace(mode[i].ToString(), String.Empty);
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("removed channel mode ("+mode[i]+") for: "+ircdata.Channel);
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
        private void _Event_PING(IrcMessageData ircdata)
        {
            string server = ircdata.RawMessageArray[1].Substring(1);
#if LOG4NET
            Logger.Connection.Debug("Ping? Pong!");
#endif
            RfcPong(server, Priority.Critical);

            if (OnPing != null) {
                OnPing(this, new PingEventArgs(ircdata, server));
            }
        }

        /// <summary>
        /// Event handler for PONG messages
        /// </summary>
        /// <param name="ircdata">Message data containing pong information</param>
        private void _Event_PONG(IrcMessageData ircdata)     
        {
            if (OnPong != null) {
                OnPong(this, new PongEventArgs(ircdata, ircdata.Irc.Lag));
            }
        }

        /// <summary>
        /// Event handler for error messages
        /// </summary>
        /// <param name="ircdata">Message data containing error information</param>
        private void _Event_ERROR(IrcMessageData ircdata)
        {
            string message = ircdata.Message;
#if LOG4NET
            Logger.Connection.Info("received ERROR from IRC server");
#endif

            if (OnError != null) {
                OnError(this, new ErrorEventArgs(ircdata, message));
            }
        }

        /// <summary>
        /// Event handler for join messages
        /// </summary>
        /// <param name="ircdata">Message data containing join information</param>
        private void _Event_JOIN(IrcMessageData ircdata)
        {
            string who = ircdata.Nick;
            string channelname = ircdata.Channel;

            if (IsMe(who)) {
                _JoinedChannels.Add(channelname);
            }

            if (ActiveChannelSyncing) {
                Channel channel;
                if (IsMe(who)) {
                    // we joined the channel
#if LOG4NET
                    Logger.ChannelSyncing.Debug("joining channel: "+channelname);
#endif
                    channel = CreateChannel(channelname);
                    _Channels.Add(channelname, channel);
                    // request channel mode
                    RfcMode(channelname);
                    // request wholist
                    RfcWho(channelname);
                    // request banlist
                    Ban(channelname);
                } else {
                    // someone else joined the channel
                    // request the who data
                    RfcWho(who);
                }

#if LOG4NET
                Logger.ChannelSyncing.Debug(who+" joins channel: "+channelname);
#endif
                channel = GetChannel(channelname);
                IrcUser ircuser = GetIrcUser(who);

                if (ircuser == null) {
                    ircuser = new IrcUser(who, this);
                    ircuser.Ident = ircdata.Ident;
                    ircuser.Host  = ircdata.Host;
                    _IrcUsers.Add(who, ircuser);
                }

                // HACK: IRCnet's anonymous channel mode feature breaks our
                // channnel sync here as they use the same nick for ALL channel
                // users!
                // Example: :anonymous!anonymous@anonymous. JOIN :$channel
                if (who == "anonymous" &&
                    ircdata.Ident == "anonymous" &&
                    ircdata.Host == "anonymous." &&
                    IsJoined(channelname, who)) {
                    // ignore
                } else {
                    ChannelUser channeluser = CreateChannelUser(channelname, ircuser);
                    channel.UnsafeUsers.Add(who, channeluser);
                }
            }

            if (OnJoin != null) {
                OnJoin(this, new JoinEventArgs(ircdata, channelname, who));
            }
        }

        /// <summary>
        /// Event handler for part messages
        /// </summary>
        /// <param name="ircdata">Message data containing part information</param>
        private void _Event_PART(IrcMessageData ircdata)
        {
            string who = ircdata.Nick;
            string channel = ircdata.Channel;
            string partmessage = ircdata.Message;

            if (IsMe(who)) {
                _JoinedChannels.Remove(channel);
            }

            if (ActiveChannelSyncing) {
                if (IsMe(who)) {
#if LOG4NET
                    Logger.ChannelSyncing.Debug("parting channel: "+channel);
#endif
                    _Channels.Remove(channel);
                } else {
#if LOG4NET
                    Logger.ChannelSyncing.Debug(who+" parts channel: "+channel);
#endif
                    // HACK: IRCnet's anonymous channel mode feature breaks our
                    // channnel sync here as they use the same nick for ALL channel
                    // users!
                    // Example: :anonymous!anonymous@anonymous. PART $channel :$msg
                    if (who == "anonymous" &&
                        ircdata.Ident == "anonymous" &&
                        ircdata.Host == "anonymous." &&
                        !IsJoined(channel, who)) {
                        // ignore
                    } else {
                        _RemoveChannelUser(channel, who);
                        _RemoveIrcUser(who);
                    }
                }
            }

            if (OnPart != null) {
                OnPart(this, new PartEventArgs(ircdata, channel, who, partmessage));
            }
        }

        /// <summary>
        /// Event handler for kick messages
        /// </summary>
        /// <param name="ircdata">Message data containing kick information</param>
        private void _Event_KICK(IrcMessageData ircdata)
        {
            string channelname = ircdata.Channel;
            string who = ircdata.Nick;
            string whom = ircdata.RawMessageArray[3];
            string reason = ircdata.Message;
            bool isme = IsMe(whom);          
            
            if (isme) {
                _JoinedChannels.Remove(channelname);
            }
            
            if (ActiveChannelSyncing) {
                if (isme) {
                    Channel channel = GetChannel(channelname);
                    _Channels.Remove(channelname);
                    if (_AutoRejoinOnKick) {
                        RfcJoin(channel.Name, channel.Key);
                    }
                } else {
                    _RemoveChannelUser(channelname, whom);
                    _RemoveIrcUser(whom);
                }
            } else {
                if (isme && AutoRejoinOnKick) {
                    RfcJoin(channelname);
                }
            }
            
            if (OnKick != null) {
                OnKick(this, new KickEventArgs(ircdata, channelname, who, whom, reason));
            }
        }

        /// <summary>
        /// Event handler for quit messages
        /// </summary>
        /// <param name="ircdata">Message data containing quit information</param>
        private void _Event_QUIT(IrcMessageData ircdata)
        {
            string who = ircdata.Nick;
            string reason = ircdata.Message;
            
            // no need to handle if we quit, disconnect event will take care
            
            if (ActiveChannelSyncing) {
                // sanity checks, freshirc is very broken about RFC
                IrcUser user = GetIrcUser(who);
                if (user != null) {
                    string[] joined_channels = user.JoinedChannels;
                    if (joined_channels != null) {
                        foreach (string channel in joined_channels) {
                            _RemoveChannelUser(channel, who);
                        }
                        _RemoveIrcUser(who);
#if LOG4NET
                    } else {
                        Logger.ChannelSyncing.Error("user.JoinedChannels (for: '"+who+"') returned null in _Event_QUIT! Ignoring...");
#endif
                    }
#if LOG4NET
                } else {
                    Logger.ChannelSyncing.Error("GetIrcUser("+who+") returned null in _Event_QUIT! Ignoring...");
#endif
                }
            }

            if (OnQuit != null) {
                OnQuit(this, new QuitEventArgs(ircdata, who, reason));
            }
        }

        /// <summary>
        /// Event handler for private messages
        /// </summary>
        /// <param name="ircdata">Message data containing private message information</param>
        private void _Event_PRIVMSG(IrcMessageData ircdata)
        {
        	
        	switch (ircdata.Type) {
                case ReceiveType.ChannelMessage:
                    if (OnChannelMessage != null) {
                        OnChannelMessage(this, new IrcEventArgs(ircdata));
                    }
                    break;
                case ReceiveType.ChannelAction:
                    if (OnChannelAction != null) {
                        string action = ircdata.Message.Substring(8, ircdata.Message.Length - 9);
                        OnChannelAction(this, new ActionEventArgs(ircdata, action));
                    }
                    break;
                case ReceiveType.QueryMessage:
                    if (OnQueryMessage != null) {
                        OnQueryMessage(this, new IrcEventArgs(ircdata));
                    }
                    break;
                case ReceiveType.QueryAction:
                    if (OnQueryAction != null) {
                        string action = ircdata.Message.Substring(8, ircdata.Message.Length - 9);
                        OnQueryAction(this, new ActionEventArgs(ircdata, action));
                    }
                    break;
                case ReceiveType.CtcpRequest:
                    if (OnCtcpRequest != null) {
                        int space_pos = ircdata.Message.IndexOf(' '); 
                        string cmd = "";
                        string param = "";
                        if (space_pos != -1) {
                            cmd = ircdata.Message.Substring(1, space_pos - 1);
                            param = ircdata.Message.Substring(space_pos + 1,
                                        ircdata.Message.Length - space_pos - 2);
                        } else {
                            cmd = ircdata.Message.Substring(1, ircdata.Message.Length - 2);
                        }
                        OnCtcpRequest(this, new CtcpEventArgs(ircdata, cmd, param));
                    }
                    break;
            }
        }

        /// <summary>
        /// Event handler for notice messages
        /// </summary>
        /// <param name="ircdata">Message data containing notice information</param>
        private void _Event_NOTICE(IrcMessageData ircdata)
        {
            switch (ircdata.Type) {
                case ReceiveType.ChannelNotice:
                    if (OnChannelNotice != null) {
                        OnChannelNotice(this, new IrcEventArgs(ircdata));
                    }
                    break;
                case ReceiveType.QueryNotice:
                    if (OnQueryNotice != null) {
                        OnQueryNotice(this, new IrcEventArgs(ircdata));
                    }
                    break;
                case ReceiveType.CtcpReply:
                    if (OnCtcpReply != null) {
                        int space_pos = ircdata.Message.IndexOf(' '); 
                        string cmd = "";
                        string param = "";
                        if (space_pos != -1) {
                            cmd = ircdata.Message.Substring(1, space_pos - 1);
                            param = ircdata.Message.Substring(space_pos + 1,
                                        ircdata.Message.Length - space_pos - 2);
                        } else {
                            cmd = ircdata.Message.Substring(1, ircdata.Message.Length - 2);
                        }
                        OnCtcpReply(this, new CtcpEventArgs(ircdata, cmd, param));
                    }
                    break;
            }
        }

        /// <summary>
        /// Event handler for topic messages
        /// </summary>
        /// <param name="ircdata">Message data containing topic information</param>
        private void _Event_TOPIC(IrcMessageData ircdata)
        {
            string who = ircdata.Nick;
            string channel = ircdata.Channel;
            string newtopic = ircdata.Message;

            if (ActiveChannelSyncing &&
                IsJoined(channel)) {
                GetChannel(channel).Topic = newtopic;
#if LOG4NET
                Logger.ChannelSyncing.Debug("stored topic for channel: "+channel);
#endif
            }

            if (OnTopicChange != null) {
                OnTopicChange(this, new TopicChangeEventArgs(ircdata, channel, who, newtopic));
            }
        }

        /// <summary>
        /// Event handler for nickname messages
        /// </summary>
        /// <param name="ircdata">Message data containing nickname information</param>
        private void _Event_NICK(IrcMessageData ircdata)
        {
            string oldnickname = ircdata.Nick;
            //string newnickname = ircdata.Message;
            // the colon in the NICK message is optional, thus we can't rely on Message
            string newnickname = ircdata.RawMessageArray[2];
            
            // so let's strip the colon if it's there
            if (newnickname.StartsWith(":")) {
                newnickname = newnickname.Substring(1);
            }
            
            if (IsMe(ircdata.Nick)) {
                // nickname change is your own
                _Nickname = newnickname;
            }

            if (ActiveChannelSyncing) {
                IrcUser ircuser = GetIrcUser(oldnickname);
                
                // if we don't have any info about him, don't update him!
                // (only queries or ourself in no channels)
                if (ircuser != null) {
                    string[] joinedchannels = ircuser.JoinedChannels;

                    // update his nickname
                    ircuser.Nick = newnickname;
                    // remove the old entry 
                    // remove first to avoid duplication, Foo -> foo
                    _IrcUsers.Remove(oldnickname);
                    // add him as new entry and new nickname as key
                    _IrcUsers.Add(newnickname, ircuser);
#if LOG4NET
                    Logger.ChannelSyncing.Debug("updated nickname of: "+oldnickname+" to: "+newnickname);
#endif
                    // now the same for all channels he is joined
                    Channel     channel;
                    ChannelUser channeluser;
                    foreach (string channelname in joinedchannels) {
                        channel     = GetChannel(channelname);
                        channeluser = GetChannelUser(channelname, oldnickname);
                        // remove first to avoid duplication, Foo -> foo
                        channel.UnsafeUsers.Remove(oldnickname);
                        channel.UnsafeUsers.Add(newnickname, channeluser);
                        if (channeluser.IsOp) {
                            channel.UnsafeOps.Remove(oldnickname);
                            channel.UnsafeOps.Add(newnickname, channeluser);
                        }
                        if (SupportNonRfc && ((NonRfcChannelUser)channeluser).IsHalfop) {
                            NonRfcChannel nchannel = (NonRfcChannel)channel;
                            nchannel.UnsafeHalfops.Remove(oldnickname);
                            nchannel.UnsafeHalfops.Add(newnickname, channeluser);
                        }
                        if (channeluser.IsVoice) {
                            channel.UnsafeVoices.Remove(oldnickname);
                            channel.UnsafeVoices.Add(newnickname, channeluser);
                        }
                    }
                }
            }
            
            if (OnNickChange != null) {
                OnNickChange(this, new NickChangeEventArgs(ircdata, oldnickname, newnickname));
            }
        }

        /// <summary>
        /// Event handler for invite messages
        /// </summary>
        /// <param name="ircdata">Message data containing invite information</param>
        private void _Event_INVITE(IrcMessageData ircdata)
        {
            string channel = ircdata.Channel;
            string inviter = ircdata.Nick;
            
            if (AutoJoinOnInvite) {
                if (channel.Trim() != "0") {
                    RfcJoin(channel);
                }
            }
            
            if (OnInvite != null) {
                OnInvite(this, new InviteEventArgs(ircdata, channel, inviter));
            }
        }

        /// <summary>
        /// Event handler for mode messages
        /// </summary>
        /// <param name="ircdata">Message data containing mode information</param>
        private void _Event_MODE(IrcMessageData ircdata)
        {
            if (IsMe(ircdata.RawMessageArray[2])) {
                // my user mode changed
                _Usermode = ircdata.RawMessageArray[3].Substring(1);
            } else {
                // channel mode changed
                string mode = ircdata.RawMessageArray[3];
                string parameter = String.Join(" ", ircdata.RawMessageArray, 4, ircdata.RawMessageArray.Length-4);
                _InterpretChannelMode(ircdata, mode, parameter);
            }
            
            if ((ircdata.Type == ReceiveType.UserModeChange) &&
                (OnUserModeChange != null)) {
                OnUserModeChange(this, new IrcEventArgs(ircdata));
            }

            if ((ircdata.Type == ReceiveType.ChannelModeChange) &&
                (OnChannelModeChange != null)) {
                OnChannelModeChange(this, new IrcEventArgs(ircdata));
            }

            if (OnModeChange != null) {
                OnModeChange(this, new IrcEventArgs(ircdata));
            }
        }

        /// <summary>
        /// Event handler for SASL authentication
        /// </summary>
        /// <param name="ircdata">Message data containing authentication subcommand </param>
        private void _Event_AUTH(IrcMessageData ircdata)
        {
            byte[] src = System.Text.UTF8Encoding.UTF8.GetBytes (String.Format ("{0}\0{0}\0{1}", _SaslAccount, _SaslPassword));
            Authenticate (System.Convert.ToBase64String (src), Priority.Critical);
        }

        /// <summary>
        /// Event handler for capability negotiation
        /// </summary>
        /// <param name="ircdata">Message data containing capability subcommand </param>
        private void _Event_CAP(IrcMessageData ircdata)
        {
            if (ircdata.Args.Length > 1)
            {
                switch (ircdata.Args[1])
                {
                    case "LS":
                        if (ircdata.MessageArray.Contains ("sasl"))
                        {
                            // sasl supported, request use
                            if (_SaslAccount != "")
                                CapReq (new string[] { "sasl" }, Priority.Critical);
                        }
                        break;
                    
                    case "ACK":
                        if (ircdata.MessageArray.Contains ("sasl"))
                        {
                            // sasl negotiated, authenticate
                            Authenticate (SaslAuthMethod.Plain, Priority.Critical);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Event handler for SASL authentication
        /// </summary>
        /// <param name="ircdata">Message data containing authentication subcommand </param>
        private void _Event_RPL_SASL(IrcMessageData ircdata)
        {
            Cap (CapabilitySubcommand.END, Priority.Critical);
            //check ircdata.ReplyCode for success
        }

        /// <summary>
        /// Event handler for channel mode reply messages
        /// </summary>
        /// <param name="ircdata">Message data containing reply information</param>
        private void _Event_RPL_CHANNELMODEIS(IrcMessageData ircdata)
        {
            if (ActiveChannelSyncing &&
                IsJoined(ircdata.Channel)) {
                // reset stored mode first, as this is the complete mode
                Channel chan = GetChannel(ircdata.Channel);
                chan.Mode = String.Empty;
                string mode = ircdata.RawMessageArray[4];
                string parameter = String.Join(" ", ircdata.RawMessageArray, 5, ircdata.RawMessageArray.Length-5);
                _InterpretChannelMode(ircdata, mode, parameter);
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
        private void _Event_RPL_WELCOME(IrcMessageData ircdata)
        {
            // updating our nickname, that we got (maybe cutted...)
            _Nickname = ircdata.RawMessageArray[2];

            if (OnRegistered != null) {
                OnRegistered(this, EventArgs.Empty);
            }
        }

        private void _Event_RPL_TOPIC(IrcMessageData ircdata)
        {
            string topic   = ircdata.Message;
            string channel = ircdata.Channel;

            if (ActiveChannelSyncing &&
                IsJoined(channel)) {
                GetChannel(channel).Topic = topic;
#if LOG4NET
                Logger.ChannelSyncing.Debug("stored topic for channel: "+channel);
#endif
            }

            if (OnTopic != null) {
                OnTopic(this, new TopicEventArgs(ircdata, channel, topic));
            }
        }

        private void _Event_RPL_NOTOPIC(IrcMessageData ircdata)
        {
            string channel = ircdata.Channel;

            if (ActiveChannelSyncing &&
                IsJoined(channel)) {
                GetChannel(channel).Topic = "";
#if LOG4NET
                Logger.ChannelSyncing.Debug("stored empty topic for channel: "+channel);
#endif
            }

            if (OnTopic != null) {
                OnTopic(this, new TopicEventArgs(ircdata, channel, ""));
            }
        }

        private void _Event_RPL_NAMREPLY(IrcMessageData ircdata)
        {
            string   channelname  = ircdata.Channel;
            string[] userlist     = ircdata.MessageArray;
            // HACK: BIP skips the colon after the channel name even though
            // RFC 1459 and 2812 says it's mandantory in RPL_NAMREPLY
            if (userlist == null) {
                if (ircdata.RawMessageArray.Length > 5) {
                    userlist = new string[] { ircdata.RawMessageArray[5] };
                } else {
                    userlist = new string[] {};
                }
            }

            if (ActiveChannelSyncing &&
                IsJoined(channelname)) {
                string nickname;
                bool   op;
                bool   halfop;
                bool   voice;
                foreach (string user in userlist) {
                    if (user.Length <= 0) {
                        continue;
                    }

                    op = false;
                    halfop = false;
                    voice = false;
                    switch (user[0]) {
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

                    IrcUser     ircuser     = GetIrcUser(nickname);
                    ChannelUser channeluser = GetChannelUser(channelname, nickname);

                    if (ircuser == null) {
#if LOG4NET
                        Logger.ChannelSyncing.Debug("creating IrcUser: "+nickname+" because he doesn't exist yet");
#endif
                        ircuser = new IrcUser(nickname, this);
                        _IrcUsers.Add(nickname, ircuser);
                    }

                    if (channeluser == null) {
#if LOG4NET
                        Logger.ChannelSyncing.Debug("creating ChannelUser: "+nickname+" for Channel: "+channelname+" because he doesn't exist yet");
#endif

                        channeluser = CreateChannelUser(channelname, ircuser);
                        Channel channel = GetChannel(channelname);
                        
                        channel.UnsafeUsers.Add(nickname, channeluser);
                        if (op) {
                            channel.UnsafeOps.Add(nickname, channeluser);
#if LOG4NET
                            Logger.ChannelSyncing.Debug("added op: "+nickname+" to: "+channelname);
#endif
                        }
                        if (SupportNonRfc && halfop)  {
                            ((NonRfcChannel)channel).UnsafeHalfops.Add(nickname, channeluser);
#if LOG4NET
                            Logger.ChannelSyncing.Debug("added halfop: "+nickname+" to: "+channelname);
#endif
                        }
                        if (voice) {
                            channel.UnsafeVoices.Add(nickname, channeluser);
#if LOG4NET
                            Logger.ChannelSyncing.Debug("added voice: "+nickname+" to: "+channelname);
#endif
                        }
                    }

                    channeluser.IsOp    = op;
                    channeluser.IsVoice = voice;
                    if (SupportNonRfc) {
                        ((NonRfcChannelUser)channeluser).IsHalfop = halfop;
                    }
                }
            }

            var filteredUserlist = new List<string>(userlist.Length);
            // filter user modes from nicknames
            foreach (string user in userlist) {
                if (String.IsNullOrEmpty(user)) {
                    continue;
                }

                switch (user[0]) {
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

            if (OnNames != null) {
                OnNames(this, new NamesEventArgs(ircdata, channelname,
                                                 filteredUserlist.ToArray()));
            }
        }
        
        private void _Event_RPL_LIST(IrcMessageData ircdata)
        {
            string channelName = ircdata.Channel;
            int userCount = Int32.Parse(ircdata.RawMessageArray[4]);
            string topic = ircdata.Message;
            
            ChannelInfo info = null;
            if (OnList != null || _ChannelList != null) {
                info = new ChannelInfo(channelName, userCount, topic);
            }
            
            if (_ChannelList != null) {
                _ChannelList.Add(info);
            }
            
            if (OnList != null) {
                OnList(this, new ListEventArgs(ircdata, info));
            }
        }
        
        private void _Event_RPL_LISTEND(IrcMessageData ircdata)
        {
            if (_ChannelListReceivedEvent != null) {
                _ChannelListReceivedEvent.Set();
            }
        }
        
        private void _Event_RPL_TRYAGAIN(IrcMessageData ircdata)
        {
            if (_ChannelListReceivedEvent != null) {
                _ChannelListReceivedEvent.Set();
            }
        }
        
        /*
        // BUG: RFC2812 says LIST and WHO might return ERR_TOOMANYMATCHES which
        // is not defined :(
        private void _Event_ERR_TOOMANYMATCHES(IrcMessageData ircdata)
        {
            if (_ListInfosReceivedEvent != null) {
                _ListInfosReceivedEvent.Set();
            }
        }
        */
        
        private void _Event_RPL_ENDOFNAMES(IrcMessageData ircdata)
        {
            string channelname = ircdata.RawMessageArray[3];
            if (ActiveChannelSyncing &&
                IsJoined(channelname)) {
#if LOG4NET
                Logger.ChannelSyncing.Debug("passive synced: "+channelname);
#endif
                if (OnChannelPassiveSynced != null) {
                    OnChannelPassiveSynced(this, new IrcEventArgs(ircdata));
                }
            }
        }

        private void _Event_RPL_AWAY(IrcMessageData ircdata)
        {
            string who = ircdata.RawMessageArray[3];
            string awaymessage = ircdata.Message;

            if (ActiveChannelSyncing) {
                IrcUser ircuser  = GetIrcUser(who);
                if (ircuser != null) {
#if LOG4NET
                    Logger.ChannelSyncing.Debug("setting away flag for user: "+who);
#endif
                    ircuser.IsAway = true;
                }
            }

            if (OnAway != null) {
                OnAway(this, new AwayEventArgs(ircdata, who, awaymessage));
            }
        }
        
        private void _Event_RPL_UNAWAY(IrcMessageData ircdata)
        {
            _IsAway = false;
            
            if (OnUnAway != null) {
                OnUnAway(this, new IrcEventArgs(ircdata));
            }
        }

        private void _Event_RPL_NOWAWAY(IrcMessageData ircdata)
        {
            _IsAway = true;

            if (OnNowAway != null) {
                OnNowAway(this, new IrcEventArgs(ircdata));
            }
        }

        private void _Event_RPL_WHOREPLY(IrcMessageData ircdata)
        {
            WhoInfo info = WhoInfo.Parse(ircdata);
            string channel = info.Channel;
            string nick = info.Nick;
            
            if (_WhoList != null) {
                _WhoList.Add(info);
            }
            
            if (ActiveChannelSyncing &&
                IsJoined(channel)) {
                // checking the irc and channel user I only do for sanity!
                // according to RFC they must be known to us already via RPL_NAMREPLY
                // psyBNC is not very correct with this... maybe other bouncers too
                IrcUser ircuser  = GetIrcUser(nick);
                ChannelUser channeluser = GetChannelUser(channel, nick);
#if LOG4NET
                if (ircuser == null) {
                    Logger.ChannelSyncing.Error("GetIrcUser("+nick+") returned null in _Event_WHOREPLY! Ignoring...");
                }
#endif

#if LOG4NET
                if (channeluser == null) {
                    Logger.ChannelSyncing.Error("GetChannelUser("+nick+") returned null in _Event_WHOREPLY! Ignoring...");
                }
#endif

                if (ircuser != null) {                                
#if LOG4NET
                    Logger.ChannelSyncing.Debug("updating userinfo (from whoreply) for user: "+nick+" channel: "+channel);
#endif

                    ircuser.Ident    = info.Ident;
                    ircuser.Host     = info.Host;
                    ircuser.Server   = info.Server;
                    ircuser.Nick     = info.Nick;
                    ircuser.HopCount = info.HopCount;
                    ircuser.Realname = info.Realname;
                    ircuser.IsAway   = info.IsAway;
                    ircuser.IsIrcOp  = info.IsIrcOp;
                    ircuser.IsRegistered = info.IsRegistered;
                
                    switch (channel[0]) {
                        case '#':
                        case '!':
                        case '&':
                        case '+':
                            // this channel may not be where we are joined!
                            // see RFC 1459 and RFC 2812, it must return a channelname
                            // we use this channel info when possible...
                            if (channeluser != null) {
                                channeluser.IsOp    = info.IsOp;
                                channeluser.IsVoice = info.IsVoice;
                            }
                        break;
                    }
                }
            }
            
            if (OnWho != null) {
                OnWho(this, new WhoEventArgs(ircdata, info));
            }
        }
        
        private void _Event_RPL_ENDOFWHO(IrcMessageData ircdata)
        {
            if (_WhoListReceivedEvent != null) {
                _WhoListReceivedEvent.Set();
            }
        }
        
        private void _Event_RPL_MOTD(IrcMessageData ircdata)
        {
            if (!_MotdReceived) {
                _Motd.Add(ircdata.Message);
            }
            
            if (OnMotd != null) {
                OnMotd(this, new MotdEventArgs(ircdata, ircdata.Message));
            }
        }
        
        private void _Event_RPL_ENDOFMOTD(IrcMessageData ircdata)
        {
            _MotdReceived = true;
        }
        
        private void _Event_RPL_BANLIST(IrcMessageData ircdata)
        {
            string channelname = ircdata.Channel;
            
            BanInfo info = BanInfo.Parse(ircdata);            
            if (_BanList != null) {
                _BanList.Add(info);
            }
            
            if (ActiveChannelSyncing &&
                IsJoined(channelname)) {
                Channel channel = GetChannel(channelname);
                if (channel.IsSycned) {
                    return;
                }
                
                channel.Bans.Add(info.Mask);
            }
        }
        
        private void _Event_RPL_ENDOFBANLIST(IrcMessageData ircdata)
        {
            string channelname = ircdata.Channel;
            
            if (_BanListReceivedEvent != null) {
                _BanListReceivedEvent.Set();
            }
            
            if (ActiveChannelSyncing &&
                IsJoined(channelname)) {
                Channel channel = GetChannel(channelname);
                if (channel.IsSycned) {
                    // only fire the event once
                    return;
                }
                
                channel.ActiveSyncStop = DateTime.Now;
                channel.IsSycned = true;
#if LOG4NET
                Logger.ChannelSyncing.Debug("active synced: "+channelname+
                    " (in "+channel.ActiveSyncTime.TotalSeconds+" sec)");
#endif
                if (OnChannelActiveSynced != null) {
                    OnChannelActiveSynced(this, new IrcEventArgs(ircdata));
                }
            }
        }
        
        // MODE +b might return ERR_NOCHANMODES for mode-less channels (like +chan) 
        private void _Event_ERR_NOCHANMODES(IrcMessageData ircdata)
        {
            string channelname = ircdata.RawMessageArray[3];
            if (ActiveChannelSyncing &&
                IsJoined(channelname)) {
                Channel channel = GetChannel(channelname);
                if (channel.IsSycned) {
                    // only fire the event once
                    return;
                }
                
                channel.ActiveSyncStop = DateTime.Now;
                channel.IsSycned = true;
#if LOG4NET
                Logger.ChannelSyncing.Debug("active synced: "+channelname+
                    " (in "+channel.ActiveSyncTime.TotalSeconds+" sec)");
#endif
                if (OnChannelActiveSynced != null) {
                    OnChannelActiveSynced(this, new IrcEventArgs(ircdata));
                }
            }
        }
        
        private void _Event_ERR(IrcMessageData ircdata)
        {
            if (OnErrorMessage != null) {
                OnErrorMessage(this, new IrcEventArgs(ircdata));
            }
        }
        
        private void _Event_ERR_NICKNAMEINUSE(IrcMessageData ircdata)
        {
#if LOG4NET
            Logger.Connection.Warn("nickname collision detected, changing nickname");
#endif
            if (!AutoNickHandling) {
                return;
            } 
            
            string nickname;
            // if a nicklist has been given loop through the nicknames
            // if the upper limit of this list has been reached and still no nickname has registered
            // then generate a random nick
            if (_CurrentNickname == NicknameList.Length-1) {
                Random rand = new Random();
                int number = rand.Next(999);
                if (Nickname.Length > 5) {
                    nickname = Nickname.Substring(0, 5)+number;
                } else {
                    nickname = Nickname.Substring(0, Nickname.Length-1)+number;
                }
            } else {
                nickname = _NextNickname();
            }
            // change the nickname
            RfcNick(nickname, Priority.Critical);
        }
#endregion
    }
}
