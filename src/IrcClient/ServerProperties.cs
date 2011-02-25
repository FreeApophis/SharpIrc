/*
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
* Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch> <http://www.apophis.ch>
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
using System.Collections.Generic;
using System.Linq;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// Description of ServerProperties.
    /// </summary>
    public class ServerProperties
    {
        public void Parse(string line)
        {
            string[] parameters = line.Split(new[] { ' ' });
            foreach (string s in parameters.Skip(3))
            {
                string[] pv = s.Split(new[] { '=' });
                raw.Add(pv[0], ((pv.Length > 1) ? pv[1] : "TRUE"));

                // Boolean value;
                switch (pv[0])
                {
                    case "EXCEPTS": BanException = true;
                        break;
                    case "INVEX": InviteExceptions = true;
                        break;
                    case "WALLCHOPS": wAllChannelOps = true;
                        break;
                    case "WALLVOICES": wAllVoices = true;
                        break;
                    case "RFC2812": RfC2812 = true;
                        break;
                    case "PENALTY": Penalty = true;
                        break;
                    case "FNC": ForcedNickChange = true;
                        break;
                    case "SAFELIST": SafeList = true;
                        break;
                    case "NOQUIT": NoQuit = true;
                        break;
                    case "USERIP": UserIp = true;
                        break;
                    case "CPRIVMSG": CPrivateMessage = true;
                        break;
                    case "CNOTICE": CNotice = true;
                        break;
                    case "KNOCK": Knock = true;
                        break;
                    case "VCHANS": VirtualChannels = true;
                        break;
                    case "WHOX": WhoX = true;
                        break;
                    case "CALLERID": ModeG = true;
                        break;
                    case "IRCD": ircDaemon = pv[1];
                        break;
                    case "PREFIX": nickPrefix = pv[1];
                        break;
                    case "CHANTYPES": channelTypes = pv[1];
                        break;
                    case "CHANMODES": channelModes = pv[1];
                        break;
                    case "MODES": maxChannelModes = int.Parse(pv[1]);
                        break;
                    case "MAXCHANNELS": maxChannels = int.Parse(pv[1]);
                        break;
                    case "CHANLIMIT": maxChannelsByType = pv[1];
                        break;
                    case "NICKLEN": maxNickLength = int.Parse(pv[1]);
                        break;
                    case "MAXBANS": maxBans = int.Parse(pv[1]);
                        break;
                    case "MAXLIST": maxList = pv[1];
                        break;
                    case "NETWORK": networkName = pv[1];
                        break;
                    case "STATUSMSG": statusMessage = pv[1];
                        break;
                    case "CASEMAPPING":
                        if (pv[1] == "ascii") caseMapping = CaseMappingType.Ascii;
                        if (pv[1] == "rfc1459") caseMapping = CaseMappingType.Rfc1459;
                        if (pv[1] == "strict-rfc1459") caseMapping = CaseMappingType.Rfc1459Strict;
                        break;
                    case "ELIST": extendedListCommand = pv[1];
                        break;
                    case "TOPICLEN": maxTopicLength = int.Parse(pv[1]);
                        break;
                    case "KICKLEN": maxKickLength = int.Parse(pv[1]);
                        break;
                    case "CHANNELLEN": maxChannelLength = int.Parse(pv[1]);
                        break;
                    case "CHIDLEN": channelIDLength = int.Parse(pv[1]);
                        break;
                    case "IDCHAN": channelIDLengthByType = pv[1];
                        break;
                    case "STD": ircStandard = pv[1];
                        break;
                    case "SILENCE": MaxSilence = int.Parse(pv[1]);
                        break;
                    case "AWAYLEN": maxAwayLength = int.Parse(pv[1]);
                        break;
                    case "MAXTARGETS": maxTargets = int.Parse(pv[1]);
                        break;
                    case "WATCH": MaxWatch = int.Parse(pv[1]);
                        break;
                    case "LANGUAGE": language = pv[1];
                        break;
                    case "KEYLEN": maxKeyLength = int.Parse(pv[1]);
                        break;
                    case "USERLEN": maxUserLength = int.Parse(pv[1]);
                        break;
                    case "HOSTLEN": maxHostLength = int.Parse(pv[1]);
                        break;
                    case "CMDS": SetCommands(pv[1]);
                        break;
                    case "MAXNICKLEN": maxNickLength = int.Parse(pv[1]);
                        break;
                    case "MAXCHANNELLEN": maxChannelLength = int.Parse(pv[1]);
                        break;
                    case "MAP": Map = true;
                        break;
                    case "TARGMAX": maxTargetsByCommand = pv[1];
                        break;
                    default:
#if LOG4NET
                        Logger.MessageParser.Warn(pv[0] + " is not parsed yet but has value: " + ((pv.Length > 1) ? pv[1] : "true"));
#endif
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
            foreach (string command in commandList.Split(new[] { ',' }))
            {
                switch (command)
                {
                    case "KNOCK": Knock = true;
                        break;
                    case "MAP": Map = true;
                        break;
                    case "DCCALLOW": DccAllow = true;
                        break;
                    case "USERIP": UserIp = true;
                        break;
                    default:
#if LOG4NET
                        Logger.MessageParser.Warn("Unknown Command: " + command);
#endif
                        break;
                }
            }
        }

        private readonly Dictionary<string, string> raw = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Safe Access to All Values Sent in RPL_ISUPPORT
        /// If a Value is not Set this will Return "FALSE" (this can also mean: we don't know)
        /// If a Value has no Parameter this will Return "TRUE"
        /// The Index should be the Identifier sent by the Server like "PREFIX" (not casesensitive)
        /// </summary>
        public string this[string s]
        {
            get
            {
                return raw.ContainsKey(s) ? raw[s] : "FALSE";
            }
        }

        public IEnumerable<string> KnownValues()
        {
            return raw.Keys;
        }


        private string ircDaemon = "unknown";
        /// <summary>
        /// Returns the Name of the IrcDaemon
        /// </summary>
        public string IrcDaemon
        {
            get
            {
                return ircDaemon;
            }
        }

        private string channelTypes = string.Empty;

        public IEnumerable<char> ChannelTypes
        {
            get
            {
                return channelTypes.ToCharArray();
            }
        }

        private string channelModes = string.Empty;

        public IEnumerable<char> ChannelModes
        {
            get
            {
                return channelModes.Where(c => c != ',').ToList();
            }
        }

        public IEnumerable<char> GetChannelModes(ChannelModeType channelModeType)
        {
            var temp = new List<char>();
            string[] modes = channelModes.Split(new[] { ',' });
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

        private int maxChannelModes = -1;

        /// <summary>
        /// Returns the maximum number of channel modes with parameter allowed per MODE command (-1 if unknown)
        /// </summary>
        public int MaxChannelModes
        {
            get
            {
                return maxChannelModes;
            }
        }

        private int maxChannels = -1;
        private string maxChannelsByType = string.Empty;


        /// <summary>
        /// Maximum number of channels allowed to join (# channels);
        /// </summary>
        public int MaxChannels
        {
            get
            {
                return GetMaxChannels('#');
            }
        }

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

        private int maxNickLength = 9;  // Rfc 2812

        /// <summary>
        /// Returns the the maximum allowed nickname length.
        /// </summary>
        public int MaxNickLength
        {
            get
            {
                return maxNickLength;
            }
        }

        private string maxList = string.Empty;

        /// <summary>
        ///  Returns the maximal number of List entries in a List.
        /// </summary>
        /// <param name="listType">On Which type of List (ex. Ban: 'b' )</param>
        /// <returns>Maximal Length of List (of type listType)</returns>        
        public int GetMaxList(char listType)
        {
            Dictionary<char, int> pfn = ParsePfxNum(maxList);

            return pfn.ContainsKey(listType) ? pfn[listType] : -1;
        }

        private int maxBans = -1;
        /// <summary>
        /// Maximum number of bans per channel.
        /// Note: This Value is either from the MAXBANS or the MAXLIST Value
        /// </summary>
        public int MaxBans
        {
            get
            {
                return Math.Max(maxBans, GetMaxList('b'));
            }
        }

        private string networkName = "unknwon";

        /// <summary>
        /// Returns the Network Name if known
        /// </summary>
        public string NetworkName
        {
            get
            {
                return networkName;
            }
        }

        /// <summary>
        /// Returns true if the server support ban exceptions (e mode).
        /// </summary>
        public bool BanException { get; private set; }

        /// <summary>
        /// Returns true if the server support invite exceptions (+I mode).
        /// </summary>
        public bool InviteExceptions { get; private set; }


        private string statusMessage = string.Empty;

        /// <summary>
        /// Returns a list of Prefixes which can be used before a Channel name to message nicks who have a certain status or higher.
        /// </summary>
        public IEnumerable<char> StatusMessage
        {
            get
            {
                return statusMessage.ToCharArray();
            }
        }

        private bool wAllVoices;

        /// <summary>
        /// Returns true if the server supports messaging channel operators (NOTICE @#channel)
        /// Note: This uses either the WALLVOICES or STATUSMSG whichever was sent
        /// </summary>
        public bool WAllVoices
        {
            get
            {
                bool statusContainsVoice = StatusMessage.Where(c => c == '+').Any();

                return (wAllVoices || statusContainsVoice);
            }
        }

        private bool wAllChannelOps;

        /// <summary>
        /// Returns true if the server supports messaging channel operators (NOTICE @#channel)
        /// Note: This uses either the WALLCHANOPS or STATUSMSG whichever was sent
        /// </summary>
        public bool WAllChannelOps
        {
            get
            {
                bool statusContainsOp = StatusMessage.Where(c => c == '@').Any();

                return (wAllChannelOps || statusContainsOp);
            }
        }

        private CaseMappingType caseMapping = CaseMappingType.Unknown;

        /// <summary>
        /// Returns the used Case Mapping type ascii or rfc1459
        /// </summary>
        public CaseMappingType CaseMapping
        {
            get
            {
                return caseMapping;
            }
        }


        private string extendedListCommand = string.Empty;

        /// <summary>
        /// Returns an Enum with all List Extentions possible on the server
        /// </summary>
        public EListType ExtendedListCommand
        {
            get
            {
                return extendedListCommand.Aggregate<char, EListType>(0, (current, c) => current | (EListType)Enum.Parse(typeof(EListType), c.ToString()));
            }
        }

        private int maxTopicLength = -1;

        /// <summary>
        /// Retruns the maximal allowed Length of a channel topic if known (-1 otherwise)
        /// </summary>
        public int MaxTopicLength
        {
            get
            {
                return maxTopicLength;
            }
        }

        private int maxKickLength = -1;

        /// <summary>
        /// Retruns the maximal allowed Length of a kick message if known (-1 otherwise)
        /// </summary>
        public int MaxKickLength
        {
            get
            {
                return maxKickLength;
            }
        }

        private int maxChannelLength = 50;  // Rfc 2812

        /// <summary>
        /// Retruns the maximal allowed Length of a channel name
        /// </summary>
        public int MaxChannelLength
        {
            get
            {
                return maxChannelLength;
            }
        }

        private int channelIDLength = 5; // Rfc 2811
        private string channelIDLengthByType = string.Empty;


        /// <summary>
        /// Returns the ID length for channels (! channels);
        /// </summary>
        public int ChannelIDLength
        {
            get
            {
                return GetChannelIDLength('!');
            }
        }


        /// <summary>
        ///  with an ID. The prefix says for which channel type it is.
        /// </summary>
        /// <param name="channelPrefix">On Which Type of channels (ex. '#')</param>
        /// <returns>Length of Channel ID</returns>
        public int GetChannelIDLength(char channelPrefix)
        {
            Dictionary<char, int> pfn = ParsePfxNum(channelIDLengthByType);

            return pfn.ContainsKey(channelPrefix) ? pfn[channelPrefix] : channelIDLength;
        }

        private static Dictionary<char, int> ParsePfxNum(string toParse)
        {
            var result = new Dictionary<char, int>();
            foreach (string sr in toParse.Split(new[] { ',' }))
            {
                string[] ssr = sr.Split(new[] { ':' });  // ssr[0] list of chars, ssr[1] numeric value
                foreach (char c in ssr[0])
                {
                    result.Add(c, int.Parse(ssr[1]));
                }
            }
            return result;
        }

        private string ircStandard = "none";

        /// <summary>
        /// Returns the used Irc-Standard if known
        /// </summary>
        public string IrcStandard
        {
            get
            {
                return ircStandard;
            }
        }


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

        private int maxAwayLength = -1;

        /// <summary>
        /// The maximum length of an away message, returns -1 if not known
        /// </summary>
        public int MaxAwayLength
        {
            get
            {
                return maxAwayLength;
            }
        }

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

        private int maxTargets = 1;
        private string maxTargetsByCommand = string.Empty;

        /// <summary>
        /// Returns the maximum number of targets for PrivMsg and Notice
        /// </summary>        
        public int MaxTargets
        {
            get
            {
                return maxTargets;
            }
        }

        /// <summary>
        /// Returns the MAXTARGETS String (unparsed);
        /// </summary>
        public string MaxTargetsByCommand
        {
            get
            {
                return maxTargetsByCommand;
            }
        }

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

        private string language = string.Empty;

        /// <summary>
        /// Returns a list of Languages if the Server supports the Language command.
        /// </summary>
        public IEnumerable<string> Languages
        {
            get
            {
                return language.Split(new[] { ',' });
            }
        }


        private string nickPrefix = string.Empty;

        /// <summary>
        /// Returns a Dictionary with Usermodes and Userprefixes for Channels.
        /// If we don't have values from the servers you can assume at least +ov / @+ are supported
        /// However the dictionary will be empty!
        /// Key = Mode, Value = Prefix, ex. NickPrefix['o'] = '@'
        /// Note: Some servers only show the most powerful, others may show all of them. 
        /// </summary>
        public Dictionary<char, char> NickPrefix
        {
            get
            {
                string[] np = nickPrefix.Split(new[] { ')' });
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

        private int maxKeyLength = -1;

        /// <summary>
        /// Returns the maximum allowed Key length on this server or -1 if unknown
        /// </summary>
        public int MaxKeyLength
        {
            get
            {
                return maxKeyLength;
            }
        }

        private int maxUserLength = -1;

        /// <summary>
        ///  Returns the Maximum allowed User length on this server or -1 if unknwon
        /// </summary>
        public int MaxUserLength
        {
            get
            {
                return maxUserLength;
            }
        }

        private int maxHostLength = -1;

        /// <summary>
        ///  Returns the Maximum allowed Host length on this server or -1 if unknwon
        /// </summary>
        public int MaxHostLength
        {
            get
            {
                return maxHostLength;
            }
        }

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
            return ((IEnumerable<KeyValuePair<string, string>>)raw).GetEnumerator();
        }

        #endregion
    }

    public enum CaseMappingType
    {
        Unknown,
        Rfc1459,
        Rfc1459Strict,
        Ascii

    }

    [Flags]
    public enum ChannelModeType
    {
        WithUserhostParameter,
        WithAlwaysParamter,
        WithSetOnlyParameter,
        WithoutParameter
    }

    /// <summary>
    /// M = mask search,
    /// N = !mask search
    /// U = usercount search (< >)
    /// C = creation time search (C< C>)
    /// T = topic search (T< T>) 
    /// </summary>
    [Flags]
    public enum EListType
    {

        M,
        N,
        U,
        C,
        T
    }
}
