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

namespace Meebey.SmartIrc4net
{
    /// <summary>
    ///
    /// </summary>
    public class ActionEventArgs : CtcpEventArgs
    {
        private readonly string _ActionMessage;

        internal ActionEventArgs(IrcMessageData data, string actionmsg) : base(data, "ACTION", actionmsg)
        {
            _ActionMessage = actionmsg;
        }

        public string ActionMessage
        {
            get { return _ActionMessage; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class CtcpEventArgs : IrcEventArgs
    {
        private readonly string _CtcpCommand;
        private readonly string _CtcpParameter;

        internal CtcpEventArgs(IrcMessageData data, string ctcpcmd, string ctcpparam) : base(data)
        {
            _CtcpCommand = ctcpcmd;
            _CtcpParameter = ctcpparam;
        }

        public string CtcpCommand
        {
            get { return _CtcpCommand; }
        }

        public string CtcpParameter
        {
            get { return _CtcpParameter; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class ErrorEventArgs : IrcEventArgs
    {
        private readonly string _ErrorMessage;

        internal ErrorEventArgs(IrcMessageData data, string errormsg) : base(data)
        {
            _ErrorMessage = errormsg;
        }

        public string ErrorMessage
        {
            get { return _ErrorMessage; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class MotdEventArgs : IrcEventArgs
    {
        private readonly string _MotdMessage;

        internal MotdEventArgs(IrcMessageData data, string motdmsg) : base(data)
        {
            _MotdMessage = motdmsg;
        }

        public string MotdMessage
        {
            get { return _MotdMessage; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class PingEventArgs : IrcEventArgs
    {
        private readonly string _PingData;

        internal PingEventArgs(IrcMessageData data, string pingdata) : base(data)
        {
            _PingData = pingdata;
        }

        public string PingData
        {
            get { return _PingData; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class PongEventArgs : IrcEventArgs
    {
        private readonly TimeSpan _Lag;

        internal PongEventArgs(IrcMessageData data, TimeSpan lag) : base(data)
        {
            _Lag = lag;
        }

        public TimeSpan Lag
        {
            get { return _Lag; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class KickEventArgs : IrcEventArgs
    {
        private readonly string _Channel;
        private readonly string _KickReason;
        private readonly string _Who;
        private readonly string _Whom;

        internal KickEventArgs(IrcMessageData data, string channel, string who, string whom, string kickreason)
            : base(data)
        {
            _Channel = channel;
            _Who = who;
            _Whom = whom;
            _KickReason = kickreason;
        }

        public string Channel
        {
            get { return _Channel; }
        }

        public string Who
        {
            get { return _Who; }
        }

        public string Whom
        {
            get { return _Whom; }
        }

        public string KickReason
        {
            get { return _KickReason; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class JoinEventArgs : IrcEventArgs
    {
        private readonly string _Channel;
        private readonly string _Who;

        internal JoinEventArgs(IrcMessageData data, string channel, string who) : base(data)
        {
            _Channel = channel;
            _Who = who;
        }

        public string Channel
        {
            get { return _Channel; }
        }

        public string Who
        {
            get { return _Who; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class NamesEventArgs : IrcEventArgs
    {
        private readonly string _Channel;
        private readonly string[] _UserList;

        internal NamesEventArgs(IrcMessageData data, string channel, string[] userlist) : base(data)
        {
            _Channel = channel;
            _UserList = userlist;
        }

        public string Channel
        {
            get { return _Channel; }
        }

        public string[] UserList
        {
            get { return _UserList; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class ListEventArgs : IrcEventArgs
    {
        private readonly ChannelInfo f_ListInfo;

        internal ListEventArgs(IrcMessageData data, ChannelInfo listInfo) : base(data)
        {
            f_ListInfo = listInfo;
        }

        public ChannelInfo ListInfo
        {
            get { return f_ListInfo; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class InviteEventArgs : IrcEventArgs
    {
        private readonly string _Channel;
        private readonly string _Who;

        internal InviteEventArgs(IrcMessageData data, string channel, string who) : base(data)
        {
            _Channel = channel;
            _Who = who;
        }

        public string Channel
        {
            get { return _Channel; }
        }

        public string Who
        {
            get { return _Who; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class PartEventArgs : IrcEventArgs
    {
        private readonly string _Channel;
        private readonly string _PartMessage;
        private readonly string _Who;

        internal PartEventArgs(IrcMessageData data, string channel, string who, string partmessage) : base(data)
        {
            _Channel = channel;
            _Who = who;
            _PartMessage = partmessage;
        }

        public string Channel
        {
            get { return _Channel; }
        }

        public string Who
        {
            get { return _Who; }
        }

        public string PartMessage
        {
            get { return _PartMessage; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class WhoEventArgs : IrcEventArgs
    {
        private readonly WhoInfo f_WhoInfo;

        internal WhoEventArgs(IrcMessageData data, WhoInfo whoInfo) : base(data)
        {
            f_WhoInfo = whoInfo;
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Channel
        {
            get { return f_WhoInfo.Channel; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Nick
        {
            get { return f_WhoInfo.Nick; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Ident
        {
            get { return f_WhoInfo.Ident; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Host
        {
            get { return f_WhoInfo.Host; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Realname
        {
            get { return f_WhoInfo.Realname; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public bool IsAway
        {
            get { return f_WhoInfo.IsAway; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public bool IsOp
        {
            get { return f_WhoInfo.IsOp; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public bool IsVoice
        {
            get { return f_WhoInfo.IsVoice; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public bool IsIrcOp
        {
            get { return f_WhoInfo.IsIrcOp; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Server
        {
            get { return f_WhoInfo.Server; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public int HopCount
        {
            get { return f_WhoInfo.HopCount; }
        }

        public WhoInfo WhoInfo
        {
            get { return f_WhoInfo; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class QuitEventArgs : IrcEventArgs
    {
        private readonly string _QuitMessage;
        private readonly string _Who;

        internal QuitEventArgs(IrcMessageData data, string who, string quitmessage) : base(data)
        {
            _Who = who;
            _QuitMessage = quitmessage;
        }

        public string Who
        {
            get { return _Who; }
        }

        public string QuitMessage
        {
            get { return _QuitMessage; }
        }
    }


    /// <summary>
    ///
    /// </summary>
    public class AwayEventArgs : IrcEventArgs
    {
        private readonly string _AwayMessage;
        private readonly string _Who;

        internal AwayEventArgs(IrcMessageData data, string who, string awaymessage) : base(data)
        {
            _Who = who;
            _AwayMessage = awaymessage;
        }

        public string Who
        {
            get { return _Who; }
        }

        public string AwayMessage
        {
            get { return _AwayMessage; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class NickChangeEventArgs : IrcEventArgs
    {
        private readonly string _NewNickname;
        private readonly string _OldNickname;

        internal NickChangeEventArgs(IrcMessageData data, string oldnick, string newnick) : base(data)
        {
            _OldNickname = oldnick;
            _NewNickname = newnick;
        }

        public string OldNickname
        {
            get { return _OldNickname; }
        }

        public string NewNickname
        {
            get { return _NewNickname; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class TopicEventArgs : IrcEventArgs
    {
        private readonly string _Channel;
        private readonly string _Topic;

        internal TopicEventArgs(IrcMessageData data, string channel, string topic) : base(data)
        {
            _Channel = channel;
            _Topic = topic;
        }

        public string Channel
        {
            get { return _Channel; }
        }

        public string Topic
        {
            get { return _Topic; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class TopicChangeEventArgs : IrcEventArgs
    {
        private readonly string _Channel;
        private readonly string _NewTopic;
        private readonly string _Who;

        internal TopicChangeEventArgs(IrcMessageData data, string channel, string who, string newtopic) : base(data)
        {
            _Channel = channel;
            _Who = who;
            _NewTopic = newtopic;
        }

        public string Channel
        {
            get { return _Channel; }
        }

        public string Who
        {
            get { return _Who; }
        }

        public string NewTopic
        {
            get { return _NewTopic; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class BanEventArgs : IrcEventArgs
    {
        private readonly string _Channel;
        private readonly string _Hostmask;
        private readonly string _Who;

        internal BanEventArgs(IrcMessageData data, string channel, string who, string hostmask) : base(data)
        {
            _Channel = channel;
            _Who = who;
            _Hostmask = hostmask;
        }

        public string Channel
        {
            get { return _Channel; }
        }

        public string Who
        {
            get { return _Who; }
        }

        public string Hostmask
        {
            get { return _Hostmask; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class UnbanEventArgs : IrcEventArgs
    {
        private readonly string _Channel;
        private readonly string _Hostmask;
        private readonly string _Who;

        internal UnbanEventArgs(IrcMessageData data, string channel, string who, string hostmask) : base(data)
        {
            _Channel = channel;
            _Who = who;
            _Hostmask = hostmask;
        }

        public string Channel
        {
            get { return _Channel; }
        }

        public string Who
        {
            get { return _Who; }
        }

        public string Hostmask
        {
            get { return _Hostmask; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class OpEventArgs : IrcEventArgs
    {
        private readonly string _Channel;
        private readonly string _Who;
        private readonly string _Whom;

        internal OpEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data)
        {
            _Channel = channel;
            _Who = who;
            _Whom = whom;
        }

        public string Channel
        {
            get { return _Channel; }
        }

        public string Who
        {
            get { return _Who; }
        }

        public string Whom
        {
            get { return _Whom; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class DeopEventArgs : IrcEventArgs
    {
        private readonly string _Channel;
        private readonly string _Who;
        private readonly string _Whom;

        internal DeopEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data)
        {
            _Channel = channel;
            _Who = who;
            _Whom = whom;
        }

        public string Channel
        {
            get { return _Channel; }
        }

        public string Who
        {
            get { return _Who; }
        }

        public string Whom
        {
            get { return _Whom; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class HalfopEventArgs : IrcEventArgs
    {
        private readonly string _Channel;
        private readonly string _Who;
        private readonly string _Whom;

        internal HalfopEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data)
        {
            _Channel = channel;
            _Who = who;
            _Whom = whom;
        }

        public string Channel
        {
            get { return _Channel; }
        }

        public string Who
        {
            get { return _Who; }
        }

        public string Whom
        {
            get { return _Whom; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class DehalfopEventArgs : IrcEventArgs
    {
        private readonly string _Channel;
        private readonly string _Who;
        private readonly string _Whom;

        internal DehalfopEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data)
        {
            _Channel = channel;
            _Who = who;
            _Whom = whom;
        }

        public string Channel
        {
            get { return _Channel; }
        }

        public string Who
        {
            get { return _Who; }
        }

        public string Whom
        {
            get { return _Whom; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class VoiceEventArgs : IrcEventArgs
    {
        private readonly string _Channel;
        private readonly string _Who;
        private readonly string _Whom;

        internal VoiceEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data)
        {
            _Channel = channel;
            _Who = who;
            _Whom = whom;
        }

        public string Channel
        {
            get { return _Channel; }
        }

        public string Who
        {
            get { return _Who; }
        }

        public string Whom
        {
            get { return _Whom; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class DevoiceEventArgs : IrcEventArgs
    {
        private readonly string _Channel;
        private readonly string _Who;
        private readonly string _Whom;

        internal DevoiceEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data)
        {
            _Channel = channel;
            _Who = who;
            _Whom = whom;
        }

        public string Channel
        {
            get { return _Channel; }
        }

        public string Who
        {
            get { return _Who; }
        }

        public string Whom
        {
            get { return _Whom; }
        }
    }
}