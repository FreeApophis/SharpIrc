/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using SharpIrc.IrcClient.EventArgs;
using SharpIrc.IrcFeatures.EventArgs;

namespace SharpIrc.IrcFeatures
{
    /// <summary>
    /// Description of IrcFeatures2.
    /// </summary>
    ///
    public class IrcFeatures : IrcClient.IrcClient
    {
        #region Public Field Access

        public IPAddress ExternalIpAddress { get; set; }

        /// <summary>
        /// Access to all DccConnections, Its not possible to change the collection itself,
        /// but you can use the public Members of the DccCollections or its inherited Classes.
        /// </summary>
        public ReadOnlyCollection<DccConnection> DccConnections => new ReadOnlyCollection<DccConnection>(_dccConnections);

        /// <summary>
        /// To handle more or less CTCP Events, modify this collection to your needs.
        /// You can also change the Delegates to your own implementations.
        /// </summary>
        public Dictionary<string, CtcpDelegate> CtcpDelegates { get; } = new Dictionary<string, CtcpDelegate>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// This Info is shown with the CTCP UserInfo Request
        /// </summary>
        public string CtcpUserInfo { get; set; }

        /// <summary>
        /// This Url will be mentioned with the CTCP Url Request
        /// </summary>
        public string CtcpUrl { get; set; }

        /// <summary>
        /// The Source of the IRC Program is show in the CTCP Source Request
        /// </summary>
        public string CtcpSource { get; set; }

        #endregion Public Field Access

        #region private variables

        private readonly List<DccConnection> _dccConnections = new List<DccConnection>();

        internal DccSpeed Speed = DccSpeed.RfcSendAhead;

        #endregion private variables

        #region Public DCC Events (Global: All Dcc Events)

        public event EventHandler<DccEventArgs> OnDccChatRequestEvent;

        public void DccChatRequestEvent(DccEventArgs e)
        {
            OnDccChatRequestEvent?.Invoke(this, e);
        }

        public event EventHandler<DccSendRequestEventArgs> OnDccSendRequestEvent;

        public void DccSendRequestEvent(DccSendRequestEventArgs e)
        {
            OnDccSendRequestEvent?.Invoke(this, e);
        }

        public event EventHandler<DccEventArgs> OnDccChatStartEvent;

        public void DccChatStartEvent(DccEventArgs e)
        {
            OnDccChatStartEvent?.Invoke(this, e);
        }

        public event EventHandler<DccEventArgs> OnDccSendStartEvent;

        public void DccSendStartEvent(DccEventArgs e)
        {
            OnDccSendStartEvent?.Invoke(this, e);
        }

        public event EventHandler<DccChatEventArgs> OnDccChatReceiveLineEvent;

        public void DccChatReceiveLineEvent(DccChatEventArgs e)
        {
            OnDccChatReceiveLineEvent?.Invoke(this, e);
        }

        public event EventHandler<DccSendEventArgs> OnDccSendReceiveBlockEvent;

        public void DccSendReceiveBlockEvent(DccSendEventArgs e)
        {
            OnDccSendReceiveBlockEvent?.Invoke(this, e);
        }

        public event EventHandler<DccChatEventArgs> OnDccChatSentLineEvent;

        public void DccChatSentLineEvent(DccChatEventArgs e)
        {
            OnDccChatSentLineEvent?.Invoke(this, e);
        }

        public event EventHandler<DccSendEventArgs> OnDccSendSentBlockEvent;

        internal void DccSendSentBlockEvent(DccSendEventArgs e)
        {
            OnDccSendSentBlockEvent?.Invoke(this, e);
        }

        public event EventHandler<DccEventArgs> OnDccChatStopEvent;

        public void DccChatStopEvent(DccEventArgs e)
        {
            OnDccChatStopEvent?.Invoke(this, e);
        }

        public event EventHandler<DccEventArgs> OnDccSendStopEvent;

