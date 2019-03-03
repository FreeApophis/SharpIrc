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
        private readonly SharpIrc.IrcClient.IrcClient _irc;

        /// <summary>
        /// Constructor to create an instance of IrcMessageData
        /// </summary>
        /// <param name="ircClient">IrcClient the message originated from</param>
        /// <param name="from">combined nickname, identity and host of the user that sent the message (nick!ident@host)</param>
        /// <param name="nick">nickname of the user that sent the message</param>
        /// <param name="ident">identity (username) of the user that sent the message</param>
        /// <param name="host">hostname of the user that sent the message</param>
        /// <param name="channel">channel the message originated from</param>
        /// <param name="message">message</param>
        /// <param name="rawMessage">raw message sent by the server</param>
        /// <param name="type">message type</param>
        /// <param name="replycode">message reply code</param>
        public IrcMessageData(SharpIrc.IrcClient.IrcClient ircClient, string from, string nick, string ident, string host, string channel, string message, string rawMessage, ReceiveType type, ReplyCode replycode)
        {
            _irc = ircClient;
            RawMessage = rawMessage;
            RawMessageArray = rawMessage.Split(' ');
            Type = type;
            ReplyCode = replycode;
            From = from;
            Nick = nick;
            Ident = ident;
            Host = host;
            Channel = channel;

            // message is optional
            if (message == null) return;

            Message = message;
            MessageArray = message.Split(' ');
        }

        /// <summary>
        /// Constructor to create an instance of IrcMessageData
        /// </summary>
        /// <param name="ircClient">IrcClient the message originated from</param>
        /// <param name="rawMessage">message as it appears on wire, stripped of newline</param>
        public IrcMessageData(IrcClient ircClient, string rawMessage)
        {

            if (rawMessage == null)
            {
                throw new ArgumentNullException(nameof(rawMessage));
            }
            if (rawMessage == "")
            {
                throw new ArgumentException("Cannot parse empty message");
            }

            _irc = ircClient;
            RawMessage = rawMessage;
            RawMessageArray = rawMessage.Split(' ');
            From = "";
            Message = "";

            int start = 0;
            int len = 0;
            if (RawMessageArray[0][0] == ':')
            {
                From = RawMessageArray[0].Substring(1);
                start = 1;
                len += From.Length + 1;
            }

            Command = RawMessageArray[start];
            len += Command.Length + 1;

            int length = RawMessageArray.Length;

            if (start + 1 < length)
            {
                for (int i = start + 1; i < RawMessageArray.Length; i++)
                {
                    if (RawMessageArray[i][0] == ':')
                    {
                        length = i;
                        break;
                    }
                    len += RawMessageArray[i].Length + 1;
                }

                Args = new string[length - start - 1];
                Array.Copy(RawMessageArray, start + 1, Args, 0, length - start - 1);
                if (length < RawMessageArray.Length)
                {
                    Message = RawMessage.Substring(RawMessage.IndexOf(':', len) + 1);
                    MessageArray = Message.Split(' ');
                }
            }
            else
            {
                Args = new string[0];
            }

            ReplyCode = ReplyCode.Null;
            Type = ReceiveType.Unknown;

            ParseLegacyInfo();
        }

        /// <summary>
        /// Gets the IrcClient object the message originated from
        /// </summary>
        public SharpIrc.IrcClient.IrcClient Irc => _irc;

        /// <summary>
        /// Gets the combined nickname, identity and hostname of the user that sent the message
        /// </summary>
        /// <example>
        /// nick!ident@host
        /// </example>
        public string From { get; }

        /// <summary>
        /// Gets the nickname of the user that sent the message
        /// </summary>
        public string Nick { get; private set; }

        /// <summary>
        /// Gets the identity (username) of the user that sent the message
        /// </summary>
        public string Ident { get; private set; }

        /// <summary>
        /// Gets the hostname of the user that sent the message
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Gets the channel the message originated from
        /// </summary>
        public string Channel { get; private set; }

        /// <summary>
        /// Gets the message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the message as an array of strings (splitted by space)
        /// </summary>
        public string[] MessageArray { get; }

        /// <summary>
        /// Gets the raw message sent by the server
        /// </summary>
        public string RawMessage { get; }

        /// <summary>
        /// Gets the raw message sent by the server as array of strings (splitted by space)
        /// </summary>
        public string[] RawMessageArray { get; }

        /// <summary>
        /// Gets the message type
        /// </summary>
        public ReceiveType Type { get; private set; }

        /// <summary>
        /// Gets the message reply code
        /// </summary>
        public ReplyCode ReplyCode { get; private set; }

        /// <summary>
        /// Gets the message prefix
        /// </summary>
        public string Prefix => From;

        /// <summary>
        /// Gets the message command word
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Gets the message arguments
        /// </summary>
        public string[] Args { get; }

        /// <summary>
        /// Gets the message trailing argument
        /// </summary>
        public string Rest => Message;

        // refactored old field parsing code below, ignore for own sanity
        private void ParseLegacyInfo()
        {
            Match match = PrefixRegex.Match(From);

            if (match.Success)
            {
                if (match.Groups[2].Success || match.Groups[3].Success || (From.IndexOf('.') < 0))
                    Nick = match.Groups[1].ToString();
            }

            if (match.Groups[2].Success)
                Ident = match.Groups[2].ToString().Substring(1);
            if (match.Groups[3].Success)
                Host = match.Groups[3].ToString().Substring(1);

            if (int.TryParse(Command, out var code))
            {
                ReplyCode = (ReplyCode)code;
            }
            else
            {
                ReplyCode = ReplyCode.Null;
            }
            if (ReplyCode != ReplyCode.Null)
            {
                // categorize replies

                switch (ReplyCode)
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
                        Type = ReceiveType.Login;
                        break;

                    case ReplyCode.LuserClient:
                    case ReplyCode.LuserOp:
                    case ReplyCode.LuserUnknown:
                    case ReplyCode.LuserMe:
                    case ReplyCode.LuserChannels:
                        Type = ReceiveType.Info;
                        break;

                    case ReplyCode.MotdStart:
                    case ReplyCode.Motd:
                    case ReplyCode.EndOfMotd:
                        Type = ReceiveType.Motd;
                        break;

                    case ReplyCode.NamesReply:
                    case ReplyCode.EndOfNames:
                        Type = ReceiveType.Name;
                        break;

                    case ReplyCode.WhoReply:
                    case ReplyCode.EndOfWho:
                        Type = ReceiveType.Who;
                        break;

                    case ReplyCode.ListStart:
                    case ReplyCode.List:
                    case ReplyCode.ListEnd:
                        Type = ReceiveType.List;
                        break;

                    case ReplyCode.BanList:
                    case ReplyCode.EndOfBanList:
                        Type = ReceiveType.BanList;
                        break;

                    case ReplyCode.Topic:
                    case ReplyCode.NoTopic:
                        Type = ReceiveType.Topic;
                        break;

                    case ReplyCode.WhoIsUser:
                    case ReplyCode.WhoIsServer:
                    case ReplyCode.WhoIsOperator:
                    case ReplyCode.WhoIsIdle:
                    case ReplyCode.WhoIsChannels:
                    case ReplyCode.EndOfWhoIs:
                        Type = ReceiveType.WhoIs;
                        break;

                    case ReplyCode.WhoWasUser:
                    case ReplyCode.EndOfWhoWas:
                        Type = ReceiveType.WhoWas;
                        break;

                    case ReplyCode.UserModeIs:
                        Type = ReceiveType.UserMode;
                        break;

                    case ReplyCode.ChannelModeIs:
                        Type = ReceiveType.ChannelMode;
                        break;

                    default:
                        if ((code >= 400) &&
                            (code <= 599))
                        {
                            Type = ReceiveType.ErrorMessage;
                        }
                        else
                        {
                            Type = ReceiveType.Unknown;
                        }
                        break;
                }
            }
            else
            {
                // categorize commands

                switch (Command)
                {
                    case "PING":
                        Type = ReceiveType.Unknown;
                        break;

                    case "ERROR":
                        Type = ReceiveType.Error;
                        break;

                    case "PRIVMSG":
                        if (Args.Length > 0 && Message.StartsWith("\x1" + "ACTION") && Message.EndsWith("\x1"))
                        {
                            switch (Args[0][0])
                            {
                                case '#':
                                case '!':
                                case '&':
                                case '+':
                                    Type = ReceiveType.ChannelAction;
                                    break;

                                default:
                                    Type = ReceiveType.QueryAction;
                                    break;
                            }
                        }
                        else if (Message.StartsWith("\x1") && Message.EndsWith("\x1"))
                        {
                            Type = ReceiveType.CtcpRequest;
                        }
                        else if (Args.Length > 0)
                        {
                            switch (Args[0][0])
                            {
                                case '#':
                                case '!':
                                case '&':
                                case '+':
                                    Type = ReceiveType.ChannelMessage;
                                    break;

                                default:
                                    Type = ReceiveType.QueryMessage;
                                    break;
                            }
                        }
                        break;

                    case "NOTICE":
                        if (Message.StartsWith("\x1") && Message.EndsWith("\x1"))
                        {
                            Type = ReceiveType.CtcpReply;
                        }
                        else if (Args.Length > 0)
                        {
                            switch (Args[0][0])
                            {
                                case '#':
                                case '!':
                                case '&':
                                case '+':
                                    Type = ReceiveType.ChannelNotice;
                                    break;

                                default:
                                    Type = ReceiveType.QueryNotice;
                                    break;
                            }
                        }
                        break;

                    case "INVITE":
                        Type = ReceiveType.Invite;
                        break;

                    case "JOIN":
                        Type = ReceiveType.Join;
                        break;

                    case "PART":
                        Type = ReceiveType.Part;
                        break;

                    case "TOPIC":
                        Type = ReceiveType.TopicChange;
                        break;

                    case "NICK":
                        Type = ReceiveType.NickChange;
                        break;

                    case "KICK":
                        Type = ReceiveType.Kick;
                        break;

                    case "MODE":
                        switch (Args[0][0])
                        {
                            case '#':
                            case '!':
                            case '&':
                            case '+':
                                Type = ReceiveType.ChannelModeChange;
                                break;

                            default:
                                Type = ReceiveType.UserModeChange;
                                break;
                        }
                        break;

                    case "QUIT":
                        Type = ReceiveType.Quit;
                        break;

                    case "CAP":
                    case "AUTHENTICATE":
                        Type = ReceiveType.Other;
                        break;
                }
            }

            switch (Type)
            {
                case ReceiveType.Join:
                case ReceiveType.Kick:
                case ReceiveType.Part:
                case ReceiveType.TopicChange:
                case ReceiveType.ChannelModeChange:
                case ReceiveType.ChannelMessage:
                case ReceiveType.ChannelAction:
                case ReceiveType.ChannelNotice:
                    Channel = RawMessageArray[2];
                    break;

                case ReceiveType.Who:
                case ReceiveType.Topic:
                case ReceiveType.Invite:
                case ReceiveType.BanList:
                case ReceiveType.ChannelMode:
                    Channel = RawMessageArray[3];
                    break;

                case ReceiveType.Name:
                    Channel = RawMessageArray[4];
                    break;
            }

            switch (ReplyCode)
            {
                case ReplyCode.List:
                case ReplyCode.ListEnd:
                case ReplyCode.ErrorNoChannelModes:
                    Channel = Args[1];
                    break;
            }

            if (Channel != null && Channel.StartsWith(":"))
            {
                Channel = Channel.Substring(1);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder("[");

            sb.Append("<");
            sb.Append(From ?? "null");
            sb.Append("> ");

            sb.Append("<");
            sb.Append(Command ?? "null");
            sb.Append("> ");

            sb.Append("<");
            string sep = "";
            foreach (string a in (Args ?? new string[0]))
            {
                sb.Append(sep);
                sep = ", ";
                sb.Append(a);
            }
            sb.Append("> ");

            sb.Append("<");
            sb.Append(Message ?? "null");
            sb.Append("> ");

            sb.Append("(Type=");
            sb.Append(Type.ToString());
            sb.Append(") ");

            sb.Append("(Nick=");
            sb.Append(Nick ?? "null");
            sb.Append(") ");

            sb.Append("(Ident=");
            sb.Append(Ident ?? "null");
            sb.Append(") ");

            sb.Append("(Host=");
            sb.Append(Host ?? "null");
            sb.Append(") ");

            sb.Append("(Channel=");
            sb.Append(Channel ?? "null");
            sb.Append(") ");

            return sb.ToString();
        }
    }
}