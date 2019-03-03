/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using SharpIrc.IrcClient.EventArgs;
using SharpIrc.IrcConnection;

namespace SharpIrc.IrcClient
{
    /// <summary>
    /// This layer is an event driven high-level API with all features you could need for IRC programming.
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    public class IrcClient : SharpIrc.IrcCommands.IrcCommands
    {
        private int _currentNickname;
        private int _iUserMode;
        private readonly Dictionary<string, string> _autoRejoinChannels = new Dictionary<string, string>();
        private bool _supportNonRfc;
        private bool _supportNonRfcLocked;
        private bool _motdReceived;
        private Array _replyCodes = Enum.GetValues(typeof(ReplyCode));

        private readonly Dictionary<string, Channel> _channels = new Dictionary<string, Channel>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, IrcUser> _ircUsers = new Dictionary<string, IrcUser>(StringComparer.InvariantCultureIgnoreCase);

        private List<ChannelInfo> _channelList;
        private readonly object _channelListSyncRoot = new object();
        private AutoResetEvent _channelListReceivedEvent;
        private List<WhoInfo> _whoList;
        private readonly object _whoListSyncRoot = new object();
        private AutoResetEvent _whoListReceivedEvent;
        private List<BanInfo> _banList;
        private AutoResetEvent _banListReceivedEvent;

        public event EventHandler<System.EventArgs> OnRegistered;
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
        public event EventHandler<AdminEventArgs> OnAdmin;
        public event EventHandler<DeadminEventArgs> OnDeadmin;
        public event EventHandler<HalfopEventArgs> OnHalfop;
        public event EventHandler<DehalfopEventArgs> OnDehalfop;
        public event EventHandler<OwnerEventArgs> OnOwner;
        public event EventHandler<DeownerEventArgs> OnDeowner;
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

        public event EventHandler<System.EventArgs> SupportNonRfcChanged;

        protected void DispatchEvent<T>(object sender, EventHandler<T> handler, T eventArgs) where T : System.EventArgs
        {
            if (handler == null) return;

            try
            {
                ThreadPool.QueueUserWorkItem(state => handler.Invoke(sender, eventArgs));
            }
            catch (Exception exception)
            {
                Console.WriteLine("I should handle this better: TODO FAILINDISPATCH: PROBABLY REMOTE (" + exception.Message + ")");
            }

        }
        public ServerProperties Properties { get; } = new ServerProperties();


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
            get;
            /*
    set {
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
        public bool AutoRejoinOnKick { get; set; }

        /// <summary>
        /// Enables/disables auto re-login to the server after a reconnect.
        /// Default: false
        /// </summary>
        public bool AutoReLogin { get; set; }

        /// <summary>
        /// Enables/disables auto nick handling on nick collisions
        /// Default: true
        /// </summary>
        public bool AutoNickHandling { get; set; } = true;

        /// <summary>
        /// Enables/disables support for non rfc features.
        /// Default: false
        /// </summary>
        public bool SupportNonRfc
        {
            get => _supportNonRfc;
            set
            {
                if (_supportNonRfcLocked)
                {
                    return;
                }
                _supportNonRfc = value;

                DispatchEvent(this, SupportNonRfcChanged, System.EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the nickname of us.
        /// </summary>
        public string Nickname { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the list of nicknames of us.
        /// </summary>
        public string[] NicknameList { get; private set; }

        /// <summary>
        /// Gets the supposed real name of us.
        /// </summary>
        public string RealName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the username for the server.
        /// </summary>
        /// <remarks>
        /// System username is set by default
        /// </remarks>
        public string Username { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the alphanumeric mode mask of us.
        /// </summary>
        public string UserMode { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the numeric mode mask of us.
        /// </summary>
        public int IUsermode => _iUserMode;

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
        public StringCollection JoinedChannels { get; }

        /// <summary>
        /// Gets the server message of the day.
        /// </summary>
        public StringCollection Motd { get; }

        public object BanListSyncRoot { get; }

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
            PassiveChannelSyncing = false;
            OnReadLine += Worker;
            OnDisconnected += Disconnected;
            OnConnectionError += ConnectionError;
        }

        /// <summary>
        /// Connection parameters required to establish an server connection.
        /// </summary>
        /// <param name="addresses">The list of server hostnames.</param>
        /// <param name="port">The TCP port the server listens on.</param>
        public new void Connect(string[] addresses, int port)
        {
            _supportNonRfcLocked = true;
            base.Connect(addresses, port);
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
                //reset the nick to the original nicks
                _currentNickname = 0;
                // FIXME: honor _Nickname (last used nickname)
                Login(NicknameList, RealName, IUsermode, Username, Password);
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
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and real name of a new user.</remark>
        /// <param name="nicks">The users list of 'nick' names which may NOT contain spaces</param>
        /// <param name="realName">The users 'real' name which may contain space characters</param>
        /// <param name="userMode">A numeric mode parameter.
        ///   <remark>
        ///     Set to 0 to receive wallops and be invisible.
        ///     Set to 4 to be invisible and not receive wallops.
        ///   </remark>
        /// </param>
        /// <param name="username">The user's machine logon name</param>
        /// <param name="password">The optional password can and MUST be set before any attempt to register
        ///  the connection is made.</param>
        public void Login(string[] nicks, string realName, int userMode, string username, string password)
        {
            NicknameList = (string[])nicks.Clone();
            // here we set the nickname which we will try first
            Nickname = NicknameList[0].Replace(" ", "");
            RealName = realName;
            _iUserMode = userMode;

            Username = !string.IsNullOrEmpty(username) ? username.Replace(" ", "") : Environment.UserName.Replace(" ", "");

            if (SaslAccount != "")
            {
                Cap(CapabilitySubCommand.LS, Priority.Critical);
            }

            if (!string.IsNullOrEmpty(password))
            {
                Password = password;
                RfcPass(Password, Priority.Critical);
            }

            RfcNick(Nickname, Priority.Critical);
            RfcUser(Username, IUsermode, RealName, Priority.Critical);
        }

        /// <summary>
        /// Login parameters required identify with server connection
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realName of a new user.</remark>
        /// <param name="nicks">The users list of 'nick' names which may NOT contain spaces</param>
        /// <param name="realName">The users 'real' name which may contain space characters</param>
        /// <param name="userMode">A numeric mode parameter.
        /// Set to 0 to receive wallops and be invisible.
        /// Set to 4 to be invisible and not receive wallops.</param>
        /// <param name="username">The user's machine logon name</param>
        public void Login(string[] nicks, string realName, int userMode, string username)
        {
            Login(nicks, realName, userMode, username, "");
        }

        /// <summary>
        /// Login parameters required identify with server connection
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realName of a new user.</remark>
        /// <param name="nicks">The users list of 'nick' names which may NOT contain spaces</param>
        /// <param name="realName">The users 'real' name which may contain space characters</param>
        /// <param name="userMode">A numeric mode parameter.
        /// Set to 0 to receive wallops and be invisible.
        /// Set to 4 to be invisible and not receive wallops.</param>
        public void Login(string[] nicks, string realName, int userMode)
        {
            Login(nicks, realName, userMode, "", "");
        }

        /// <summary>
        /// Login parameters required identify with server connection
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realName of a new user.</remark>
        /// <param name="nicks">The users list of 'nick' names which may NOT contain spaces</param>
        /// <param name="realName">The users 'real' name which may contain space characters</param>
        public void Login(string[] nicks, string realName)
        {
            Login(nicks, realName, 0, "", "");
        }

        /// <summary>
        /// Login parameters required identify with server connection
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realName of a new user.</remark>
        /// <param name="nick">The users 'nick' name which may NOT contain spaces</param>
        /// <param name="realName">The users 'real' name which may contain space characters</param>
        /// <param name="userMode">A numeric mode parameter.
        /// Set to 0 to receive wallops and be invisible.
        /// Set to 4 to be invisible and not receive wallops.</param>
        /// <param name="username">The user's machine logon name</param>
        /// <param name="password">The optional password can and MUST be set before any attempt to register
        ///  the connection is made.</param>
        public void Login(string nick, string realName, int userMode, string username, string password)
        {
            Login(new[] { nick, nick + "_", nick + "__" }, realName, userMode, username, password);
        }

        /// <summary>
        /// Login parameters required identify with server connection
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realName of a new user.</remark>
        /// <param name="nick">The users 'nick' name which may NOT contain spaces</param>
        /// <param name="realName">The users 'real' name which may contain space characters</param>
        /// <param name="userMode">A numeric mode parameter.
        /// Set to 0 to receive wallops and be invisible.
        /// Set to 4 to be invisible and not receive wallops.</param>
        /// <param name="username">The user's machine logon name</param>
        public void Login(string nick, string realName, int userMode, string username)
        {
            Login(new[] { nick, nick + "_", nick + "__" }, realName, userMode, username, "");
        }

        /// <summary>
        /// Login parameters required identify with server connection
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realName of a new user.</remark>
        /// <param name="nick">The users 'nick' name which may NOT contain spaces</param>
        /// <param name="realName">The users 'real' name which may contain space characters</param>
        /// <param name="userMode">A numeric mode parameter.
        /// Set to 0 to receive wallops and be invisible.
        /// Set to 4 to be invisible and not receive wallops.</param>
        public void Login(string nick, string realName, int userMode)
        {
            Login(new[] { nick, nick + "_", nick + "__" }, realName, userMode, "", "");
        }

        /// <summary>
        /// Login parameters required identify with server connection
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realName of a new user.</remark>
        /// <param name="nick">The users 'nick' name which may NOT contain spaces</param>
        /// <param name="realName">The users 'real' name which may contain space characters</param>
        public void Login(string nick, string realName)
        {
            Login(new[] { nick, nick + "_", nick + "__" }, realName, 0, "", "");
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
        /// <param name="channelName">The name of the channel you wish to query</param>
        /// <returns>True if you are found in channel</returns>
        public bool IsJoined(string channelName)
        {
            return IsJoined(channelName, Nickname);
        }

        /// <summary>
        /// Determine if a specified nickname can be found in a specified channel
        /// </summary>
        /// <param name="channelName">The name of the channel you wish to query</param>
        /// <param name="nickname">The users 'nick' name which may NOT contain spaces</param>
        /// <returns>True if nickname is found in channel</returns>
        public bool IsJoined(string channelName, string nickname)
        {
            if (channelName == null)
            {
                throw new ArgumentNullException(nameof(channelName));
            }

            if (nickname == null)
            {
                throw new ArgumentNullException(nameof(nickname));
            }

            Channel channel = GetChannel(channelName);

            return channel?.UnsafeUsers != null && channel.UnsafeUsers.ContainsKey(nickname);
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
                throw new ArgumentNullException(nameof(nickname));
            }

            return _ircUsers.TryGetValue(nickname, out var ircUser) ? ircUser : null;

        }

        /// <summary>
        /// Returns extended user information including channel information
        /// </summary>
        /// <param name="channelName">The name of the channel you wish to query</param>
        /// <param name="nickname">The users 'nick' name which may NOT contain spaces</param>
        /// <returns>ChannelUser object of requested channelName/nickname</returns>
        public ChannelUser GetChannelUser(string channelName, string nickname)
        {
            if (channelName == null)
            {
                throw new ArgumentNullException(nameof(channelName));
            }

            if (nickname == null)
            {
                throw new ArgumentNullException(nameof(nickname));
            }

            Channel channel = GetChannel(channelName);
            return (ChannelUser)channel?.UnsafeUsers[nickname];
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="channelName">The name of the channel you wish to query</param>
        /// <returns>Channel object of requested channel</returns>
        public Channel GetChannel(string channelName)
        {
            if (channelName == null)
            {
                throw new ArgumentNullException(nameof(channelName));
            }

            return _channels.TryGetValue(channelName, out var channel) ? channel : null;
        }

        /// <summary>
        /// Gets a list of all joined channels on server
        /// </summary>
        /// <returns>String array of all joined channel names</returns>
        public string[] GetChannels()
        {
            return _channels.Keys.ToArray();
        }

        /// <summary>
        /// Fetches a fresh list of all available channels that match the passed mask
        /// </summary>
        /// <returns>List of ListInfo</returns>
        public IList<ChannelInfo> GetChannelList(string mask)
        {
            var list = new List<ChannelInfo>();
            lock (_channelListSyncRoot)
            {
                _channelList = list;
                _channelListReceivedEvent = new AutoResetEvent(false);

                // request list
                RfcList(mask);
                // wait till we have the complete list
                _channelListReceivedEvent.WaitOne();

                _channelListReceivedEvent = null;
                _channelList = null;
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
            lock (_whoListSyncRoot)
            {
                _whoList = list;
                _whoListReceivedEvent = new AutoResetEvent(false);

                // request list
                RfcWho(mask);
                // wait till we have the complete list
                _whoListReceivedEvent.WaitOne();

                _whoListReceivedEvent = null;
                _whoList = null;
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
                _banList = list;
                _banListReceivedEvent = new AutoResetEvent(false);

                // request list
                Ban(channel);
                // wait till we have the complete list
                _banListReceivedEvent.WaitOne();

                _banListReceivedEvent = null;
                _banList = null;
            }

            return list;
        }

        protected virtual IrcUser CreateIrcUser(string nickname)
        {
            return new IrcUser(nickname, this);
        }

        protected virtual Channel CreateChannel(string name)
        {
            return _supportNonRfc ? new NonRfcChannel(name) : new Channel(name);
        }

        protected virtual ChannelUser CreateChannelUser(string channel, IrcUser ircUser)
        {
            return _supportNonRfc ? new NonRfcChannelUser(channel, ircUser) : new ChannelUser(channel, ircUser);
        }

        private void Worker(object sender, ReadLineEventArgs e)
        {
            var msg = new IrcMessageData(this, e.Line);

            // lets see if we have events or internal message handler for it
            HandleEvents(msg);
        }

        private void Disconnected(object sender, System.EventArgs e)
        {
            if (AutoRejoin)
            {
                StoreChannelsToRejoin();
            }
            SyncingCleanup();
        }

        private void ConnectionError(object sender, System.EventArgs e)
        {
            try
            {
                // AutoReconnect is handled in IrcConnection.ConnectionError
                if (AutoReconnect && AutoReLogin)
                {
                    Login(NicknameList, RealName, IUsermode, Username, Password);
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
            lock (_autoRejoinChannels)
            {
                _autoRejoinChannels.Clear();
                if (ActiveChannelSyncing || PassiveChannelSyncing)
                {
                    // store the key using channel sync
                    foreach (Channel channel in _channels.Values)
                    {
                        _autoRejoinChannels.Add(channel.Name, channel.Key);
                    }
                }
                else
                {
                    foreach (string channel in JoinedChannels)
                    {
                        _autoRejoinChannels.Add(channel, null);
                    }
                }
            }
        }

        private void RejoinChannels()
        {
            lock (_autoRejoinChannels)
            {
                RfcJoin(_autoRejoinChannels.Keys.ToArray(), _autoRejoinChannels.Values.ToArray(), Priority.High);
                _autoRejoinChannels.Clear();
            }
        }

        private void SyncingCleanup()
        {
            // lets clean it baby, powered by Mr. Proper
            JoinedChannels.Clear();
            if (ActiveChannelSyncing)
            {
                _channels.Clear();
                _ircUsers.Clear();
            }

            IsAway = false;

            _motdReceived = false;
            Motd.Clear();
        }

        /// <summary>
        ///
        /// </summary>
        private string NextNickname()
        {
            _currentNickname++;
            //if we reach the end stay there
            if (_currentNickname >= NicknameList.Length)
            {
                _currentNickname--;
            }
            return NicknameList[_currentNickname];
        }

        private void HandleEvents(IrcMessageData ircData)
        {
            DispatchEvent(this, OnRawMessage, new IrcEventArgs(ircData));

            switch (ircData.Command)
            {
                case "PING":
                    EventPing(ircData);
                    break;
                case "ERROR":
                    EventError(ircData);
                    break;
                case "PRIVMSG":
                    EventPrivateMessage(ircData);
                    break;
                case "NOTICE":
                    EventNotice(ircData);
                    break;
                case "JOIN":
                    EventJoin(ircData);
                    break;
                case "PART":
                    EventPart(ircData);
                    break;
                case "KICK":
                    EventKick(ircData);
                    break;
                case "QUIT":
                    EventQuit(ircData);
                    break;
                case "TOPIC":
                    EventTopic(ircData);
                    break;
                case "NICK":
                    EventNick(ircData);
                    break;
                case "INVITE":
                    EventInvite(ircData);
                    break;
                case "MODE":
                    EventMode(ircData);
                    break;
                case "PONG":
                    EventPong(ircData);
                    break;
                case "CAP":
                    EventCap(ircData);
                    break;
                case "AUTHENTICATE":
                    EventAuth(ircData);
                    break;
            }

            if (ircData.ReplyCode != ReplyCode.Null)
            {
                switch (ircData.ReplyCode)
                {
                    case ReplyCode.Welcome:
                        EventRplWelcome(ircData);
                        break;
                    case ReplyCode.Topic:
                        EventRplTopic(ircData);
                        break;
                    case ReplyCode.NoTopic:
                        EventRplNoTopic(ircData);
                        break;
                    case ReplyCode.NamesReply:
                        EventRplNamreply(ircData);
                        break;
                    case ReplyCode.EndOfNames:
                        EventRplEndOfNames(ircData);
                        break;
                    case ReplyCode.List:
                        EventRplList(ircData);
                        break;
                    case ReplyCode.ListEnd:
                        EventRplListEnd(ircData);
                        break;
                    case ReplyCode.WhoReply:
                        EventRplWhoReply(ircData);
                        break;
                    case ReplyCode.EndOfWho:
                        EventRplEndOfWho(ircData);
                        break;
                    case ReplyCode.ChannelModeIs:
                        EventRplChannelModeIs(ircData);
                        break;
                    case ReplyCode.BanList:
                        EventRplBanList(ircData);
                        break;
                    case ReplyCode.EndOfBanList:
                        EventRplEndOfBanList(ircData);
                        break;
                    case ReplyCode.ErrorNoChannelModes:
                        EventErrNoChanModes(ircData);
                        break;
                    case ReplyCode.Motd:
                        EventRplMotd(ircData);
                        break;
                    case ReplyCode.EndOfMotd:
                        EventRplEndOfMotd(ircData);
                        break;
                    case ReplyCode.Away:
                        EventRplAway(ircData);
                        break;
                    case ReplyCode.UnAway:
                        EventRplUnAway(ircData);
                        break;
                    case ReplyCode.NowAway:
                        EventRplNowAway(ircData);
                        break;
                    case ReplyCode.TryAgain:
                        EventRplTryAgain(ircData);
                        break;
                    case ReplyCode.ErrorNicknameInUse:
                        EventErrNickNameInUse(ircData);
                        break;
                    case ReplyCode.Bounce:
                        if (!ircData.RawMessage.StartsWith("Try server"))
                        {
                            // RPL_ISUPPORT (Very common enhancement)
                            Properties.Parse(ircData.RawMessage);
                        }
                        // RPL_BOUNCE (Rfc 2812)
                        break;
                    case ReplyCode.SaslSuccess:
                    case ReplyCode.SaslFailure1:
                    case ReplyCode.SaslFailure2:
                    case ReplyCode.SaslAbort:
                        EventRplSasl(ircData);
                        break;
                }
            }

            if (ircData.Type == ReceiveType.ErrorMessage)
            {
                EventErr(ircData);
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
                _ircUsers.Remove(nickname);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a specified user from a specified channel list
        /// </summary>
        /// <param name="channelName">The name of the channel you wish to query</param>
        /// <param name="nickname">The users 'nick' name which may NOT contain spaces</param>
        private void RemoveChannelUser(string channelName, string nickname)
        {
            Channel chan = GetChannel(channelName);
            chan.UnsafeUsers.Remove(nickname);
            chan.UnsafeOps.Remove(nickname);
            chan.UnsafeVoices.Remove(nickname);
            if (SupportNonRfc)
            {
                var nonRfcChannel = (NonRfcChannel)chan;
                nonRfcChannel.UnsafeAdmins.Remove(nickname);
                nonRfcChannel.UnsafeHalfops.Remove(nickname);
                nonRfcChannel.UnsafeOwners.Remove(nickname);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ircData">Message data containing channel mode information</param>
        /// <param name="mode">Channel mode</param>
        /// <param name="parameter">List of supplied parameters</param>
        private void InterpretChannelMode(IrcMessageData ircData, string mode, string parameter)
        {
            string[] parameters = parameter.Split(' ');
            bool add = false;
            bool remove = false;
            int modeLength = mode.Length;
            Channel channel = null;
            if (ActiveChannelSyncing)
            {
                channel = GetChannel(ircData.Channel);
            }

            IEnumerator parametersEnumerator = parameters.GetEnumerator();
            // bring the enumerator to the 1. element
            parametersEnumerator.MoveNext();
            for (int i = 0; i < modeLength; i++)
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
                                if (GetChannelUser(ircData.Channel, temp) != null)
                                {
                                    // update the op list
                                    try
                                    {
                                        channel.UnsafeOps.Add(temp, GetIrcUser(temp));
                                    }
                                    catch (ArgumentException)
                                    {
                                    }

                                    // update the user op status
                                    ChannelUser channelUser = GetChannelUser(ircData.Channel, temp);
                                    channelUser.IsOp = true;
                                }
                            }

                            DispatchEvent(this, OnOp, new OpEventArgs(ircData, ircData.Channel, ircData.Nick, temp));
                        }
                        if (remove)
                        {
                            if (ActiveChannelSyncing)
                            {
                                // sanity check
                                if (GetChannelUser(ircData.Channel, temp) != null)
                                {
                                    // update the op list
                                    channel.UnsafeOps.Remove(temp);
                                    // update the user op status
                                    ChannelUser channelUser = GetChannelUser(ircData.Channel, temp);
                                    channelUser.IsOp = false;
                                }
                            }

                            DispatchEvent(this, OnDeop, new DeopEventArgs(ircData, ircData.Channel, ircData.Nick, temp));
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
                                    if (GetChannelUser(ircData.Channel, temp) != null)
                                    {
                                        // update the halfop list
                                        try
                                        {
                                            ((NonRfcChannel)channel).UnsafeHalfops.Add(temp, GetIrcUser(temp));
                                        }
                                        catch (ArgumentException)
                                        {
                                        }

                                        // update the user halfop status
                                        var channelUser = (NonRfcChannelUser)GetChannelUser(ircData.Channel, temp);
                                        channelUser.IsHalfop = true;
                                    }
                                }

                                DispatchEvent(this, OnHalfop, new HalfopEventArgs(ircData, ircData.Channel, ircData.Nick, temp));
                            }
                            if (remove)
                            {
                                if (ActiveChannelSyncing)
                                {
                                    // sanity check
                                    if (GetChannelUser(ircData.Channel, temp) != null)
                                    {
                                        // update the halfop list
                                        ((NonRfcChannel)channel).UnsafeHalfops.Remove(temp);
                                        // update the user halfop status
                                        var channelUser = (NonRfcChannelUser)GetChannelUser(ircData.Channel, temp);
                                        channelUser.IsHalfop = false;
                                    }
                                }

                                DispatchEvent(this, OnDehalfop, new DehalfopEventArgs(ircData, ircData.Channel, ircData.Nick, temp));
                            }
                        }
                        break;
                    case 'a':
                        if (SupportNonRfc)
                        {
                            temp = (string)parametersEnumerator.Current;
                            parametersEnumerator.MoveNext();

                            if (add)
                            {
                                if (ActiveChannelSyncing)
                                {
                                    // sanity check
                                    if (GetChannelUser(ircData.Channel, temp) != null)
                                    {
                                        // update the halfop list
                                        try
                                        {
                                            ((NonRfcChannel)channel).UnsafeAdmins.Add(temp, GetIrcUser(temp));
                                        }
                                        catch (ArgumentException)
                                        {
                                        }

                                        // update the user halfop status
                                        NonRfcChannelUser channelUser = (NonRfcChannelUser)GetChannelUser(ircData.Channel, temp);
                                        channelUser.IsAdmin = true;
                                    }
                                }

                                DispatchEvent(this, OnAdmin, new AdminEventArgs(ircData, ircData.Channel, ircData.Nick, temp));

                            }
                            if (remove)
                            {
                                if (ActiveChannelSyncing)
                                {
                                    // sanity check
                                    if (GetChannelUser(ircData.Channel, temp) != null)
                                    {
                                        // update the halfop list
                                        ((NonRfcChannel)channel).UnsafeAdmins.Remove(temp);
                                        // update the user halfop status
                                        NonRfcChannelUser channelUser = (NonRfcChannelUser)GetChannelUser(ircData.Channel, temp);
                                        channelUser.IsAdmin = false;
                                    }
                                }

                                DispatchEvent(this, OnDeadmin, new DeadminEventArgs(ircData, ircData.Channel, ircData.Nick, temp));
                            }
                        }
                        break;
                    case 'r':
                        if (SupportNonRfc)
                        {
                            temp = (string)parametersEnumerator.Current;
                            parametersEnumerator.MoveNext();

                            if (add)
                            {
                                if (ActiveChannelSyncing)
                                {
                                    // sanity check
                                    if (GetChannelUser(ircData.Channel, temp) != null)
                                    {
                                        // update the halfop list
                                        try
                                        {
                                            ((NonRfcChannel)channel).UnsafeOwners.Add(temp, GetIrcUser(temp));
                                        }
                                        catch (ArgumentException)
                                        {
                                        }

                                        // update the user halfop status
                                        NonRfcChannelUser channelUser = (NonRfcChannelUser)GetChannelUser(ircData.Channel, temp);
                                        channelUser.IsOwner = true;
                                    }
                                }

                                DispatchEvent(this, OnOwner, new OwnerEventArgs(ircData, ircData.Channel, ircData.Nick, temp));
                            }
                            if (remove)
                            {
                                if (ActiveChannelSyncing)
                                {
                                    // sanity check
                                    if (GetChannelUser(ircData.Channel, temp) != null)
                                    {
                                        // update the halfop list
                                        ((NonRfcChannel)channel).UnsafeOwners.Remove(temp);
                                        // update the user halfop status
                                        NonRfcChannelUser channelUser = (NonRfcChannelUser)GetChannelUser(ircData.Channel, temp);
                                        channelUser.IsOwner = false;
                                    }
                                }

                                DispatchEvent(this, OnDeowner, new DeownerEventArgs(ircData, ircData.Channel, ircData.Nick, temp));
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
                                if (GetChannelUser(ircData.Channel, temp) != null)
                                {
                                    // update the voice list
                                    try
                                    {
                                        channel.UnsafeVoices.Add(temp, GetIrcUser(temp));
                                    }
                                    catch (ArgumentException)
                                    {
                                    }

                                    // update the user voice status
                                    ChannelUser channelUser = GetChannelUser(ircData.Channel, temp);
                                    channelUser.IsVoice = true;
                                }
                            }

                            DispatchEvent(this, OnVoice, new VoiceEventArgs(ircData, ircData.Channel, ircData.Nick, temp));
                        }
                        if (remove)
                        {
                            if (ActiveChannelSyncing)
                            {
                                // sanity check
                                if (GetChannelUser(ircData.Channel, temp) != null)
                                {
                                    // update the voice list
                                    channel.UnsafeVoices.Remove(temp);
                                    // update the user voice status
                                    ChannelUser channelUser = GetChannelUser(ircData.Channel, temp);
                                    channelUser.IsVoice = false;
                                }
                            }

                            DispatchEvent(this, OnDevoice, new DevoiceEventArgs(ircData, ircData.Channel, ircData.Nick, temp));
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
                                }
                                catch (ArgumentException)
                                {
                                }
                            }
                            DispatchEvent(this, OnBan, new BanEventArgs(ircData, ircData.Channel, ircData.Nick, temp));
                        }
                        if (remove)
                        {
                            if (ActiveChannelSyncing)
                            {
                                channel.Bans.Remove(temp);
                            }
                            DispatchEvent(this, OnUnban, new UnbanEventArgs(ircData, ircData.Channel, ircData.Nick, temp));
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
                                }
                                catch (FormatException)
                                {
                                }
                            }
                        }
                        if (remove)
                        {
                            if (ActiveChannelSyncing)
                            {
                                channel.UserLimit = 0;
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
                            }
                        }
                        if (remove)
                        {
                            if (ActiveChannelSyncing)
                            {
                                channel.Key = "";
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
                                }
                            }
                        }
                        if (remove)
                        {
                            if (ActiveChannelSyncing)
                            {
                                channel.Mode = channel.Mode.Replace(mode[i].ToString(), String.Empty);
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
        /// <param name="ircData">Message data containing ping information</param>
        private void EventPing(IrcMessageData ircData)
        {
            string server = ircData.RawMessageArray[1].Substring(1);

            RfcPong(server, Priority.Critical);

            DispatchEvent(this, OnPing, new PingEventArgs(ircData, server));
        }

        /// <summary>
        /// Event handler for PONG messages
        /// </summary>
        /// <param name="ircData">Message data containing pong information</param>
        private void EventPong(IrcMessageData ircData)
        {
            DispatchEvent(this, OnPong, new PongEventArgs(ircData, ircData.Irc.Lag));
        }

        /// <summary>
        /// Event handler for error messages
        /// </summary>
        /// <param name="ircData">Message data containing error information</param>
        private void EventError(IrcMessageData ircData)
        {
            string message = ircData.Message;
            DispatchEvent(this, OnError, new ErrorEventArgs(ircData, message));
        }

        /// <summary>
        /// Event handler for join messages
        /// </summary>
        /// <param name="ircData">Message data containing join information</param>
        private void EventJoin(IrcMessageData ircData)
        {
            string who = ircData.Nick;
            string channelName = ircData.Channel;

            if (IsMe(who))
            {
                JoinedChannels.Add(channelName);
            }

            if (ActiveChannelSyncing)
            {
                Channel channel;
                if (IsMe(who))
                {
                    // we joined the channel
                    channel = CreateChannel(channelName);
                    _channels.Add(channelName, channel);

                    // request channel mode
                    RfcMode(channelName);

                    // request the who-list
                    RfcWho(channelName);

                    // request the ban-list
                    Ban(channelName);
                }
                else
                {
                    // someone else joined the channel
                    // request the who data
                    RfcWho(who);
                }

                channel = GetChannel(channelName);
                IrcUser ircUser = GetIrcUser(who);

                if (ircUser == null)
                {
                    ircUser = new IrcUser(who, this) { Ident = ircData.Ident, Host = ircData.Host };
                    _ircUsers.Add(who, ircUser);
                }

                // HACK: IRCnet's anonymous channel mode feature breaks our
                // channel sync here as they use the same nick for ALL channel
                // users!
                // Example: :anonymous!anonymous@anonymous. JOIN :$channel
                if (who == "anonymous" && ircData.Ident == "anonymous" && ircData.Host == "anonymous." && IsJoined(channelName, who))
                {
                    // ignore
                }
                else
                {
                    ChannelUser channelUser = CreateChannelUser(channelName, ircUser);
                    channel.UnsafeUsers.Add(who, channelUser);
                }
            }

            DispatchEvent(this, OnJoin, new JoinEventArgs(ircData, channelName, who));
        }

        /// <summary>
        /// Event handler for part messages
        /// </summary>
        /// <param name="ircData">Message data containing part information</param>
        private void EventPart(IrcMessageData ircData)
        {
            string who = ircData.Nick;
            string channel = ircData.Channel;
            string partMessage = ircData.Message;

            if (IsMe(who))
            {
                JoinedChannels.Remove(channel);
            }

            if (ActiveChannelSyncing)
            {
                if (IsMe(who))
                {
                    _channels.Remove(channel);
                }
                else
                {
                    // HACK: IRCnet's anonymous channel mode feature breaks our
                    // channel sync here as they use the same nick for ALL channel
                    // users!
                    // Example: :anonymous!anonymous@anonymous. PART $channel :$msg
                    if (who == "anonymous" &&
                        ircData.Ident == "anonymous" &&
                        ircData.Host == "anonymous." &&
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

            DispatchEvent(this, OnPart, new PartEventArgs(ircData, channel, who, partMessage));
        }

        /// <summary>
        /// Event handler for kick messages
        /// </summary>
        /// <param name="ircData">Message data containing kick information</param>
        private void EventKick(IrcMessageData ircData)
        {
            string channelName = ircData.Channel;
            string who = ircData.Nick;
            string whom = ircData.RawMessageArray[3];
            string reason = ircData.Message;
            bool isMe = IsMe(whom);

            if (isMe)
            {
                JoinedChannels.Remove(channelName);
            }

            if (ActiveChannelSyncing)
            {
                if (isMe)
                {
                    Channel channel = GetChannel(channelName);
                    _channels.Remove(channelName);
                    if (AutoRejoinOnKick)
                    {
                        RfcJoin(channel.Name, channel.Key);
                    }
                }
                else
                {
                    RemoveChannelUser(channelName, whom);
                    RemoveIrcUser(whom);
                }
            }
            else
            {
                if (isMe && AutoRejoinOnKick)
                {
                    RfcJoin(channelName);
                }
            }

            DispatchEvent(this, OnKick, new KickEventArgs(ircData, channelName, who, whom, reason));
        }

        /// <summary>
        /// Event handler for quit messages
        /// </summary>
        /// <param name="ircData">Message data containing quit information</param>
        private void EventQuit(IrcMessageData ircData)
        {
            string who = ircData.Nick;
            string reason = ircData.Message;

            // no need to handle if we quit, disconnect event will take care

            if (ActiveChannelSyncing)
            {
                // sanity checks, freshirc is very broken about RFC
                IrcUser user = GetIrcUser(who);
                string[] joinedChannels = user?.JoinedChannels;
                if (joinedChannels != null)
                {
                    foreach (string channel in joinedChannels)
                    {
                        RemoveChannelUser(channel, who);
                    }
                    RemoveIrcUser(who);
                }
            }

            DispatchEvent(this, OnQuit, new QuitEventArgs(ircData, who, reason));
        }

        /// <summary>
        /// Event handler for private messages
        /// </summary>
        /// <param name="ircData">Message data containing private message information</param>
        private void EventPrivateMessage(IrcMessageData ircData)
        {
            switch (ircData.Type)
            {
                case ReceiveType.ChannelMessage:
                    DispatchEvent(this, OnChannelMessage, new IrcEventArgs(ircData));
                    break;
                case ReceiveType.ChannelAction:
                    DispatchEvent(this, OnChannelAction, new ActionEventArgs(ircData, ircData.Message.Substring(8, ircData.Message.Length - 9)));
                    break;
                case ReceiveType.QueryMessage:
                    DispatchEvent(this, OnQueryMessage, new IrcEventArgs(ircData));
                    break;
                case ReceiveType.QueryAction:
                    DispatchEvent(this, OnQueryAction, new ActionEventArgs(ircData, ircData.Message.Substring(8, ircData.Message.Length - 9)));
                    break;
                case ReceiveType.CtcpRequest:
                    int spacePos = ircData.Message.IndexOf(' ');
                    string cmd;
                    string param = "";
                    if (spacePos != -1)
                    {
                        cmd = ircData.Message.Substring(1, spacePos - 1);
                        param = ircData.Message.Substring(spacePos + 1, ircData.Message.Length - spacePos - 2);
                    }
                    else
                    {
                        cmd = ircData.Message.Substring(1, ircData.Message.Length - 2);
                    }
                    DispatchEvent(this, OnCtcpRequest, new CtcpEventArgs(ircData, cmd, param));
                    break;
            }
        }

        /// <summary>
        /// Event handler for notice messages
        /// </summary>
        /// <param name="ircData">Message data containing notice information</param>
        private void EventNotice(IrcMessageData ircData)
        {
            switch (ircData.Type)
            {
                case ReceiveType.ChannelNotice:
                    DispatchEvent(this, OnChannelNotice, new IrcEventArgs(ircData));
                    break;
                case ReceiveType.QueryNotice:
                    DispatchEvent(this, OnQueryNotice, new IrcEventArgs(ircData));
                    break;
                case ReceiveType.CtcpReply:
                    int spacePos = ircData.Message.IndexOf(' ');
                    string cmd;
                    string param = "";
                    if (spacePos != -1)
                    {
                        cmd = ircData.Message.Substring(1, spacePos - 1);
                        param = ircData.Message.Substring(spacePos + 1, ircData.Message.Length - spacePos - 2);
                    }
                    else
                    {
                        cmd = ircData.Message.Substring(1, ircData.Message.Length - 2);
                    }
                    DispatchEvent(this, OnCtcpReply, new CtcpEventArgs(ircData, cmd, param));
                    break;
            }
        }

        /// <summary>
        /// Event handler for topic messages
        /// </summary>
        /// <param name="ircData">Message data containing topic information</param>
        private void EventTopic(IrcMessageData ircData)
        {
            string who = ircData.Nick;
            string channel = ircData.Channel;
            string newTopic = ircData.Message;

            if (ActiveChannelSyncing &&
                IsJoined(channel))
            {
                GetChannel(channel).Topic = newTopic;
            }

            DispatchEvent(this, OnTopicChange, new TopicChangeEventArgs(ircData, channel, who, newTopic));
        }

        /// <summary>
        /// Event handler for nickname messages
        /// </summary>
        /// <param name="ircData">Message data containing nickname information</param>
        private void EventNick(IrcMessageData ircData)
        {
            string oldNickname = ircData.Nick;
            string newNickname = ircData.RawMessageArray[2];

            // so let's strip the colon if it's there
            if (newNickname.StartsWith(":"))
            {
                newNickname = newNickname.Substring(1);
            }

            if (IsMe(ircData.Nick))
            {
                // nickname change is your own
                Nickname = newNickname;
            }

            if (ActiveChannelSyncing)
            {
                IrcUser ircUser = GetIrcUser(oldNickname);

                // if we don't have any info about him, don't update him!
                // (only queries or ourself in no channels)
                if (ircUser != null)
                {
                    string[] joinedChannels = ircUser.JoinedChannels;

                    // update his nickname
                    ircUser.Nick = newNickname;
                    // remove the old entry
                    // remove first to avoid duplication, Foo -> foo
                    _ircUsers.Remove(oldNickname);
                    // add him as new entry and new nickname as key
                    _ircUsers.Add(newNickname, ircUser);
                    // now the same for all channels he is joined
                    foreach (string channelName in joinedChannels)
                    {
                        var channel = GetChannel(channelName);
                        var channelUser = GetChannelUser(channelName, oldNickname);
                        // remove first to avoid duplication, Foo -> foo
                        channel.UnsafeUsers.Remove(oldNickname);
                        channel.UnsafeUsers.Add(newNickname, channelUser);
                        if (channelUser.IsOp)
                        {
                            channel.UnsafeOps.Remove(oldNickname);
                            channel.UnsafeOps.Add(newNickname, channelUser);
                        }
                        if (channelUser.IsVoice)
                        {
                            channel.UnsafeVoices.Remove(oldNickname);
                            channel.UnsafeVoices.Add(newNickname, channelUser);
                        }
                        if (SupportNonRfc)
                        {
                            var nonRfcChannel = (NonRfcChannel)channel;
                            if (((NonRfcChannelUser)channelUser).IsAdmin)
                            {
                                nonRfcChannel.UnsafeAdmins.Remove(oldNickname);
                                nonRfcChannel.UnsafeAdmins.Add(newNickname, channelUser);
                            }
                            if (((NonRfcChannelUser)channelUser).IsHalfop)
                            {
                                nonRfcChannel.UnsafeHalfops.Remove(oldNickname);
                                nonRfcChannel.UnsafeHalfops.Add(newNickname, channelUser);
                            }
                            if (((NonRfcChannelUser)channelUser).IsOwner)
                            {
                                nonRfcChannel.UnsafeOwners.Remove(oldNickname);
                                nonRfcChannel.UnsafeOwners.Add(newNickname, channelUser);
                            }
                        }
                    }
                }
            }

            DispatchEvent(this, OnNickChange, new NickChangeEventArgs(ircData, oldNickname, newNickname));
        }

        /// <summary>
        /// Event handler for invite messages
        /// </summary>
        /// <param name="ircData">Message data containing invite information</param>
        private void EventInvite(IrcMessageData ircData)
        {
            string channel = ircData.Channel;
            string inviter = ircData.Nick;

            if (AutoJoinOnInvite)
            {
                if (channel.Trim() != "0")
                {
                    RfcJoin(channel);
                }
            }

            DispatchEvent(this, OnInvite, new InviteEventArgs(ircData, channel, inviter));
        }

        /// <summary>
        /// Event handler for mode messages
        /// </summary>
        /// <param name="ircData">Message data containing mode information</param>
        private void EventMode(IrcMessageData ircData)
        {
            if (IsMe(ircData.RawMessageArray[2]))
            {
                // my user mode changed
                UserMode = ircData.RawMessageArray[3].Substring(1);
            }
            else
            {
                // channel mode changed
                string mode = ircData.RawMessageArray[3];
                string parameter = String.Join(" ", ircData.RawMessageArray, 4, ircData.RawMessageArray.Length - 4);
                InterpretChannelMode(ircData, mode, parameter);
            }

            switch (ircData.Type)
            {
                case ReceiveType.UserModeChange:
                    DispatchEvent(this, OnUserModeChange, new IrcEventArgs(ircData));
                    break;
                case ReceiveType.ChannelModeChange:
                    DispatchEvent(this, OnChannelModeChange, new IrcEventArgs(ircData));
                    break;
            }

            DispatchEvent(this, OnModeChange, new IrcEventArgs(ircData));
        }

        /// <summary>
        /// Event handler for SASL authentication
        /// </summary>
        /// <param name="ircData">Message data containing authentication sub command </param>
        private void EventAuth(IrcMessageData ircData)
        {
            byte[] src = Encoding.UTF8.GetBytes($"{SaslAccount}\0{SaslAccount}\0{SaslPassword}");
            Authenticate(Convert.ToBase64String(src), Priority.Critical);
        }

        /// <summary>
        /// Event handler for capability negotiation
        /// </summary>
        /// <param name="ircData">Message data containing capability sub command </param>
        private void EventCap(IrcMessageData ircData)
        {
            if (ircData.Args.Length > 1)
            {
                switch (ircData.Args[1])
                {
                    case "LS":
                        if (ircData.MessageArray.Contains("sasl"))
                        {
                            // sasl supported, request use
                            if (SaslAccount != "")
                                CapReq(new[] { "sasl" }, Priority.Critical);
                        }
                        break;

                    case "ACK":
                        if (ircData.MessageArray.Contains("sasl"))
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
        /// <param name="ircData">Message data containing authentication sub command </param>
        private void EventRplSasl(IrcMessageData ircData)
        {
            Cap(CapabilitySubCommand.END, Priority.Critical);
            //check ircData.ReplyCode for success
        }

        /// <summary>
        /// Event handler for channel mode reply messages
        /// </summary>
        /// <param name="ircData">Message data containing reply information</param>
        private void EventRplChannelModeIs(IrcMessageData ircData)
        {
            if (ActiveChannelSyncing &&
                IsJoined(ircData.Channel))
            {
                // reset stored mode first, as this is the complete mode
                Channel chan = GetChannel(ircData.Channel);
                chan.Mode = String.Empty;
                string mode = ircData.RawMessageArray[4];
                string parameter = String.Join(" ", ircData.RawMessageArray, 5, ircData.RawMessageArray.Length - 5);
                InterpretChannelMode(ircData, mode, parameter);
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
        /// <param name="ircData">Message data containing reply information</param>
        private void EventRplWelcome(IrcMessageData ircData)
        {
            // updating our nickname, that we got (maybe cut...)
            Nickname = ircData.RawMessageArray[2];

            DispatchEvent(this, OnRegistered, System.EventArgs.Empty);
        }

        private void EventRplTopic(IrcMessageData ircData)
        {
            string topic = ircData.Message;
            string channel = ircData.Channel;

            if (ActiveChannelSyncing &&
                IsJoined(channel))
            {
                GetChannel(channel).Topic = topic;
            }

            DispatchEvent(this, OnTopic, new TopicEventArgs(ircData, channel, topic));
        }

        private void EventRplNoTopic(IrcMessageData ircData)
        {
            string channel = ircData.Channel;

            if (ActiveChannelSyncing && IsJoined(channel))
            {
                GetChannel(channel).Topic = "";
            }

            DispatchEvent(this, OnTopic, new TopicEventArgs(ircData, channel, string.Empty));
        }

        private void EventRplNamreply(IrcMessageData ircData)
        {
            string channelName = ircData.Channel;
            string[] users = ircData.MessageArray ?? (ircData.RawMessageArray.Length > 5 ? new[] { ircData.RawMessageArray[5] } : new string[] { });
            // HACK: BIP skips the colon after the channel name even though
            // RFC 1459 and 2812 says it's mandatory in RPL_NAMREPLY

            if (ActiveChannelSyncing && IsJoined(channelName))
            {
                foreach (string user in users)
                {
                    if (user.Length <= 0)
                    {
                        continue;
                    }

                    var op = false;
                    var voice = false;
                    var halfop = false;
                    var admin = false;
                    var owner = false;

                    string nickname;
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
                        case '*':
                            owner = true;
                            nickname = user.Substring(1);
                            break;
                        case '!':
                            admin = true;
                            nickname = user.Substring(1);
                            break;
                        default:
                            nickname = user;
                            break;
                    }

                    IrcUser ircUser = GetIrcUser(nickname);
                    ChannelUser channelUser = GetChannelUser(channelName, nickname);

                    if (ircUser == null)
                    {
                        ircUser = new IrcUser(nickname, this);
                        _ircUsers.Add(nickname, ircUser);
                    }

                    if (channelUser == null)
                    {

                        channelUser = CreateChannelUser(channelName, ircUser);
                        Channel channel = GetChannel(channelName);

                        channel.UnsafeUsers.Add(nickname, channelUser);
                        if (op)
                        {
                            channel.UnsafeOps.Add(nickname, channelUser);
                        }
                        if (voice)
                        {
                            channel.UnsafeVoices.Add(nickname, channelUser);
                        }
                        if (SupportNonRfc)
                        {
                            if (admin)
                            {
                                ((NonRfcChannel)channel).UnsafeAdmins.Add(nickname, channelUser);
                            }
                            if (halfop)
                            {
                                ((NonRfcChannel)channel).UnsafeHalfops.Add(nickname, channelUser);
                            }
                            if (owner)
                            {
                                ((NonRfcChannel)channel).UnsafeOwners.Add(nickname, channelUser);
                            }

                        }
                    }

                    channelUser.IsOp = op;
                    channelUser.IsVoice = voice;
                    if (SupportNonRfc)
                    {
                        ((NonRfcChannelUser)channelUser).IsAdmin = admin;
                        ((NonRfcChannelUser)channelUser).IsHalfop = halfop;
                        ((NonRfcChannelUser)channelUser).IsOwner = owner;
                    }
                }
            }

            var filteredUsers = new List<string>(users.Length);
            // filter user modes from nicknames
            foreach (string user in users)
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
                        filteredUsers.Add(user.Substring(1));
                        break;
                    default:
                        filteredUsers.Add(user);
                        break;
                }
            }

            DispatchEvent(this, OnNames, new NamesEventArgs(ircData, channelName, filteredUsers.ToArray()));
        }

        private void EventRplList(IrcMessageData ircData)
        {
            string channelName = ircData.Channel;
            int userCount = Int32.Parse(ircData.RawMessageArray[4]);
            string topic = ircData.Message;

            ChannelInfo info = null;
            if (OnList != null || _channelList != null)
            {
                info = new ChannelInfo(channelName, userCount, topic);
            }

            _channelList?.Add(info);

            DispatchEvent(this, OnList, new ListEventArgs(ircData, info));
        }

        private void EventRplListEnd(IrcMessageData ircData)
        {
            _channelListReceivedEvent?.Set();
        }

        private void EventRplTryAgain(IrcMessageData ircData)
        {
            _channelListReceivedEvent?.Set();
        }

        /*
        // BUG: RFC2812 says LIST and WHO might return ERR_TOOMANYMATCHES which
        // is not defined :(
        private void _Event_ERR_TOOMANYMATCHES(IrcMessageData ircData)
        {
            if (ListInfosReceivedEvent != null)
            {
                ListInfosReceivedEvent.Set();
            }
        }
        */

        private void EventRplEndOfNames(IrcMessageData ircData)
        {
            string channelName = ircData.RawMessageArray[3];
            if (ActiveChannelSyncing &&
                IsJoined(channelName))
            {
                DispatchEvent(this, OnChannelPassiveSynced, new IrcEventArgs(ircData));
            }
        }

        private void EventRplAway(IrcMessageData ircData)
        {
            string who = ircData.RawMessageArray[3];
            string awayMessage = ircData.Message;

            if (ActiveChannelSyncing)
            {
                IrcUser ircUser = GetIrcUser(who);
                if (ircUser != null)
                {
                    ircUser.IsAway = true;
                }
            }

            DispatchEvent(this, OnAway, new AwayEventArgs(ircData, who, awayMessage));
        }

        private void EventRplUnAway(IrcMessageData ircData)
        {
            IsAway = false;

            DispatchEvent(this, OnUnAway, new IrcEventArgs(ircData));
        }

        private void EventRplNowAway(IrcMessageData ircData)
        {
            IsAway = true;

            DispatchEvent(this, OnNowAway, new IrcEventArgs(ircData));
        }

        private void EventRplWhoReply(IrcMessageData ircData)
        {
            WhoInfo info = WhoInfo.Parse(ircData);
            string channel = info.Channel;
            string nick = info.Nick;

            _whoList?.Add(info);

            if (ActiveChannelSyncing &&
                IsJoined(channel))
            {
                // checking the irc and channel user I only do for sanity!
                // according to RFC they must be known to us already via RPL_NAMREPLY
                // psyBNC is not very correct with this... maybe other bouncers too
                IrcUser ircUser = GetIrcUser(nick);
                ChannelUser channelUser = GetChannelUser(channel, nick);

                if (ircUser != null)
                {
                    ircUser.Ident = info.Ident;
                    ircUser.Host = info.Host;
                    ircUser.Server = info.Server;
                    ircUser.Nick = info.Nick;
                    ircUser.HopCount = info.HopCount;
                    ircUser.RealName = info.RealName;
                    ircUser.IsAway = info.IsAway;
                    ircUser.IsIrcOp = info.IsIrcOp;
                    ircUser.IsRegistered = info.IsRegistered;

                    switch (channel[0])
                    {
                        case '#':
                        case '!':
                        case '&':
                        case '+':
                            // this channel may not be where we are joined!
                            // see RFC 1459 and RFC 2812, it must return a channelName
                            // we use this channel info when possible...
                            if (channelUser != null)
                            {
                                channelUser.IsOp = info.IsOp;
                                channelUser.IsVoice = info.IsVoice;
                            }
                            break;
                    }
                }
            }

            DispatchEvent(this, OnWho, new WhoEventArgs(ircData, info));
        }

        private void EventRplEndOfWho(IrcMessageData ircData)
        {
            _whoListReceivedEvent?.Set();
        }

        private void EventRplMotd(IrcMessageData ircData)
        {
            if (!_motdReceived)
            {
                Motd.Add(ircData.Message);
            }

            DispatchEvent(this, OnMotd, new MotdEventArgs(ircData, ircData.Message));
        }

        private void EventRplEndOfMotd(IrcMessageData ircData)
        {
            _motdReceived = true;
        }

        private void EventRplBanList(IrcMessageData ircData)
        {
            string channelName = ircData.Channel;

            BanInfo info = BanInfo.Parse(ircData);
            _banList?.Add(info);

            if (ActiveChannelSyncing &&
                IsJoined(channelName))
            {
                Channel channel = GetChannel(channelName);
                if (channel.IsSynced)
                {
                    return;
                }

                channel.Bans.Add(info.Mask);
            }
        }

        private void EventRplEndOfBanList(IrcMessageData ircData)
        {
            string ircDataChannel = ircData.Channel;

            _banListReceivedEvent?.Set();

            if (ActiveChannelSyncing && IsJoined(ircDataChannel))
            {
                Channel channel = GetChannel(ircDataChannel);
                if (channel.IsSynced)
                {
                    // only fire the event once
                    return;
                }

                channel.ActiveSyncStop = DateTime.Now;
                channel.IsSynced = true;
                DispatchEvent(this, OnChannelActiveSynced, new IrcEventArgs(ircData));
            }
        }

        // MODE +b might return ERR_NOCHANMODES for mode-less channels (like +chan)
        private void EventErrNoChanModes(IrcMessageData ircData)
        {
            string channelName = ircData.RawMessageArray[3];
            if (ActiveChannelSyncing && IsJoined(channelName))
            {
                Channel channel = GetChannel(channelName);
                if (channel.IsSynced)
                {
                    // only fire the event once
                    return;
                }

                channel.ActiveSyncStop = DateTime.Now;
                channel.IsSynced = true;
                DispatchEvent(this, OnChannelActiveSynced, new IrcEventArgs(ircData));
            }
        }

        private void EventErr(IrcMessageData ircData)
        {
            DispatchEvent(this, OnErrorMessage, new IrcEventArgs(ircData));
        }

        private void EventErrNickNameInUse(IrcMessageData ircData)
        {
            if (!AutoNickHandling)
            {
                return;
            }

            string nick;
            // if a nicks has been given loop through the nicknames
            // if the upper limit of this list has been reached and still no nickname has registered
            // then generate a random nick
            if (_currentNickname == NicknameList.Length - 1)
            {
                var rand = new Random();
                int number = rand.Next(999);
                if (Nickname.Length > 5)
                {
                    nick = Nickname.Substring(0, 5) + number;
                }
                else
                {
                    nick = Nickname.Substring(0, Nickname.Length - 1) + number;
                }
            }
            else
            {
                nick = NextNickname();
            }
            // change the nickname
            RfcNick(nick, Priority.Critical);
        }

        #endregion Internal Messagehandlers
    }
}