        public void DccSendStopEvent(DccEventArgs e)
        {
            OnDccSendStopEvent?.Invoke(this, e);
        }

        #endregion Public DCC Events (Global: All Dcc Events)

        #region Public Interface Methods

        public IrcFeatures()
        {
            // This method calls all the ctcp handlers defined below (or added anywhere else)
            OnCtcpRequest += CtcpRequestsHandler;

            // Adding ctcp handler, all commands are lower case (.ToLower() in handler)
            CtcpDelegates.Add("version", CtcpVersionDelegate);
            CtcpDelegates.Add("clientinfo", CtcpClientInfoDelegate);
            CtcpDelegates.Add("time", CtcpTimeDelegate);
            CtcpDelegates.Add("userinfo", CtcpUserInfoDelegate);
            CtcpDelegates.Add("url", CtcpUrlDelegate);
            CtcpDelegates.Add("source", CtcpSourceDelegate);
            CtcpDelegates.Add("finger", CtcpFingerDelegate);

            // The DCC Handler
            CtcpDelegates.Add("dcc", CtcpDccDelegate);

            // Don't remove the Ping handler without your own implementation
            CtcpDelegates.Add("ping", CtcpPingDelegate);
        }

        /// <summary>
        /// Init a DCC Chat Session
        /// </summary>
        /// <param name="user">User to DCC</param>
        public void InitDccChat(string user)
        {
            InitDccChat(user, false);
        }

        /// <summary>
        /// Init a DCC Chat Session
        /// </summary>
        /// <param name="user">User to DCC</param>
        /// <param name="passive">Passive DCC</param>
        public void InitDccChat(string user, bool passive)
        {
            InitDccChat(user, passive, Priority.Medium);
        }

        /// <summary>
        /// Init a DCC Chat Session
        /// </summary>
        /// <param name="user">User to DCC</param>
        /// <param name="passive">Passive DCC</param>
        /// <param name="priority">Non Dcc Message Priority for Negotiation</param>
        public void InitDccChat(string user, bool passive, Priority priority)
        {
            var chat = new DccChat(this, user, ExternalIpAddress, passive, priority);
            _dccConnections.Add(chat);
            ThreadPool.QueueUserWorkItem(chat.InitWork);
            RemoveInvalidDccConnections();
        }


        /// <summary>
        /// Send a local File
        /// </summary>
        /// <param name="user">Destination of the File (no channel)</param>
        /// <param name="filepath">complete filepath, absolute or relative (careful)</param>
        public void SendFile(string user, string filepath)
        {
            var fi = new FileInfo(filepath);
            if (fi.Exists)
            {
                SendFile(user, new FileStream(filepath, FileMode.Open), fi.Name, fi.Length, DccSpeed.RfcSendAhead, false, Priority.Medium);
            }
        }

        /// <summary>
        /// Send a local File passively
        /// </summary>
        /// <param name="user">Destination of the File (no channel)</param>
        /// <param name="filepath">complete filepath, absolute or relative (careful)</param>
        /// <param name="passive">Passive DCC</param>
        public void SendFile(string user, string filepath, bool passive)
        {
            var fi = new FileInfo(filepath);
            if (fi.Exists)
            {
                SendFile(user, new FileStream(filepath, FileMode.Open), fi.Name, fi.Length, DccSpeed.RfcSendAhead, passive, Priority.Medium);
            }
        }

        /// <summary>
        /// Send any Stream, active initiator, fast RfC method
        /// </summary>
        /// <param name="user">Destination of the File (no channel)</param>
        /// <param name="file">You can send any stream here</param>
        /// <param name="filename">give a filename for the remote User</param>
        /// <param name="fileSize">give the length of the stream</param>
        public void SendFile(string user, Stream file, string filename, long fileSize)
        {
            SendFile(user, file, filename, fileSize, DccSpeed.RfcSendAhead, false);
        }

