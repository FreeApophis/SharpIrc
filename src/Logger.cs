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

using System.Collections;

namespace Meebey.SmartIrc4net
{
#if LOG4NET
    /// <summary>
    ///
    /// </summary>
    public enum LogCategory
    {
        Main,
        Connection,
        Socket,
        Queue,
        IrcMessages,
        MessageTypes,
        MessageParser,
        ActionHandler,
        TimeHandler,
        MessageHandler,
        ChannelSyncing,
        UserSyncing,
        Modules,
        Dcc
    }

    /// <summary>
    ///
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    internal class Logger
    {
        private static readonly SortedList LoggerList = new SortedList();
        private static bool init;

        private Logger()
        {
        }

        public static void Init()
        {
            if (init)
            {
                return;
            }

            init = true;

            /*
            FileInfo fi = new FileInfo("SmartIrc4net_log.config");
            if (fi.Exists) {
                    log4net.Config.DOMConfigurator.ConfigureAndWatch(fi);
            } else {
                log4net.Config.BasicConfigurator.Configure();
            }
            */

            LoggerList[LogCategory.Main] = log4net.LogManager.GetLogger("MAIN");
            LoggerList[LogCategory.Socket] = log4net.LogManager.GetLogger("SOCKET");
            LoggerList[LogCategory.Queue] = log4net.LogManager.GetLogger("QUEUE");
            LoggerList[LogCategory.Connection] = log4net.LogManager.GetLogger("CONNECTION");
            LoggerList[LogCategory.IrcMessages] = log4net.LogManager.GetLogger("IRCMESSAGE");
            LoggerList[LogCategory.MessageParser] = log4net.LogManager.GetLogger("MESSAGEPARSER");
            LoggerList[LogCategory.MessageTypes] = log4net.LogManager.GetLogger("MESSAGETYPES");
            LoggerList[LogCategory.ActionHandler] = log4net.LogManager.GetLogger("ACTIONHANDLER");
            LoggerList[LogCategory.TimeHandler] = log4net.LogManager.GetLogger("TIMEHANDLER");
            LoggerList[LogCategory.MessageHandler] = log4net.LogManager.GetLogger("MESSAGEHANDLER");
            LoggerList[LogCategory.ChannelSyncing] = log4net.LogManager.GetLogger("CHANNELSYNCING");
            LoggerList[LogCategory.UserSyncing] = log4net.LogManager.GetLogger("USERSYNCING");
            LoggerList[LogCategory.Modules] = log4net.LogManager.GetLogger("MODULES");
            LoggerList[LogCategory.Dcc] = log4net.LogManager.GetLogger("DCC");
        }

        public static log4net.ILog Main
        {
            get
            {
                return (log4net.ILog)LoggerList[LogCategory.Main];
            }
        }

        public static log4net.ILog Socket
        {
            get
            {
                return (log4net.ILog)LoggerList[LogCategory.Socket];
            }
        }

        public static log4net.ILog Queue
        {
            get
            {
                return (log4net.ILog)LoggerList[LogCategory.Queue];
            }
        }

        public static log4net.ILog Connection
        {
            get
            {
                return (log4net.ILog)LoggerList[LogCategory.Connection];
            }
        }

        public static log4net.ILog IrcMessages
        {
            get
            {
                return (log4net.ILog)LoggerList[LogCategory.IrcMessages];
            }
        }

        public static log4net.ILog MessageParser
        {
            get
            {
                return (log4net.ILog)LoggerList[LogCategory.MessageParser];
            }
        }

        public static log4net.ILog MessageTypes
        {
            get
            {
                return (log4net.ILog)LoggerList[LogCategory.MessageTypes];
            }
        }

        public static log4net.ILog ActionHandler
        {
            get
            {
                return (log4net.ILog)LoggerList[LogCategory.ActionHandler];
            }
        }

        public static log4net.ILog TimeHandler
        {
            get
            {
                return (log4net.ILog)LoggerList[LogCategory.TimeHandler];
            }
        }

        public static log4net.ILog MessageHandler
        {
            get
            {
                return (log4net.ILog)LoggerList[LogCategory.MessageHandler];
            }
        }

        public static log4net.ILog ChannelSyncing
        {
            get
            {
                return (log4net.ILog)LoggerList[LogCategory.ChannelSyncing];
            }
        }

        public static log4net.ILog UserSyncing
        {
            get
            {
                return (log4net.ILog)LoggerList[LogCategory.UserSyncing];
            }
        }

        public static log4net.ILog Modules
        {
            get
            {
                return (log4net.ILog)LoggerList[LogCategory.Modules];
            }
        }

        public static log4net.ILog Dcc
        {
            get
            {
                return (log4net.ILog)LoggerList[LogCategory.Dcc];
            }
        }
    }
#endif
}