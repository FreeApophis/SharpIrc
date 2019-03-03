/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpIrc.IrcClient
{
    /// <summary>
    /// Description of ServerProperties.
    /// </summary>
    public class ServerProperties
    {
        public void Parse(string line)
        {
            string[] parameters = line.Substring(0, line.LastIndexOf(":", StringComparison.Ordinal) - 1).Split(' ');

            foreach (string s in parameters.Skip(3))
            {
                string[] pv = s.Split('=');
                _raw.Add(pv[0], ((pv.Length > 1) ? pv[1] : "TRUE"));

                // Boolean value;
                switch (pv[0])
                {
                    case "EXCEPTS":
                        BanException = true;
                        break;
                    case "INVEX":
                        InviteExceptions = true;
                        break;
                    case "WALLCHOPS":
                        _wAllChannelOps = true;
                        break;
                    case "WALLVOICES":
                        _wAllVoices = true;
                        break;
                    case "RFC2812":
                        RfC2812 = true;
                        break;
                    case "PENALTY":
                        Penalty = true;
                        break;
                    case "FNC":
                        ForcedNickChange = true;
                        break;
                    case "SAFELIST":
                        SafeList = true;
                        break;
                    case "NOQUIT":
                        NoQuit = true;
                        break;
                    case "USERIP":
                        UserIp = true;
                        break;
                    case "CPRIVMSG":
                        CPrivateMessage = true;
                        break;
                    case "CNOTICE":
                        CNotice = true;
                        break;
                    case "KNOCK":
                        Knock = true;
                        break;
                    case "VCHANS":
                        VirtualChannels = true;
                        break;
                    case "WHOX":
                        WhoX = true;
                        break;
                    case "CALLERID":
                        ModeG = true;
                        break;
                    case "IRCD":
                        _ircDaemon = pv[1];
                        break;
                    case "PREFIX":
                        _nickPrefix = pv[1];
                        break;
                    case "CHANTYPES":
                        _channelTypes = pv[1];
                        break;
                    case "CHANMODES":
                        _channelModes = pv[1];
                        break;
                    case "MODES":
                        MaxChannelModes = int.Parse(pv[1]);
                        break;
                    case "MAXCHANNELS":
                        maxChannels = int.Parse(pv[1]);
                        break;
                    case "CHANLIMIT":
                        maxChannelsByType = pv[1];
                        break;
                    case "NICKLEN":
                        MaxNickLength = int.Parse(pv[1]);
                        break;
                    case "MAXBANS":
                        _maxBans = int.Parse(pv[1]);
                        break;
                    case "MAXLIST":
                        _maxList = pv[1];
                        break;
                    case "NETWORK":
                        NetworkName = pv[1];
                        break;
                    case "STATUSMSG":
                        _statusMessage = pv[1];
                        break;
                    case "CASEMAPPING":
                        if (pv[1] == "ascii") CaseMapping = CaseMappingType.Ascii;
                        if (pv[1] == "rfc1459") CaseMapping = CaseMappingType.Rfc1459;
                        if (pv[1] == "strict-rfc1459") CaseMapping = CaseMappingType.Rfc1459Strict;
                        break;
                    case "ELIST":
                        _extendedListCommand = pv[1];
                        break;
                    case "TOPICLEN":
                        MaxTopicLength = int.Parse(pv[1]);
                        break;
                    case "KICKLEN":
                        MaxKickLength = int.Parse(pv[1]);
                        break;
                    case "CHANNELLEN":
                        MaxChannelLength = int.Parse(pv[1]);
                        break;
                    case "CHIDLEN":
                        _channelIdLength = int.Parse(pv[1]);
                        break;
                    case "IDCHAN":
                        _channelIdLengthByType = pv[1];
                        break;
                    case "STD":
                        IrcStandard = pv[1];
                        break;
                    case "SILENCE":
                        MaxSilence = int.Parse(pv[1]);
                        break;
                    case "AWAYLEN":
                        MaxAwayLength = int.Parse(pv[1]);
                        break;
                    case "MAXTARGETS":
                        MaxTargets = int.Parse(pv[1]);
                        break;
                    case "WATCH":
                        MaxWatch = int.Parse(pv[1]);
                        break;
                    case "LANGUAGE":
                        _language = pv[1];
                        break;
                    case "KEYLEN":
                        _maxKeyLength = int.Parse(pv[1]);
                        break;
                    case "USERLEN":
                        MaxUserLength = int.Parse(pv[1]);
                        break;
                    case "HOSTLEN":
                        MaxHostLength = int.Parse(pv[1]);
                        break;
                    case "CMDS":
                        SetCommands(pv[1]);
                        break;
                    case "MAXNICKLEN":
                        MaxNickLength = int.Parse(pv[1]);
                        break;
                    case "MAXCHANNELLEN":
                        MaxChannelLength = int.Parse(pv[1]);
                        break;
                    case "MAP":
                        Map = true;
                        break;
                    case "TARGMAX":
                        MaxTargetsByCommand = pv[1];
                        break;
                    default:
                        break;
                }
            }
        }

        internal ServerProperties()
        {
            DccAllow = false;
            ModeG = false;
            WhoX = false;
            MaxWatch = 0;
        }

        private void SetCommands(string commandList)
        {
            foreach (string command in commandList.Split(','))
            {
                switch (command)
                {
                    case "KNOCK":
                        Knock = true;
                        break;
                    case "MAP":
                        Map = true;
                        break;
                    case "DCCALLOW":
                        DccAllow = true;
                        break;
                    case "USERIP":
                        UserIp = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private readonly Dictionary<string, string> _raw = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Safe Access to All Values Sent in RPL_ISUPPORT
        /// If a Value is not Set this will Return "FALSE" (this can also mean: we don't know)
        /// If a Value has no Parameter this will Return "TRUE"
        /// The Index should be the Identifier sent by the Server like "PREFIX" (not case sensitive)
        /// </summary>
        public string this[string s] => _raw.ContainsKey(s) ? _raw[s] : "FALSE";

        public IEnumerable<string> KnownValues()
        {
            return _raw.Keys;
        }


        private string _ircDaemon = "unknown";
        /// <summary>
        /// Returns the Name of the IrcDaemon
        /// </summary>
        public string IrcDaemon => _ircDaemon;

        private string _channelTypes = string.Empty;

        public IEnumerable<char> ChannelTypes => _channelTypes.ToCharArray();

        private string _channelModes = string.Empty;

        public IEnumerable<char> ChannelModes => _channelModes.Where(c => c != ',').ToList();

        public IEnumerable<char> GetChannelModes(ChannelModeType channelModeType)
        {
            var temp = new List<char>();
            string[] modes = _channelModes.Split(',');
            if (modes.Length < 4)
                return temp;
            if (channelModeType == ChannelModeType.WithUserhostParameter)
            {
                AddCharsToList(ref temp, ref modes[0]);
            }
            if (channelModeType == ChannelModeType.WithAlwaysParamter)
            {
                AddCharsToList(ref temp, ref modes[1]);
            }
            if (channelModeType == ChannelModeType.WithSetOnlyParameter)
            {
                AddCharsToList(ref temp, ref modes[2]);
            }
            if (channelModeType == ChannelModeType.WithoutParameter)
            {
                AddCharsToList(ref temp, ref modes[3]);
            }
            return temp;
        }

        static void AddCharsToList(ref List<char> temp, ref string modes)
        {
            temp.AddRange(modes.Where(c => c != ','));
        }

        /// <summary>
        /// Returns the maximum number of channel modes with parameter allowed per MODE command (-1 if unknown)
        /// </summary>
        public int MaxChannelModes { get; private set; } = -1;

        private int maxChannels = -1;
        private string maxChannelsByType = string.Empty;


        /// <summary>
        /// Maximum number of channels allowed to join (# channels);
        /// </summary>
        public int MaxChannels => GetMaxChannels('#');

        /// <summary>
        /// Maximum number of channels allowed to join per Channel Type;
        /// </summary>
        /// <param name="channelPrefix">On Which Type of channels (ex. '#')</param>
        /// <returns>Length of Channel ID</returns>
        public int GetMaxChannels(char channelPrefix)
        {
            Dictionary<char, int> pfn = ParsePfxNum(maxChannelsByType);

            return pfn.ContainsKey(channelPrefix) ? pfn[channelPrefix] : maxChannels;
        }

        /// <summary>
        /// Returns the the maximum allowed nickname length.
        /// </summary>
        public int MaxNickLength { get; private set; } = 9;

        private string _maxList = string.Empty;

        /// <summary>
        ///  Returns the maximal number of List entries in a List.
        /// </summary>
        /// <param name="listType">On Which type of List (ex. Ban: 'b' )</param>
        /// <returns>Maximal Length of List (of type listType)</returns>
        public int GetMaxList(char listType)
        {
            Dictionary<char, int> pfn = ParsePfxNum(_maxList);

            return pfn.ContainsKey(listType) ? pfn[listType] : -1;
        }

        private int _maxBans = -1;
        /// <summary>
        /// Maximum number of bans per channel.
        /// Note: This Value is either from the MAXBANS or the MAXLIST Value
        /// </summary>
        public int MaxBans => Math.Max(_maxBans, GetMaxList('b'));

        /// <summary>
        /// Returns the Network Name if known
        /// </summary>
        public string NetworkName { get; private set; } = "unknown";

        /// <summary>
        /// Returns true if the server support ban exceptions (e mode).
        /// </summary>
        public bool BanException { get; private set; }

        /// <summary>
        /// Returns true if the server support invite exceptions (+I mode).
        /// </summary>
        public bool InviteExceptions { get; private set; }


        private string _statusMessage = string.Empty;

        /// <summary>
        /// Returns a list of Prefixes which can be used before a Channel name to message nicks who have a certain status or higher.
        /// </summary>
        public IEnumerable<char> StatusMessage => _statusMessage.ToCharArray();

        private bool _wAllVoices;

        /// <summary>
        /// Returns true if the server supports messaging channel operators (NOTICE @#channel)
        /// Note: This uses either the WALLVOICES or STATUSMSG whichever was sent
        /// </summary>
        public bool WAllVoices
        {
            get
            {
                bool statusContainsVoice = StatusMessage.Any(c => c == '+');

                return (_wAllVoices || statusContainsVoice);
            }
        }

        private bool _wAllChannelOps;

        /// <summary>
        /// Returns true if the server supports messaging channel operators (NOTICE @#channel)
        /// Note: This uses either the WALLCHANOPS or STATUSMSG whichever was sent
        /// </summary>
        public bool WAllChannelOps
        {
            get
            {
                bool statusContainsOp = StatusMessage.Any(c => c == '@');

                return (_wAllChannelOps || statusContainsOp);
            }
        }

        /// <summary>
        /// Returns the used Case Mapping type ascii or rfc1459
        /// </summary>
        public CaseMappingType CaseMapping { get; private set; } = CaseMappingType.Unknown;


        private string _extendedListCommand = string.Empty;

        /// <summary>
        /// Returns an Enum with all List extensions possible on the server
        /// </summary>
        public EListType ExtendedListCommand => _extendedListCommand.Aggregate<char, EListType>(0, (current, c) => current | (EListType)Enum.Parse(typeof(EListType), c.ToString()));

        /// <summary>
        /// Retruns the maximal allowed Length of a channel topic if known (-1 otherwise)
        /// </summary>
        public int MaxTopicLength { get; private set; } = -1;

        /// <summary>
        /// Retruns the maximal allowed Length of a kick message if known (-1 otherwise)
        /// </summary>
        public int MaxKickLength { get; private set; } = -1;

        /// <summary>
        /// Retruns the maximal allowed Length of a channel name
        /// </summary>
        public int MaxChannelLength { get; private set; } = 50;

        private int _channelIdLength = 5; // Rfc 2811
        private string _channelIdLengthByType = string.Empty;


        /// <summary>
        /// Returns the ID length for channels (! channels);
        /// </summary>
        public int ChannelIdLength => GetChannelIdLength('!');


        /// <summary>
        ///  with an ID. The prefix says for which channel type it is.
        /// </summary>
        /// <param name="channelPrefix">On Which Type of channels (ex. '#')</param>
        /// <returns>Length of Channel ID</returns>
        public int GetChannelIdLength(char channelPrefix)
        {
            Dictionary<char, int> pfn = ParsePfxNum(_channelIdLengthByType);

            return pfn.ContainsKey(channelPrefix) ? pfn[channelPrefix] : _channelIdLength;
        }

        private static Dictionary<char, int> ParsePfxNum(string toParse)
        {
            var result = new Dictionary<char, int>();
            foreach (string sr in toParse.Split(','))
            {
                string[] ssr = sr.Split(':');  // ssr[0] list of chars, ssr[1] numeric value
                foreach (char c in ssr[0])
                {
                    result.Add(c, int.Parse(ssr[1]));
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the used Irc-Standard if known
        /// </summary>
        public string IrcStandard { get; private set; } = "none";


        /// <summary>
        /// If the server supports the SILENCE command. The number is the maximum number of allowed entries in the list. (0 otherwise)
        /// </summary>
        public int MaxSilence { get; private set; }

        /// <summary>
        /// Server supports Rfc2812 Features beyond Rfc1459
        /// </summary>
        public bool RfC2812 { get; private set; }

        /// <summary>
        /// Server gives extra penalty to some commands instead of the normal 2 seconds per message and 1 second for every 120 bytes in a message.
        /// </summary>
        public bool Penalty { get; private set; }

        /// <summary>
        /// Forced nick changes: The server may change the nickname without the client sending a NICK message.
        /// </summary>
        public bool ForcedNickChange { get; private set; }

        /// <summary>
        /// The LIST is sent in multiple iterations so send queue won't fill and kill the client connection.
        /// </summary>
        public bool SafeList { get; private set; }

        /// <summary>
        /// The maximum length of an away message, returns -1 if not known
        /// </summary>
        public int MaxAwayLength { get; private set; } = -1;

        /// <summary>
        /// NOQUIT
        /// </summary>
        public bool NoQuit { get; private set; }

        /// <summary>
        /// Returns true if the Server supports the Userip Command
        /// </summary>
        public bool UserIp { get; private set; }

        /// <summary>
        /// Returns true if the Server supports the CPrivMsg Command
        /// </summary>
        public bool CPrivateMessage { get; private set; }

        /// <summary>
        /// Returns true if the Server supports the CNotice Command
        /// </summary>
        public bool CNotice { get; private set; }

        /// <summary>
        /// Returns the maximum number of targets for PrivMsg and Notice
        /// </summary>
        public int MaxTargets { get; private set; } = 1;

        /// <summary>
        /// Returns the MAXTARGETS String (unparsed);
        /// </summary>
        public string MaxTargetsByCommand { get; private set; } = string.Empty;

        /// <summary>
        /// Returns true if the Server supports the Knock Command
        /// </summary>
        public bool Knock { get; private set; }

        /// <summary>
        /// Returns true if the Server supports Virtual Channels
        /// </summary>
        public bool VirtualChannels { get; private set; }

        /// <summary>
        /// Returns how many Users can be on the watch list, returns 0 if the Watch command is not available.
        /// </summary>
        public int MaxWatch { get; private set; }

        /// <summary>
        /// Returns true if the Server  uses WHOX protocol for the Who command.
        /// </summary>
        public bool WhoX { get; private set; }

        /// <summary>
        /// Returns true if the server supports server side ignores via the +g user mode.
        /// </summary>
        public bool ModeG { get; private set; }

        private string _language = string.Empty;

        /// <summary>
        /// Returns a list of Languages if the Server supports the Language command.
        /// </summary>
        public IEnumerable<string> Languages => _language.Split(',');


        private string _nickPrefix = string.Empty;

        /// <summary>
        /// Returns a dictionary with user modes and user prefixes for channels.
        /// If we don't have values from the servers you can assume at least +ov / @+ are supported
        /// However the dictionary will be empty!
        /// Key = Mode, Value = Prefix, ex. NickPrefix['o'] = '@'
        /// Note: Some servers only show the most powerful, others may show all of them.
        /// </summary>
        public Dictionary<char, char> NickPrefix
        {
            get
            {
                string[] np = _nickPrefix.Split(')');
                var temp = new Dictionary<char, char>();
                int i = 0;
                foreach (char c in np[1])
                {
                    i++;
                    temp.Add(np[0][i], c);
                }
                return temp;
            }
        }

        private int _maxKeyLength = -1;

        /// <summary>
        /// Returns the maximum allowed Key length on this server or -1 if unknown
        /// </summary>
        public int MaxKeyLength => _maxKeyLength;

        /// <summary>
        ///  Returns the Maximum allowed User length on this server or -1 if unknown
        /// </summary>
        public int MaxUserLength { get; private set; } = -1;

        /// <summary>
        ///  Returns the Maximum allowed Host length on this server or -1 if unknown
        /// </summary>
        public int MaxHostLength { get; private set; } = -1;

        /// <summary>
        /// Returns true if we know this server supports the Map Command
        /// </summary>
        public bool Map { get; private set; }

        /// <summary>
        /// Server Supports the DccAllow Command
        /// </summary>
        public bool DccAllow { get; private set; }

        #region foreach
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, string>>)_raw).GetEnumerator();
        }
        #endregion foreach
    }
}