        /// <summary>
        /// Send any Stream, full flexibility in Dcc Connection Negotiation
        /// </summary>
        /// <param name="user">Destination of the File (no channel)</param>
        /// <param name="file">You can send any stream here</param>
        /// <param name="filename">give a filename for the remote User</param>
        /// <param name="fileSize">give the length of the stream</param>
        /// <param name="speed">What ACK management should be used</param>
        /// <param name="passive">Passive DCC</param>
        public void SendFile(string user, Stream file, string filename, long fileSize, DccSpeed speed, bool passive)
        {
            SendFile(user, file, filename, fileSize, speed, passive, Priority.Medium);
        }

        /// <summary>
        /// Send any Stream, full flexibility in Dcc Connection Negotiation
        /// </summary>
        /// <param name="user">Destination of the File (no channel)</param>
        /// <param name="file">You can send any stream here</param>
        /// <param name="filename">give a filename for the remote User</param>
        /// <param name="fileSize">give the length of the stream</param>
        /// <param name="speed">What ACK management should be used</param>
        /// <param name="passive">Passive DCC</param>
        /// <param name="priority">Non Dcc Message Priority for Negotiation</param>
        public void SendFile(string user, Stream file, string filename, long fileSize, DccSpeed speed, bool passive,
                             Priority priority)
        {
            var send = new DccSend(this, user, ExternalIpAddress, file, filename, fileSize, speed, passive, priority);
            _dccConnections.Add(send);
            ThreadPool.QueueUserWorkItem(send.InitWork);
            RemoveInvalidDccConnections();
        }

        #endregion Public Interface Methods

        #region Private Methods

        private void CtcpRequestsHandler(object sender, CtcpEventArgs e)
        {
            if (CtcpDelegates.ContainsKey(e.CtcpCommand))
            {
                CtcpDelegates[e.CtcpCommand].Invoke(e);
            }
            RemoveInvalidDccConnections();
        }

        #endregion Private Methods

        #region implemented ctcp delegates, can be overwritten by changing the ctcpDelagtes Dictionary

        private void CtcpVersionDelegate(CtcpEventArgs e)
        {
            SendMessage(SendType.CtcpReply, e.Data.Nick, "VERSION " + (CtcpVersion ?? VersionString));
        }

        private void CtcpClientInfoDelegate(CtcpEventArgs e)
        {
            string clientInfo = CtcpDelegates.Aggregate("CLIENTINFO", (current, kvp) => current + " " + kvp.Key.ToUpper());
            SendMessage(SendType.CtcpReply, e.Data.Nick, clientInfo);
        }

        private void CtcpPingDelegate(CtcpEventArgs e)
        {
            if (e.Data.Message.Length > 7)
            {
                SendMessage(SendType.CtcpReply, e.Data.Nick, "PING " + e.Data.Message.Substring(6, (e.Data.Message.Length - 7)));
            }
            else
            {
                SendMessage(SendType.CtcpReply, e.Data.Nick, "PING"); //according to RFC, it should be PONG!
            }
        }

        /// <summary>
        ///  This is the correct Rfc Ping Delegate, which is not used because all other clients do not use the PING According to RfC
        /// </summary>
        /// <param name="e"></param>
        private void CtcpRfcPingDelegate(CtcpEventArgs e)
        {
            if (e.Data.Message.Length > 7)
            {
                SendMessage(SendType.CtcpReply, e.Data.Nick, "PONG " + e.Data.Message.Substring(6, (e.Data.Message.Length - 7)));
            }
            else
            {
                SendMessage(SendType.CtcpReply, e.Data.Nick, "PONG");
            }
        }


        private void CtcpTimeDelegate(CtcpEventArgs e)
        {
            SendMessage(SendType.CtcpReply, e.Data.Nick, "TIME " + DateTime.Now.ToString("r"));
        }

        private void CtcpUserInfoDelegate(CtcpEventArgs e)
        {
            SendMessage(SendType.CtcpReply, e.Data.Nick, "USERINFO " + (CtcpUserInfo ?? "No user info given."));
        }

