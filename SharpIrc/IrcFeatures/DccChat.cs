/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SharpIrc.IrcClient.EventArgs;
using SharpIrc.IrcFeatures.EventArgs;

namespace SharpIrc.IrcFeatures
{
    /// <summary>
    /// Dcc Chat Connection, Line Based Text
    /// </summary>
    public sealed class DccChat : DccConnection
    {
        #region Private Variables

        private StreamReader _streamReader;
        private StreamWriter _streamWriter;

        #endregion Private Variables

        #region Public Properties

        public int LineCount { get; private set; }

        #endregion Public Properties

        /// <summary>
        /// Constructor of DCC Chat for local DCC Chat Request to a certain user.
        /// </summary>
        /// <param name="irc">IrcFeature Class</param>
        /// <param name="user">Chat Destination (channels are no valid targets)</param>
        /// <param name="externalIpAddress">Our externally reachable IP address (can be anything if passive)</param>
        /// <param name="passive">if you have no reachable ports!</param>
        /// <param name="priority">Non DCC Message Priority</param>
        internal DccChat(IrcFeatures irc, string user, IPAddress externalIpAddress, bool passive, Priority priority)
        {
            Irc = irc;
            ExternalIpAddress = externalIpAddress;
            User = user;

            if (passive)
            {
                irc.SendMessage(SendType.CtcpRequest, user, "DCC CHAT chat " + HostToDccInt(externalIpAddress) + " 0 " + SessionId, priority);
                Disconnect();
            }
            else
            {
                DccServer = new TcpListener(new IPEndPoint(IPAddress.Any, 0));
                DccServer.Start();
                LocalEndPoint = (IPEndPoint)DccServer.LocalEndpoint;
                irc.SendMessage(SendType.CtcpRequest, user, "DCC CHAT chat " + HostToDccInt(externalIpAddress) + " " + LocalEndPoint.Port, priority);
            }
        }

        /// <summary>
        /// Constructor of a DCC Chat for a Incoming DCC Chat Request
        /// </summary>
        /// <param name="irc">IrcFeature Class</param>
        /// <param name="externalIpAddress">Our externally reachable IP Address</param>
        /// <param name="e">The Ctcp Event which initiated this constructor</param>
        internal DccChat(IrcFeatures irc, IPAddress externalIpAddress, CtcpEventArgs e)
        {
            Irc = irc;
            ExternalIpAddress = externalIpAddress;
            User = e.Data.Nick;

            if (e.Data.MessageArray.Length > 4)
            {
                bool ipOk = long.TryParse(e.Data.MessageArray[3], out var ip);
                bool portOk = int.TryParse(FilterMarker(e.Data.MessageArray[4]), out var port); // port 0 = passive
                if ((e.Data.MessageArray[2] == "chat") && ipOk && portOk)
                {
                    RemoteEndPoint = new IPEndPoint(IPAddress.Parse(DccIntToHost(ip)), port);
                    if (e.Data.MessageArray.Length > 5 && e.Data.MessageArray[5] != "T")
                    {
                        AcceptRequest(); // Since we initiated the Request, we accept DCC
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

            _streamReader = new StreamReader(Connection.GetStream(), Irc.Encoding);
            _streamWriter = new StreamWriter(Connection.GetStream(), Irc.Encoding) { AutoFlush = true };

            string line;
            while (((line = _streamReader.ReadLine()) != null) && (IsConnected))
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
        /// Accept an incoming chat request, returns false if anything but a Connect happens
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
                                    "DCC CHAT chat " + HostToDccInt(ExternalIpAddress) + " " + LocalEndPoint.Port);
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
            _streamWriter.WriteLine(message);
            LineCount++;
            DccChatSentLineEvent(new DccChatEventArgs(this, message));
        }

        #endregion Public Methods for the DCC Chat Object
    }
}