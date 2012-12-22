/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 *
 * Copyright (c) 2008-2013 Thomas Bruderer <apophis@apophis.ch> <http://www.apophis.ch>
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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using apophis.SharpIRC.IrcClient;

namespace apophis.SharpIRC.IrcFeatures
{
    /// <summary>
    /// Dcc Chat Connection, Line Based Text
    /// </summary>
    public sealed class DccChat : DccConnection
    {
        #region Private Variables

        private StreamReader streamReader;
        private StreamWriter streamWriter;

        #endregion

        #region Public Properties

        public int LineCount { get; private set; }

        #endregion

        /// <summary>
        /// Constructor of DCC CHat for local DCC Chat Request to a certain user.
        /// </summary>
        /// <param name="irc">IrcFeature Class</param>
        /// <param name="user">Chat Destination (channels are no valid targets)</param>
        /// <param name="externalIpAdress">Our externally reachable IP Adress (can be anything if passive)</param>
        /// <param name="passive">if you have no reachable ports!</param>
        /// <param name="priority">Non DCC Message Priority</param>
        internal DccChat(IrcFeatures irc, string user, IPAddress externalIpAdress, bool passive, Priority priority)
        {
            Irc = irc;
            ExternalIPAdress = externalIpAdress;
            User = user;

            if (passive)
            {
                irc.SendMessage(SendType.CtcpRequest, user, "DCC CHAT chat " + HostToDccInt(externalIpAdress) + " 0 " + SessionID, priority);
                Disconnect();
            }
            else
            {
                DccServer = new TcpListener(new IPEndPoint(IPAddress.Any, 0));
                DccServer.Start();
                LocalEndPoint = (IPEndPoint)DccServer.LocalEndpoint;
                irc.SendMessage(SendType.CtcpRequest, user, "DCC CHAT chat " + HostToDccInt(externalIpAdress) + " " + LocalEndPoint.Port, priority);
            }
        }

        /// <summary>
        /// Constructor of a DCC Chat for a Incoming DCC Chat Request
        /// </summary>
        /// <param name="irc">IrcFeature Class</param>
        /// <param name="externalIpAdress">Our externally reachable IP Adress</param>
        /// <param name="e">The Ctcp Event which initiated this constructor</param>
        internal DccChat(IrcFeatures irc, IPAddress externalIpAdress, CtcpEventArgs e)
        {
            Irc = irc;
            ExternalIPAdress = externalIpAdress;
            User = e.Data.Nick;

            if (e.Data.MessageArray.Length > 4)
            {
                long ip;
                bool okIP = long.TryParse(e.Data.MessageArray[3], out ip);
                int port;
                bool okPort = int.TryParse(FilterMarker(e.Data.MessageArray[4]), out port); // port 0 = passive
                if ((e.Data.MessageArray[2] == "chat") && okIP && okPort)
                {
                    RemoteEndPoint = new IPEndPoint(IPAddress.Parse(DccIntToHost(ip)), port);
                    if (e.Data.MessageArray.Length > 5 && e.Data.MessageArray[5] != "T")
                    {
                        AcceptRequest(); // Since we initated the Request, we accept DCC
                        return; // No OnDccChatRequestEvent Event! (we know that we want a connection)
                    }
                    DccChatRequestEvent(new DccEventArgs(this));
                    return;
                }
                irc.SendMessage(SendType.CtcpReply, e.Data.Nick, "ERRMSG DCC Chat Parameter Error");
            }
            else
            {
                irc.SendMessage(SendType.CtcpReply, e.Data.Nick, "ERRMSG DCC Chat not enough parameters");
            }
            IsValid = false;
        }

        internal override void InitWork(Object stateInfo)
        {
            if (!Valid)
                return;
            if (DccServer != null)
            {
                Connection = DccServer.AcceptTcpClient();
                RemoteEndPoint = (IPEndPoint)Connection.Client.RemoteEndPoint;
                DccServer.Stop();
                IsConnected = true;
            }
            else
            {
                while (!IsConnected)
                {
                    Thread.Sleep(500); // We wait till Request is Accepted (or jump out when rejected)
                    if (Reject)
                    {
                        IsValid = false;
                        return;
                    }
                }
            }

            DccChatStartEvent(new DccEventArgs(this));

            streamReader = new StreamReader(Connection.GetStream(), Irc.Encoding);
            streamWriter = new StreamWriter(Connection.GetStream(), Irc.Encoding) { AutoFlush = true };

            string line;
            while (((line = streamReader.ReadLine()) != null) && (IsConnected))
            {
                DccChatReceiveLineEvent(new DccChatEventArgs(this, line));
                LineCount++;
            }
            IsValid = false;
            IsConnected = false;
            DccChatStopEvent(new DccEventArgs(this));
        }

        #region Public Methods for the DCC Chat Object

        /// <summary>
        /// Accept an incoming Chatrequest, returns false if anything but a Connect happens
        /// </summary>
        /// <returns></returns>
        public bool AcceptRequest()
        {
            if (IsConnected)
                return false;
            try
            {
                if (RemoteEndPoint.Port == 0)
                {
                    DccServer = new TcpListener(new IPEndPoint(IPAddress.Any, 0));
                    DccServer.Start();
                    LocalEndPoint = (IPEndPoint)DccServer.LocalEndpoint;
                    Irc.SendMessage(SendType.CtcpRequest, User,
                                    "DCC CHAT chat " + HostToDccInt(ExternalIPAdress) + " " + LocalEndPoint.Port);
                }
                else
                {
                    Connection = new TcpClient();
                    Connection.Connect(RemoteEndPoint);
                    IsConnected = true;
                }
                return true;
            }
            catch (Exception)
            {
                IsValid = false;
                IsConnected = false;
                return false;
            }
        }

        public void WriteLine(string message)
        {
            if (!IsConnected)
            {
                throw new NotConnectedException("DCC Chat is not Connected");
            }
            streamWriter.WriteLine(message);
            LineCount++;
            DccChatSentLineEvent(new DccChatEventArgs(this, message));
        }

        #endregion
    }
}