        private void CtcpUrlDelegate(CtcpEventArgs e)
        {
            SendMessage(SendType.CtcpReply, e.Data.Nick, "URL " + (CtcpUrl ?? "http://www.google.com"));
        }

        private void CtcpSourceDelegate(CtcpEventArgs e)
        {
            SendMessage(SendType.CtcpReply, e.Data.Nick, "SOURCE " + (CtcpSource ?? "https://github.com/FreeApophis/sharpIRC"));
        }

        private void CtcpFingerDelegate(CtcpEventArgs e)
        {
            SendMessage(SendType.CtcpReply, e.Data.Nick, "FINGER Don't touch little Helga there! ");
            //SendMessage(SendType.CtcpReply, e.Data.Nick, "FINGER " + this.RealName + " (" + this.Email + ") Idle " + this.Idle + " seconds (" + ((string.IsNullOrEmpty(this.Reason)) ? this.Reason : "-") + ") ");
        }

        private void CtcpDccDelegate(CtcpEventArgs e)
        {
            if (e.Data.MessageArray.Length < 2)
            {
                SendMessage(SendType.CtcpReply, e.Data.Nick, "ERRMSG DCC missing parameters");
            }
            else
            {
                switch (e.Data.MessageArray[1])
                {
                    case "CHAT":
                        var chat = new DccChat(this, ExternalIpAddress, e);
                        _dccConnections.Add(chat);
                        ThreadPool.QueueUserWorkItem(chat.InitWork);
                        break;
                    case "SEND":
                        if (e.Data.MessageArray.Length > 6 && (FilterMarker(e.Data.MessageArray[6]) != "T"))
                        {
                            long.TryParse(FilterMarker(e.Data.MessageArray[6]), out var session);
                            foreach (DccConnection dc in _dccConnections)
                            {
                                if (dc.IsSession(session))
                                {
                                    ((DccSend)dc).SetRemote(e);
                                    ((DccSend)dc).AcceptRequest(null, 0);
                                    return;
                                }
                            }
                            SendMessage(SendType.CtcpReply, e.Data.Nick, "ERRMSG Invalid passive DCC");
                        }
                        else
                        {
                            var send = new DccSend(this, ExternalIpAddress, e);
                            _dccConnections.Add(send);
                            ThreadPool.QueueUserWorkItem(send.InitWork);
                        }
                        break;
                    case "RESUME":
                        if (_dccConnections.Any(dc => (dc is DccSend send) && (send.TryResume(e))))
                        {
                            return;
                        }
                        SendMessage(SendType.CtcpReply, e.Data.Nick, "ERRMSG Invalid DCC RESUME");
                        break;
                    case "ACCEPT":
                        if (_dccConnections.Any(dc => (dc is DccSend send) && (send.TryAccept(e))))
                        {
                            return;
                        }
                        SendMessage(SendType.CtcpReply, e.Data.Nick, "ERRMSG Invalid DCC ACCEPT");
                        break;
                    case "XMIT":
                        SendMessage(SendType.CtcpReply, e.Data.Nick, "ERRMSG DCC XMIT not implemented");
                        break;
                    default:
                        SendMessage(SendType.CtcpReply, e.Data.Nick, "ERRMSG DCC " + e.CtcpParameter + " unavailable");
                        break;
                }
            }
        }

        /// <summary>
        /// cleanup all old invalid DCCs (late cleaning)
        /// </summary>
        private void RemoveInvalidDccConnections()
        {
            foreach (DccConnection dc in _dccConnections.Where(dc => (!dc.Valid) && (!dc.Connected)).ToList())
            {
                _dccConnections.Remove(dc);
            }
        }

        private static string FilterMarker(IEnumerable<char> msg)
        {
            return msg.Where(c => c != IrcConstants.CtcpChar).Aggregate("", (current, c) => current + c);
        }

        #endregion implemented ctcp delegates, can be overwritten by changing the ctcpDelagtes Dictionary
    }
}