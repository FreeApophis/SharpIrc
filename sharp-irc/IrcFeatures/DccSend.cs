/*
 *
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
    /// Dcc Send Connection, Filetransfer
    /// </summary>
    public sealed class DccSend : DccConnection
    {
        #region Private Variables

        private readonly byte[] buffer = new byte[8192];
        private readonly bool directionUp;
        private readonly string filename;
        private readonly long filesize;
        private readonly DccSpeed speed;
        private Stream file;
        private long sentBytes;

        #endregion

        #region Public Properties

        public long SentBytes
        {
            get { return sentBytes; }
        }

        #endregion

        internal DccSend(IrcFeatures irc, string user, IPAddress externalIpAdress, Stream file, string filename, long filesize, DccSpeed speed, bool passive, Priority priority)
        {
            Irc = irc;
            directionUp = true;
            this.file = file;
            this.filesize = filesize;
            this.filename = filename;
            this.speed = speed;
            User = user;

            if (passive)
            {
                irc.SendMessage(SendType.CtcpRequest, user, "DCC SEND \"" + filename + "\" " + HostToDccInt(externalIpAdress) + " 0 " + filesize + " " + SessionID, priority);
            }
            else
            {
                DccServer = new TcpListener(new IPEndPoint(IPAddress.Any, 0));
                DccServer.Start();
                LocalEndPoint = (IPEndPoint)DccServer.LocalEndpoint;
                irc.SendMessage(SendType.CtcpRequest, user, "DCC SEND \"" + filename + "\" " + HostToDccInt(externalIpAdress) + " " + LocalEndPoint.Port + " " + filesize, priority);
            }
        }

        internal DccSend(IrcFeatures irc, IPAddress externalIpAdress, CtcpEventArgs e)
        {
            /* Remote Request */
            Irc = irc;
            directionUp = false;
            User = e.Data.Nick;

            if (e.Data.MessageArray.Length > 4)
            {
                long ip, filesize = 0;
                int port = 0;
                bool okIP = long.TryParse(e.Data.MessageArray[3], out ip);
                bool okPo = int.TryParse(e.Data.MessageArray[4], out port); // port 0 = passive
                if (e.Data.MessageArray.Length > 5)
                {
                    bool okFs = long.TryParse(FilterMarker(e.Data.MessageArray[5]), out filesize);
                    this.filesize = filesize;
                    filename = e.Data.MessageArray[2].Trim(new[] { '\"' });
                }
                if (okIP && okPo)
                {
                    RemoteEndPoint = new IPEndPoint(IPAddress.Parse(DccIntToHost(ip)), port);
                    DccSendRequestEvent(new DccSendRequestEventArgs(this, e.Data.MessageArray[2], filesize));
                    return;
                }
                irc.SendMessage(SendType.CtcpReply, e.Data.Nick, "ERRMSG DCC Send Parameter Error");
            }
            else
            {
                irc.SendMessage(SendType.CtcpReply, e.Data.Nick, "ERRMSG DCC Send not enough parameters");
            }
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
                    if (Reject) return;
                }
            }

            DccSendStartEvent(new DccEventArgs(this));
            int bytes;

            if (directionUp)
            {
                do
                {
                    while (Connection.Available > 0)
                    {
                        switch (speed)
                        {
                            case DccSpeed.Rfc:
                                Connection.GetStream().Read(buffer, 0, buffer.Length);
                                // TODO: only send x not ACKed Bytes ahead / (nobody wants this anyway)
                                break;
                            case DccSpeed.RfcSendAhead:
                                Connection.GetStream().Read(buffer, 0, buffer.Length);
                                break;
                            case DccSpeed.Turbo: // Available > 0 should not happen
                                break;
                        }
                    }

                    bytes = file.Read(buffer, 0, buffer.Length);
                    try
                    {
                        Connection.GetStream().Write(buffer, 0, bytes);
                    }
                    catch (IOException)
                    {
                        bytes = 0; // Connection Lost
                    }

                    sentBytes += bytes;

                    if (bytes > 0)
                    {
                        DccSendSentBlockEvent(new DccSendEventArgs(this, buffer, bytes));
                        Console.Write(".");
                    }
                } while (bytes > 0);
            }
            else
            {
                while ((bytes = Connection.GetStream().Read(buffer, 0, buffer.Length)) > 0)
                {
                    file.Write(buffer, 0, bytes);
                    sentBytes += bytes;
                    if (speed != DccSpeed.Turbo)
                    {
                        Connection.GetStream().Write(GetAck(sentBytes), 0, 4);
                    }

                    DccSendReceiveBlockEvent(new DccSendEventArgs(this, buffer, bytes));
                }
            }


            IsValid = false;
            IsConnected = false;
            Console.WriteLine("--> Filetrangsfer Endet / Bytes sent: " + sentBytes + " of " + filesize);
            DccSendStopEvent(new DccEventArgs(this));
        }

        #region Public Methods for the DCC Send Object

        /// <summary>
        /// With this methode you can accept a DCC SEND Request you got from another User
        /// </summary>
        /// <param name="file">Any Stream you want use as a file, if you use offset it should be Seekable</param>
        /// <param name="offset">Offset to start a Resume Request for the rest of a file</param>
        /// <returns></returns>
        public bool AcceptRequest(Stream file, long offset)
        {
            if (IsConnected)
                return false;
            try
            {
                if (file != null)
                    this.file = file;
                if (RemoteEndPoint.Port == 0)
                {
                    DccServer = new TcpListener(new IPEndPoint(IPAddress.Any, 0));
                    DccServer.Start();
                    LocalEndPoint = (IPEndPoint)DccServer.LocalEndpoint;
                    Irc.SendMessage(SendType.CtcpRequest, User, "DCC SEND \"" + filename + "\" " + HostToDccInt(ExternalIPAdress) + " " + LocalEndPoint.Port + " " + filesize);
                }
                else
                {
                    if (offset == 0)
                    {
                        Connection = new TcpClient();
                        Connection.Connect(RemoteEndPoint);
                        IsConnected = true;
                    }
                    else
                    {
                        if (this.file.CanSeek)
                        {
                            this.file.Seek(offset, SeekOrigin.Begin);
                            sentBytes = offset;
                            Irc.SendMessage(SendType.CtcpRequest, User, "DCC RESUME \"" + filename + "\" " + RemoteEndPoint.Port + " " + offset);
                        }
                        else
                        {
                            /* Resume of a file which is not seekable : I dont care, its your filestream! */
                            sentBytes = offset;
                            Irc.SendMessage(SendType.CtcpRequest, User, "DCC RESUME \"" + filename + "\" " + RemoteEndPoint.Port + " " + offset);
                        }
                    }
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

        #endregion

        #region Handler for Passive / Resume DCC

        internal bool TryResume(CtcpEventArgs e)
        {
            if (User == e.Data.Nick)
            {
                if ((e.Data.MessageArray.Length > 4) && (filename == e.Data.MessageArray[2].Trim(new[] { '\"' })))
                {
                    long offset;
                    long.TryParse(FilterMarker(e.Data.MessageArray[4]), out offset);
                    if (file.CanSeek)
                    {
                        if (e.Data.MessageArray.Length > 5)
                        {
                            Irc.SendMessage(SendType.CtcpRequest, e.Data.Nick, "DCC ACCEPT " + e.Data.MessageArray[2] + " " + e.Data.MessageArray[3] + " " + e.Data.MessageArray[4] + " " + FilterMarker(e.Data.MessageArray[5]));
                        }
                        else
                        {
                            Irc.SendMessage(SendType.CtcpRequest, e.Data.Nick, "DCC ACCEPT " + e.Data.MessageArray[2] + " " + e.Data.MessageArray[3] + " " + FilterMarker(e.Data.MessageArray[4]));
                        }

                        file.Seek(offset, SeekOrigin.Begin);
                        sentBytes = offset;
                        return true;
                    }
                    Irc.SendMessage(SendType.CtcpRequest, e.Data.Nick, "ERRMSG DCC File not seekable");
                }
            }
            return false;
        }

        internal bool TryAccept(CtcpEventArgs e)
        {
            if (User == e.Data.Nick)
            {
                if ((e.Data.MessageArray.Length > 4) && (filename == e.Data.MessageArray[2].Trim(new[] { '\"' })))
                {
                    return AcceptRequest(null, 0);
                }
            }
            return false;
        }

        internal bool SetRemote(CtcpEventArgs e)
        {
            long ip;
            int port;
            bool okIP = long.TryParse(e.Data.MessageArray[3], out ip);
            bool okPo = int.TryParse(e.Data.MessageArray[4], out port); // port 0 = passive
            if (okIP && okPo)
            {
                RemoteEndPoint = new IPEndPoint(IPAddress.Parse(DccIntToHost(ip)), port);
                return true;
            }
            return false;
        }

        #endregion
    }
}