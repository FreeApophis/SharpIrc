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

        public IPAddress ExternalIpAdress { get; set; }

        /// <summary>
        /// Access to all DccConnections, Its not possible to change the collection itself,
        /// but you can use the public Members of the DccCollections or its inherited Classes.
        /// </summary>
        public ReadOnlyCollection<DccConnection> DccConnections
        {
            get { return new ReadOnlyCollection<DccConnection>(dccConnections); }
        }

        /// <summary>
        /// To handle more or less CTCP Events, modify this collection to your needs.
        /// You can also change the Delegates to your own implementations.
        /// </summary>
        public Dictionary<string, CtcpDelegate> CtcpDelegates
        {
            get { return ctcpDelegates; }
        }

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

        private readonly Dictionary<string, CtcpDelegate> ctcpDelegates = new Dictionary<string, CtcpDelegate>(StringComparer.CurrentCultureIgnoreCase);

        private readonly List<DccConnection> dccConnections = new List<DccConnection>();

        internal DccSpeed Speed = DccSpeed.RfcSendAhead;

        #endregion private variables

        #region Public DCC Events (Global: All Dcc Events)

        public event EventHandler<DccEventArgs> OnDccChatRequestEvent;

        public void DccChatRequestEvent(DccEventArgs e)
        {
            if (OnDccChatRequestEvent != null)
            {
                OnDccChatRequestEvent(this, e);
            }
        }

        public event EventHandler<DccSendRequestEventArgs> OnDccSendRequestEvent;

        public void DccSendRequestEvent(DccSendRequestEventArgs e)
        {
            if (OnDccSendRequestEvent != null)
            {
                OnDccSendRequestEvent(this, e);
            }
        }

        public event EventHandler<DccEventArgs> OnDccChatStartEvent;

        public void DccChatStartEvent(DccEventArgs e)
        {
            if (OnDccChatStartEvent != null)
            {
                OnDccChatStartEvent(this, e);
            }
        }

        public event EventHandler<DccEventArgs> OnDccSendStartEvent;

        public void DccSendStartEvent(DccEventArgs e)
        {
            if (OnDccSendStartEvent != null)
            {
                OnDccSendStartEvent(this, e);
            }
        }

        public event EventHandler<DccChatEventArgs> OnDccChatReceiveLineEvent;

        public void DccChatReceiveLineEvent(DccChatEventArgs e)
        {
            if (OnDccChatReceiveLineEvent != null)
            {
                OnDccChatReceiveLineEvent(this, e);
            }
        }

        public event EventHandler<DccSendEventArgs> OnDccSendReceiveBlockEvent;

        public void DccSendReceiveBlockEvent(DccSendEventArgs e)
        {
            if (OnDccSendReceiveBlockEvent != null)
            {
                OnDccSendReceiveBlockEvent(this, e);
            }
        }

        public event EventHandler<DccChatEventArgs> OnDccChatSentLineEvent;

        public void DccChatSentLineEvent(DccChatEventArgs e)
        {
            if (OnDccChatSentLineEvent != null)
            {
                OnDccChatSentLineEvent(this, e);
            }
        }

        public event EventHandler<DccSendEventArgs> OnDccSendSentBlockEvent;

        internal void DccSendSentBlockEvent(DccSendEventArgs e)
        {
            if (OnDccSendSentBlockEvent != null)
            {
                OnDccSendSentBlockEvent(this, e);
            }
        }

        public event EventHandler<DccEventArgs> OnDccChatStopEvent;

        public void DccChatStopEvent(DccEventArgs e)
        {
            if (OnDccChatStopEvent != null)
            {
                OnDccChatStopEvent(this, e);
            }
        }

        public event EventHandler<DccEventArgs> OnDccSendStopEvent;

        public void DccSendStopEvent(DccEventArgs e)
        {
            if (OnDccSendStopEvent != null)
            {
                OnDccSendStopEvent(this, e);
            }
        }

        #endregion Public DCC Events (Global: All Dcc Events)

        #region Public Interface Methods

        public IrcFeatures()
        {
            // This method calls all the ctcp handlers defined below (or added anywhere else)
            OnCtcpRequest += CtcpRequestsHandler;

            // Adding ctcp handler, all commands are lower case (.ToLower() in handler)
            ctcpDelegates.Add("version", CtcpVersionDelegate);
            ctcpDelegates.Add("clientinfo", CtcpClientInfoDelegate);
            ctcpDelegates.Add("time", CtcpTimeDelegate);
            ctcpDelegates.Add("userinfo", CtcpUserInfoDelegate);
            ctcpDelegates.Add("url", CtcpUrlDelegate);
            ctcpDelegates.Add("source", CtcpSourceDelegate);
            ctcpDelegates.Add("finger", CtcpFingerDelegate);

            // The DCC Handler
            ctcpDelegates.Add("dcc", CtcpDccDelegate);

            // Don't remove the Ping handler without your own implementation
            ctcpDelegates.Add("ping", CtcpPingDelegate);
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
            var chat = new DccChat(this, user, ExternalIpAdress, passive, priority);
            dccConnections.Add(chat);
            ThreadPool.QueueUserWorkItem(chat.InitWork);
            RemoveInvalidDccConnections();
        }


        /// <summary>
        /// Send a local File
        /// </summary>
        /// <param name="user">Destination of the File (no channel)</param>
        /// <param name="filepath">complete filepath, absolute or relative (carefull)</param>
        public void SendFile(string user, string filepath)
        {
            var fi = new FileInfo(filepath);
            if (fi.Exists)
            {
                SendFile(user, new FileStream(filepath, FileMode.Open), fi.Name, fi.Length, DccSpeed.RfcSendAhead, false, Priority.Medium);
            }
        }

        /// <summary>
        /// Send a local File passivly
        /// </summary>
        /// <param name="user">Destination of the File (no channel)</param>
        /// <param name="filepath">complete filepath, absolute or relative (carefull)</param>
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
        /// <param name="filesize">give the length of the stream</param>
        public void SendFile(string user, Stream file, string filename, long filesize)
        {
            SendFile(user, file, filename, filesize, DccSpeed.RfcSendAhead, false);
        }

        /// <summary>
        /// Send any Stream, full flexibility in Dcc Connection Negotiation
        /// </summary>
        /// <param name="user">Destination of the File (no channel)</param>
        /// <param name="file">You can send any stream here</param>
        /// <param name="filename">give a filename for the remote User</param>
        /// <param name="filesize">give the length of the stream</param>
        /// <param name="speed">What ACK Managment should be used</param>
        /// <param name="passive">Passive DCC</param>
        public void SendFile(string user, Stream file, string filename, long filesize, DccSpeed speed, bool passive)
        {
            SendFile(user, file, filename, filesize, speed, passive, Priority.Medium);
        }

        /// <summary>
        /// Send any Stream, full flexibility in Dcc Connection Negotiation
        /// </summary>
        /// <param name="user">Destination of the File (no channel)</param>
        /// <param name="file">You can send any stream here</param>
        /// <param name="filename">give a filename for the remote User</param>
        /// <param name="filesize">give the length of the stream</param>
        /// <param name="speed">What ACK Managment should be used</param>
        /// <param name="passive">Passive DCC</param>
        /// <param name="priority">Non Dcc Message Priority for Negotiation</param>
        public void SendFile(string user, Stream file, string filename, long filesize, DccSpeed speed, bool passive,
                             Priority priority)
        {
            var send = new DccSend(this, user, ExternalIpAdress, file, filename, filesize, speed, passive, priority);
            dccConnections.Add(send);
            ThreadPool.QueueUserWorkItem(send.InitWork);
            RemoveInvalidDccConnections();
        }

        #endregion Public Interface Methods

        #region Private Methods

        private void CtcpRequestsHandler(object sender, CtcpEventArgs e)
        {
            if (ctcpDelegates.ContainsKey(e.CtcpCommand))
            {
                ctcpDelegates[e.CtcpCommand].Invoke(e);
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
            string clientInfo = ctcpDelegates.Aggregate("CLIENTINFO", (current, kvp) => current + " " + kvp.Key.ToUpper());
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
            //SendMessage(SendType.CtcpReply, e.Data.Nick, "FINGER " + this.Realname + " (" + this.Email + ") Idle " + this.Idle + " seconds (" + ((string.IsNullOrEmpty(this.Reason)) ? this.Reason : "-") + ") ");
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
                        var chat = new DccChat(this, ExternalIpAdress, e);
                        dccConnections.Add(chat);
                        ThreadPool.QueueUserWorkItem(chat.InitWork);
                        break;
                    case "SEND":
                        if (e.Data.MessageArray.Length > 6 && (FilterMarker(e.Data.MessageArray[6]) != "T"))
                        {
                            long session;
                            long.TryParse(FilterMarker(e.Data.MessageArray[6]), out session);
                            foreach (DccConnection dc in dccConnections)
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
                            var send = new DccSend(this, ExternalIpAdress, e);
                            dccConnections.Add(send);
                            ThreadPool.QueueUserWorkItem(send.InitWork);
                        }
                        break;
                    case "RESUME":
                        if (dccConnections.Any(dc => (dc is DccSend) && (((DccSend)dc).TryResume(e))))
                        {
                            return;
                        }
                        SendMessage(SendType.CtcpReply, e.Data.Nick, "ERRMSG Invalid DCC RESUME");
                        break;
                    case "ACCEPT":
                        if (dccConnections.Any(dc => (dc is DccSend) && (((DccSend)dc).TryAccept(e))))
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
        /// cleanup all old invalide DCCs (late cleaning)
        /// </summary>
        private void RemoveInvalidDccConnections()
        {
            foreach (DccConnection dc in dccConnections.Where(dc => (!dc.Valid) && (!dc.Connected)).ToList())
            {
                dccConnections.Remove(dc);
            }
        }

        private static string FilterMarker(IEnumerable<char> msg)
        {
            return msg.Where(c => c != IrcConstants.CtcpChar).Aggregate("", (current, c) => current + c);
        }

        #endregion implemented ctcp delegates, can be overwritten by changing the ctcpDelagtes Dictionary
    }
}