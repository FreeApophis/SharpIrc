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
    /// Dcc Send Connection, file transfer
    /// </summary>
    public sealed class DccSend : DccConnection
    {
        #region Private Variables

        private readonly byte[] _buffer = new byte[8192];
        private readonly bool _directionUp;
        private readonly string _filename;
        private readonly long _fileSize;
        private readonly DccSpeed _speed;
        private Stream _file;

        #endregion Private Variables

        #region Public Properties

        public long SentBytes { get; private set; }

        #endregion Public Properties

        internal DccSend(IrcFeatures irc, string user, IPAddress externalIpAddress, Stream file, string filename, long fileSize, DccSpeed speed, bool passive, Priority priority)
        {
            Irc = irc;
            _directionUp = true;
            _file = file;
            _fileSize = fileSize;
            _filename = filename;
            _speed = speed;
            User = user;

            if (passive)
            {
                irc.SendMessage(SendType.CtcpRequest, user, "DCC SEND \"" + filename + "\" " + HostToDccInt(externalIpAddress) + " 0 " + fileSize + " " + SessionId, priority);
            }
            else
            {
                DccServer = new TcpListener(new IPEndPoint(IPAddress.Any, 0));
                DccServer.Start();
                LocalEndPoint = (IPEndPoint)DccServer.LocalEndpoint;
                irc.SendMessage(SendType.CtcpRequest, user, "DCC SEND \"" + filename + "\" " + HostToDccInt(externalIpAddress) + " " + LocalEndPoint.Port + " " + fileSize, priority);
            }
        }

        internal DccSend(IrcFeatures irc, IPAddress externalIpAddress, CtcpEventArgs e)
        {
            /* Remote Request */
            Irc = irc;
            _directionUp = false;
            User = e.Data.Nick;

            if (e.Data.MessageArray.Length > 4)
            {
                long fileSize = 0;
                bool ipOk = long.TryParse(e.Data.MessageArray[3], out var ip);
                bool portOk = int.TryParse(e.Data.MessageArray[4], out var port); // port 0 = passive
                if (e.Data.MessageArray.Length > 5)
                {
                    bool fileSizeOk = long.TryParse(FilterMarker(e.Data.MessageArray[5]), out fileSize);
                    _fileSize = fileSize;
                    _filename = e.Data.MessageArray[2].Trim('\"');
                }
                if (ipOk && portOk)
                {
                    RemoteEndPoint = new IPEndPoint(IPAddress.Parse(DccIntToHost(ip)), port);
                    DccSendRequestEvent(new DccSendRequestEventArgs(this, e.Data.MessageArray[2], fileSize));
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

            if (_directionUp)
            {
                do
                {
                    while (Connection.Available > 0)
                    {
                        switch (_speed)
                        {
                            case DccSpeed.Rfc:
                                Connection.GetStream().Read(_buffer, 0, _buffer.Length);
                                // TODO: only send x not ACKed Bytes ahead / (nobody wants this anyway)
                                break;
                            case DccSpeed.RfcSendAhead:
                                Connection.GetStream().Read(_buffer, 0, _buffer.Length);
                                break;
                            case DccSpeed.Turbo: // Available > 0 should not happen
                                break;
                        }
                    }

                    bytes = _file.Read(_buffer, 0, _buffer.Length);
                    try
                    {
                        Connection.GetStream().Write(_buffer, 0, bytes);
                    }
                    catch (IOException)
                    {
                        bytes = 0; // Connection Lost
                    }

                    SentBytes += bytes;

                    if (bytes > 0)
                    {
                        DccSendSentBlockEvent(new DccSendEventArgs(this, _buffer, bytes));
                        Console.Write(".");
                    }
                } while (bytes > 0);
            }
            else
            {
                while ((bytes = Connection.GetStream().Read(_buffer, 0, _buffer.Length)) > 0)
                {
                    _file.Write(_buffer, 0, bytes);
                    SentBytes += bytes;
                    if (_speed != DccSpeed.Turbo)
                    {
                        Connection.GetStream().Write(GetAck(SentBytes), 0, 4);
                    }

                    DccSendReceiveBlockEvent(new DccSendEventArgs(this, _buffer, bytes));
                }
            }


            IsValid = false;
            IsConnected = false;
            Console.WriteLine("--> Filetrangsfer Endet / Bytes sent: " + SentBytes + " of " + _fileSize);
            DccSendStopEvent(new DccEventArgs(this));
        }

        #region Public Methods for the DCC Send Object

        /// <summary>
        /// With this method you can accept a DCC SEND Request you got from another User
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
                    _file = file;
                if (RemoteEndPoint.Port == 0)
                {
                    DccServer = new TcpListener(new IPEndPoint(IPAddress.Any, 0));
                    DccServer.Start();
                    LocalEndPoint = (IPEndPoint)DccServer.LocalEndpoint;
                    Irc.SendMessage(SendType.CtcpRequest, User, "DCC SEND \"" + _filename + "\" " + HostToDccInt(ExternalIpAddress) + " " + LocalEndPoint.Port + " " + _fileSize);
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
                        if (_file.CanSeek)
                        {
                            _file.Seek(offset, SeekOrigin.Begin);
                            SentBytes = offset;
                            Irc.SendMessage(SendType.CtcpRequest, User, "DCC RESUME \"" + _filename + "\" " + RemoteEndPoint.Port + " " + offset);
                        }
                        else
                        {
                            /* Resume of a file which is not seekable : I don't care, its your file stream! */
                            SentBytes = offset;
                            Irc.SendMessage(SendType.CtcpRequest, User, "DCC RESUME \"" + _filename + "\" " + RemoteEndPoint.Port + " " + offset);
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

        #endregion Public Methods for the DCC Send Object

        #region Handler for Passive / Resume DCC

        internal bool TryResume(CtcpEventArgs e)
        {
            if (User == e.Data.Nick)
            {
                if ((e.Data.MessageArray.Length > 4) && (_filename == e.Data.MessageArray[2].Trim('\"')))
                {
                    long.TryParse(FilterMarker(e.Data.MessageArray[4]), out var offset);
                    if (_file.CanSeek)
                    {
                        if (e.Data.MessageArray.Length > 5)
                        {
                            Irc.SendMessage(SendType.CtcpRequest, e.Data.Nick, "DCC ACCEPT " + e.Data.MessageArray[2] + " " + e.Data.MessageArray[3] + " " + e.Data.MessageArray[4] + " " + FilterMarker(e.Data.MessageArray[5]));
                        }
                        else
                        {
                            Irc.SendMessage(SendType.CtcpRequest, e.Data.Nick, "DCC ACCEPT " + e.Data.MessageArray[2] + " " + e.Data.MessageArray[3] + " " + FilterMarker(e.Data.MessageArray[4]));
                        }

                        _file.Seek(offset, SeekOrigin.Begin);
                        SentBytes = offset;
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
                if ((e.Data.MessageArray.Length > 4) && (_filename == e.Data.MessageArray[2].Trim('\"')))
                {
                    return AcceptRequest(null, 0);
                }
            }
            return false;
        }

        internal bool SetRemote(CtcpEventArgs e)
        {
            bool ipOk = long.TryParse(e.Data.MessageArray[3], out var ip);
            bool portOk = int.TryParse(e.Data.MessageArray[4], out var port); // port 0 = passive
            if (ipOk && portOk)
            {
                RemoteEndPoint = new IPEndPoint(IPAddress.Parse(DccIntToHost(ip)), port);
                return true;
            }
            return false;
        }

        #endregion Handler for Passive / Resume DCC
    }
}