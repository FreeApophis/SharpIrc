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
        internal ActionEventArgs(IrcMessageData data, string actionmsg)
            : base(data, "ACTION", actionmsg)
        {
            ActionMessage = actionmsg;
        }

        public string ActionMessage { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class CtcpEventArgs : IrcEventArgs
    {
        internal CtcpEventArgs(IrcMessageData data, string ctcpcmd, string ctcpparam)
            : base(data)
        {
            CtcpCommand = ctcpcmd;
            CtcpParameter = ctcpparam;
        }

        public string CtcpCommand { get; private set; }

        public string CtcpParameter { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class ErrorEventArgs : IrcEventArgs
    {
        internal ErrorEventArgs(IrcMessageData data, string errormsg)
            : base(data)
        {
            ErrorMessage = errormsg;
        }

        public string ErrorMessage { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class MotdEventArgs : IrcEventArgs
    {
        internal MotdEventArgs(IrcMessageData data, string motdmsg)
            : base(data)
        {
            MotdMessage = motdmsg;
        }

        public string MotdMessage { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class PingEventArgs : IrcEventArgs
    {
        internal PingEventArgs(IrcMessageData data, string pingdata)
            : base(data)
        {
            PingData = pingdata;
        }

        public string PingData { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class PongEventArgs : IrcEventArgs
    {
        internal PongEventArgs(IrcMessageData data, TimeSpan lag)
            : base(data)
        {
            Lag = lag;
        }

        public TimeSpan Lag { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class KickEventArgs : IrcEventArgs
    {
        internal KickEventArgs(IrcMessageData data, string channel, string who, string whom, string kickreason)
            : base(data)
        {
            Channel = channel;
            Who = who;
            Whom = whom;
            KickReason = kickreason;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }

        public string Whom { get; private set; }

        public string KickReason { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class JoinEventArgs : IrcEventArgs
    {
        internal JoinEventArgs(IrcMessageData data, string channel, string who)
            : base(data)
        {
            Channel = channel;
            Who = who;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class NamesEventArgs : IrcEventArgs
    {
        private readonly string[] userList;

        internal NamesEventArgs(IrcMessageData data, string channel, string[] userlist)
            : base(data)
        {
            Channel = channel;
            userList = userlist;
        }

        public string Channel { get; private set; }

        public string[] UserList
        {
            get { return userList; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class ListEventArgs : IrcEventArgs
    {
        internal ListEventArgs(IrcMessageData data, ChannelInfo listInfo)
            : base(data)
        {
            ListInfo = listInfo;
        }

        public ChannelInfo ListInfo { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class InviteEventArgs : IrcEventArgs
    {
        internal InviteEventArgs(IrcMessageData data, string channel, string who)
            : base(data)
        {
            Channel = channel;
            Who = who;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class PartEventArgs : IrcEventArgs
    {
        internal PartEventArgs(IrcMessageData data, string channel, string who, string partmessage)
            : base(data)
        {
            Channel = channel;
            Who = who;
            PartMessage = partmessage;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }

        public string PartMessage { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class WhoEventArgs : IrcEventArgs
    {
        private readonly WhoInfo whoInfo;

        internal WhoEventArgs(IrcMessageData data, WhoInfo whoInfo)
            : base(data)
        {
            this.whoInfo = whoInfo;
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Channel
        {
            get { return whoInfo.Channel; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Nick
        {
            get { return whoInfo.Nick; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Ident
        {
            get { return whoInfo.Ident; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Host
        {
            get { return whoInfo.Host; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Realname
        {
            get { return whoInfo.Realname; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public bool IsAway
        {
            get { return whoInfo.IsAway; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public bool IsOp
        {
            get { return whoInfo.IsOp; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public bool IsVoice
        {
            get { return whoInfo.IsVoice; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public bool IsIrcOp
        {
            get { return whoInfo.IsIrcOp; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Server
        {
            get { return whoInfo.Server; }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public int HopCount
        {
            get { return whoInfo.HopCount; }
        }

        public WhoInfo WhoInfo
        {
            get { return whoInfo; }
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class QuitEventArgs : IrcEventArgs
    {
        internal QuitEventArgs(IrcMessageData data, string who, string quitmessage)
            : base(data)
        {
            Who = who;
            QuitMessage = quitmessage;
        }

        public string Who { get; private set; }

        public string QuitMessage { get; private set; }
    }


    /// <summary>
    ///
    /// </summary>
    public class AwayEventArgs : IrcEventArgs
    {
        internal AwayEventArgs(IrcMessageData data, string who, string awaymessage)
            : base(data)
        {
            Who = who;
            AwayMessage = awaymessage;
        }

        public string Who { get; private set; }

        public string AwayMessage { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class NickChangeEventArgs : IrcEventArgs
    {
        internal NickChangeEventArgs(IrcMessageData data, string oldnick, string newnick)
            : base(data)
        {
            OldNickname = oldnick;
            NewNickname = newnick;
        }

        public string OldNickname { get; private set; }

        public string NewNickname { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class TopicEventArgs : IrcEventArgs
    {
        internal TopicEventArgs(IrcMessageData data, string channel, string topic)
            : base(data)
        {
            Channel = channel;
            Topic = topic;
        }

        public string Channel { get; private set; }

        public string Topic { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class TopicChangeEventArgs : IrcEventArgs
    {
        internal TopicChangeEventArgs(IrcMessageData data, string channel, string who, string newtopic)
            : base(data)
        {
            Channel = channel;
            Who = who;
            NewTopic = newtopic;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }

        public string NewTopic { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class BanEventArgs : IrcEventArgs
    {
        internal BanEventArgs(IrcMessageData data, string channel, string who, string hostmask)
            : base(data)
        {
            Channel = channel;
            Who = who;
            Hostmask = hostmask;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }

        public string Hostmask { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class UnbanEventArgs : IrcEventArgs
    {
        internal UnbanEventArgs(IrcMessageData data, string channel, string who, string hostmask)
            : base(data)
        {
            Channel = channel;
            Who = who;
            Hostmask = hostmask;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }

        public string Hostmask { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class OpEventArgs : IrcEventArgs
    {
        internal OpEventArgs(IrcMessageData data, string channel, string who, string whom)
            : base(data)
        {
            Channel = channel;
            Who = who;
            Whom = whom;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }

        public string Whom { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class DeopEventArgs : IrcEventArgs
    {
        internal DeopEventArgs(IrcMessageData data, string channel, string who, string whom)
            : base(data)
        {
            Channel = channel;
            Who = who;
            Whom = whom;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }

        public string Whom { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class HalfopEventArgs : IrcEventArgs
    {
        internal HalfopEventArgs(IrcMessageData data, string channel, string who, string whom)
            : base(data)
        {
            Channel = channel;
            Who = who;
            Whom = whom;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }

        public string Whom { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class DehalfopEventArgs : IrcEventArgs
    {
        internal DehalfopEventArgs(IrcMessageData data, string channel, string who, string whom)
            : base(data)
        {
            Channel = channel;
            Who = who;
            Whom = whom;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }

        public string Whom { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class VoiceEventArgs : IrcEventArgs
    {
        internal VoiceEventArgs(IrcMessageData data, string channel, string who, string whom)
            : base(data)
        {
            Channel = channel;
            Who = who;
            Whom = whom;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }

        public string Whom { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class DevoiceEventArgs : IrcEventArgs
    {
        internal DevoiceEventArgs(IrcMessageData data, string channel, string who, string whom)
            : base(data)
        {
            Channel = channel;
            Who = who;
            Whom = whom;
        }

        public string Channel { get; private set; }

        public string Who { get; private set; }

        public string Whom { get; private set; }
    }
}