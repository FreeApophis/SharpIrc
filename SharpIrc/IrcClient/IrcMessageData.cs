/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace SharpIrc.IrcClient
{
    /// <summary>
    /// This class contains an IRC message in a parsed form
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    [Serializable]
    public class IrcMessageData
    {
        private static readonly Regex PrefixRegex = new Regex("([^!@]+)(![^@]+)?(@.+)?");
        private readonly string[] args;
        private readonly string command;
        private readonly SharpIrc.IrcClient.IrcClient irc;
        private readonly string[] messageArray;
        private readonly string prefix;
        private readonly string rawMessage;
        private readonly string[] rawMessageArray;
        private readonly string rest;
        private string channel;
        private string host;
        private string ident;
        private string nick;
        private ReplyCode replyCode;
        private ReceiveType type;

        /// <summary>
        /// Constructor to create an instace of IrcMessageData
        /// </summary>
        /// <param name="ircclient">IrcClient the message originated from</param>
        /// <param name="from">combined nickname, identity and host of the user that sent the message (nick!ident@host)</param>
        /// <param name="nick">nickname of the user that sent the message</param>
        /// <param name="ident">identity (username) of the userthat sent the message</param>
        /// <param name="host">hostname of the user that sent the message</param>
        /// <param name="channel">channel the message originated from</param>
        /// <param name="message">message</param>
        /// <param name="rawmessage">raw message sent by the server</param>
        /// <param name="type">message type</param>
        /// <param name="replycode">message reply code</param>
        public IrcMessageData(SharpIrc.IrcClient.IrcClient ircclient, string from, string nick, string ident, string host, string channel, string message, string rawmessage, ReceiveType type, ReplyCode replycode)
        {
            irc = ircclient;
            rawMessage = rawmessage;
            rawMessageArray = rawmessage.Split(new[] { ' ' });
            this.type = type;
            replyCode = replycode;
            prefix = from;
            this.nick = nick;
            this.ident = ident;
            this.host = host;
            this.channel = channel;

            // message is optional
            if (message == null) return;

            rest = message;
            messageArray = message.Split(new[] { ' ' });
        }

        /// <summary>
        /// Constructor to create an instace of IrcMessageData
        /// </summary>
        /// <param name="ircClient">IrcClient the message originated from</param>
        /// <param name="rawMessage">message as it appears on wire, stripped of newline</param>
        public IrcMessageData(SharpIrc.IrcClient.IrcClient ircClient, string rawMessage)
        {

            if (rawMessage == null)
            {
                throw new ArgumentException("Cannot parse null message");
            }
            if (rawMessage == "")
            {
                throw new ArgumentException("Cannot parse empty message");
            }

            irc = ircClient;
            this.rawMessage = rawMessage;
            rawMessageArray = rawMessage.Split(' ');
            prefix = "";
            rest = "";

            int start = 0;
            int len = 0;
            if (rawMessageArray[0][0] == ':')
            {
                prefix = rawMessageArray[0].Substring(1);
                start = 1;
                len += prefix.Length + 1;
            }

            command = rawMessageArray[start];
            len += command.Length + 1;

            int length = rawMessageArray.Length;

            if (start + 1 < length)
            {
                for (int i = start + 1; i < rawMessageArray.Length; i++)
                {
                    if (rawMessageArray[i][0] == ':')
                    {
                        length = i;
                        break;
                    }
                    len += rawMessageArray[i].Length + 1;
                }

                args = new string[length - start - 1];
                Array.Copy(rawMessageArray, start + 1, args, 0, length - start - 1);
                if (length < rawMessageArray.Length)
                {
                    rest = this.rawMessage.Substring(this.rawMessage.IndexOf(':', len) + 1);
                    messageArray = rest.Split(' ');
                }
            }
            else
            {
                args = new string[0];
            }

            replyCode = ReplyCode.Null;
            type = ReceiveType.Unknown;

            ParseLegacyInfo();
        }

        /// <summary>
        /// Gets the IrcClient object the message originated from
        /// </summary>
        public SharpIrc.IrcClient.IrcClient Irc
        {
            get { return irc; }
        }

        /// <summary>
        /// Gets the combined nickname, identity and hostname of the user that sent the message
        /// </summary>
        /// <example>
        /// nick!ident@host
        /// </example>
        public string From
        {
            get { return prefix; }
        }

        /// <summary>
        /// Gets the nickname of the user that sent the message
        /// </summary>
        public string Nick
        {
            get { return nick; }
        }

        /// <summary>
        /// Gets the identity (username) of the user that sent the message
        /// </summary>
        public string Ident
        {
            get { return ident; }
        }

        /// <summary>
        /// Gets the hostname of the user that sent the message
        /// </summary>
        public string Host
        {
            get { return host; }
        }

        /// <summary>
        /// Gets the channel the message originated from
        /// </summary>
        public string Channel
        {
            get { return channel; }
        }

        /// <summary>
        /// Gets the message
        /// </summary>
        public string Message
        {
            get { return rest; }
        }

        /// <summary>
        /// Gets the message as an array of strings (splitted by space)
        /// </summary>
        public string[] MessageArray
        {
            get { return messageArray; }
        }

        /// <summary>
        /// Gets the raw message sent by the server
        /// </summary>
        public string RawMessage
        {
            get { return rawMessage; }
        }

        /// <summary>
        /// Gets the raw message sent by the server as array of strings (splitted by space)
        /// </summary>
        public string[] RawMessageArray
        {
            get { return rawMessageArray; }
        }

        /// <summary>
        /// Gets the message type
        /// </summary>
        public ReceiveType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Gets the message reply code
        /// </summary>
        public ReplyCode ReplyCode
        {
            get { return replyCode; }
        }

        /// <summary>
        /// Gets the message prefix
        /// </summary>
        public string Prefix
        {
            get { return prefix; }
        }

        /// <summary>
        /// Gets the message command word
        /// </summary>
        public string Command
        {
            get { return command; }
        }

        /// <summary>
        /// Gets the message arguments
        /// </summary>
        public string[] Args
        {
            get { return args; }
        }

        /// <summary>
        /// Gets the message trailing argument
        /// </summary>
        public string Rest
        {
            get { return rest; }
        }

        // refactored old field parsing code below, ignore for own sanity
        private void ParseLegacyInfo()
        {
            Match match = PrefixRegex.Match(prefix);

            if (match.Success)
            {
                if (match.Groups[2].Success || match.Groups[3].Success || (prefix.IndexOf('.') < 0))
                    nick = match.Groups[1].ToString();
            }

            if (match.Groups[2].Success)
                ident = match.Groups[2].ToString().Substring(1);
            if (match.Groups[3].Success)
                host = match.Groups[3].ToString().Substring(1);

            int code;
            if (int.TryParse(command, out code))
            {
                replyCode = (ReplyCode)code;
            }
            else
            {
                replyCode = ReplyCode.Null;
            }
            if (replyCode != ReplyCode.Null)
            {
                // categorize replies

                switch (replyCode)
                {
                    case ReplyCode.Welcome:
                    case ReplyCode.YourHost:
                    case ReplyCode.Created:
                    case ReplyCode.MyInfo:
                    case ReplyCode.Bounce:
                    case ReplyCode.SaslSuccess:
                    case ReplyCode.SaslFailure1:
                    case ReplyCode.SaslFailure2:
                    case ReplyCode.SaslAbort:
                        type = ReceiveType.Login;
                        break;

                    case ReplyCode.LuserClient:
                    case ReplyCode.LuserOp:
                    case ReplyCode.LuserUnknown:
                    case ReplyCode.LuserMe:
                    case ReplyCode.LuserChannels:
                        type = ReceiveType.Info;
                        break;

                    case ReplyCode.MotdStart:
                    case ReplyCode.Motd:
                    case ReplyCode.EndOfMotd:
                        type = ReceiveType.Motd;
                        break;

                    case ReplyCode.NamesReply:
                    case ReplyCode.EndOfNames:
                        type = ReceiveType.Name;
                        break;

                    case ReplyCode.WhoReply:
                    case ReplyCode.EndOfWho:
                        type = ReceiveType.Who;
                        break;

                    case ReplyCode.ListStart:
                    case ReplyCode.List:
                    case ReplyCode.ListEnd:
                        type = ReceiveType.List;
                        break;

                    case ReplyCode.BanList:
                    case ReplyCode.EndOfBanList:
                        type = ReceiveType.BanList;
                        break;

                    case ReplyCode.Topic:
                    case ReplyCode.NoTopic:
                        type = ReceiveType.Topic;
                        break;

                    case ReplyCode.WhoIsUser:
                    case ReplyCode.WhoIsServer:
                    case ReplyCode.WhoIsOperator:
                    case ReplyCode.WhoIsIdle:
                    case ReplyCode.WhoIsChannels:
                    case ReplyCode.EndOfWhoIs:
                        type = ReceiveType.WhoIs;
                        break;

                    case ReplyCode.WhoWasUser:
                    case ReplyCode.EndOfWhoWas:
                        type = ReceiveType.WhoWas;
                        break;

                    case ReplyCode.UserModeIs:
                        type = ReceiveType.UserMode;
                        break;

                    case ReplyCode.ChannelModeIs:
                        type = ReceiveType.ChannelMode;
                        break;

                    default:
                        if ((code >= 400) &&
                            (code <= 599))
                        {
                            type = ReceiveType.ErrorMessage;
                        }
                        else
                        {
                            type = ReceiveType.Unknown;
                        }
                        break;
                }
            }
            else
            {
                // categorize commands

                switch (command)
                {
                    case "PING":
                        type = ReceiveType.Unknown;
                        break;

                    case "ERROR":
                        type = ReceiveType.Error;
                        break;

                    case "PRIVMSG":
                        if (args.Length > 0 && rest.StartsWith("\x1" + "ACTION") && rest.EndsWith("\x1"))
                        {
                            switch (args[0][0])
                            {
                                case '#':
                                case '!':
                                case '&':
                                case '+':
                                    type = ReceiveType.ChannelAction;
                                    break;

                                default:
                                    type = ReceiveType.QueryAction;
                                    break;
                            }
                        }
                        else if (rest.StartsWith("\x1") && rest.EndsWith("\x1"))
                        {
                            type = ReceiveType.CtcpRequest;
                        }
                        else if (args.Length > 0)
                        {
                            switch (args[0][0])
                            {
                                case '#':
                                case '!':
                                case '&':
                                case '+':
                                    type = ReceiveType.ChannelMessage;
                                    break;

                                default:
                                    type = ReceiveType.QueryMessage;
                                    break;
                            }
                        }
                        break;

                    case "NOTICE":
                        if (rest.StartsWith("\x1") && rest.EndsWith("\x1"))
                        {
                            type = ReceiveType.CtcpReply;
                        }
                        else if (args.Length > 0)
                        {
                            switch (args[0][0])
                            {
                                case '#':
                                case '!':
                                case '&':
                                case '+':
                                    type = ReceiveType.ChannelNotice;
                                    break;

                                default:
                                    type = ReceiveType.QueryNotice;
                                    break;
                            }
                        }
                        break;

                    case "INVITE":
                        type = ReceiveType.Invite;
                        break;

                    case "JOIN":
                        type = ReceiveType.Join;
                        break;

                    case "PART":
                        type = ReceiveType.Part;
                        break;

                    case "TOPIC":
                        type = ReceiveType.TopicChange;
                        break;

                    case "NICK":
                        type = ReceiveType.NickChange;
                        break;

                    case "KICK":
                        type = ReceiveType.Kick;
                        break;

                    case "MODE":
                        switch (args[0][0])
                        {
                            case '#':
                            case '!':
                            case '&':
                            case '+':
                                type = ReceiveType.ChannelModeChange;
                                break;

                            default:
                                type = ReceiveType.UserModeChange;
                                break;
                        }
                        break;

                    case "QUIT":
                        type = ReceiveType.Quit;
                        break;

                    case "CAP":
                    case "AUTHENTICATE":
                        type = ReceiveType.Other;
                        break;
                }
            }

            switch (type)
            {
                case ReceiveType.Join:
                case ReceiveType.Kick:
                case ReceiveType.Part:
                case ReceiveType.TopicChange:
                case ReceiveType.ChannelModeChange:
                case ReceiveType.ChannelMessage:
                case ReceiveType.ChannelAction:
                case ReceiveType.ChannelNotice:
                    channel = rawMessageArray[2];
                    break;

                case ReceiveType.Who:
                case ReceiveType.Topic:
                case ReceiveType.Invite:
                case ReceiveType.BanList:
                case ReceiveType.ChannelMode:
                    channel = rawMessageArray[3];
                    break;

                case ReceiveType.Name:
                    channel = rawMessageArray[4];
                    break;
            }

            switch (replyCode)
            {
                case ReplyCode.List:
                case ReplyCode.ListEnd:
                case ReplyCode.ErrorNoChannelModes:
                    channel = args[1];
                    break;
            }

            if (channel != null && channel.StartsWith(":"))
            {
                channel = Channel.Substring(1);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder("[");

            sb.Append("<");
            sb.Append(prefix ?? "null");
            sb.Append("> ");

            sb.Append("<");
            sb.Append(command ?? "null");
            sb.Append("> ");

            sb.Append("<");
            string sep = "";
            foreach (string a in (args ?? new string[0]))
            {
                sb.Append(sep);
                sep = ", ";
                sb.Append(a);
            }
            sb.Append("> ");

            sb.Append("<");
            sb.Append(rest ?? "null");
            sb.Append("> ");

            sb.Append("(Type=");
            sb.Append(type.ToString());
            sb.Append(") ");

            sb.Append("(Nick=");
            sb.Append(nick ?? "null");
            sb.Append(") ");

            sb.Append("(Ident=");
            sb.Append(ident ?? "null");
            sb.Append(") ");

            sb.Append("(Host=");
            sb.Append(host ?? "null");
            sb.Append(") ");

            sb.Append("(Channel=");
            sb.Append(channel ?? "null");
            sb.Append(") ");

            return sb.ToString();
        }
    }
}