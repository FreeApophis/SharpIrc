/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using SharpIrc.IrcFeatures.EventArgs;

namespace SharpIrc.IrcFeatures
{
    /// <summary>
    /// Base class for all DccConnections
    /// </summary>
    public class DccConnection
    {
        #region Private Variables

        protected TcpClient Connection;
        protected TcpListener DccServer;
        protected IPAddress ExternalIpAddress;
        protected IrcFeatures Irc;
        protected IPEndPoint LocalEndPoint;
        protected IPEndPoint RemoteEndPoint;
        protected DateTime Timeout;
        protected string User;

        protected bool IsConnected;
        protected bool IsValid = true;

        protected bool Reject;
        protected long SessionId;

        private class Session
        {
            private static long _next;

            internal static long Next => ++_next;
        }

        #endregion Private Variables

        #region Public Fields

        /// <summary>
        /// Returns false when the Connections is not Valid (before or after Connection)
        /// </summary>
        public bool Connected => IsConnected;

        /// <summary>
        /// Returns false when the Connections is not Valid anymore (only at the end)
        /// </summary>
        public bool Valid => IsValid && (IsConnected || (DateTime.Now < Timeout));

        /// <summary>
        /// Returns the Nick of the User we have a DCC with
        /// </summary>
        public string Nick => User;

        #endregion Public Fields

        #region Public DCC Events

        public event EventHandler<DccEventArgs> OnDccChatRequestEvent;

        protected virtual void DccChatRequestEvent(DccEventArgs e)
        {
            OnDccChatRequestEvent?.Invoke(this, e);
            Irc.DccChatRequestEvent(e);
        }

        public event EventHandler<DccSendRequestEventArgs> OnDccSendRequestEvent;

        protected virtual void DccSendRequestEvent(DccSendRequestEventArgs e)
        {
            OnDccSendRequestEvent?.Invoke(this, e);
            Irc.DccSendRequestEvent(e);
        }

        public event EventHandler<DccEventArgs> OnDccChatStartEvent;

        protected virtual void DccChatStartEvent(DccEventArgs e)
        {
            OnDccChatStartEvent?.Invoke(this, e);
            Irc.DccChatStartEvent(e);
        }

        public event EventHandler<DccEventArgs> OnDccSendStartEvent;

        protected virtual void DccSendStartEvent(DccEventArgs e)
        {
            OnDccSendStartEvent?.Invoke(this, e);
            Irc.DccSendStartEvent(e);
        }

        public event EventHandler<DccChatEventArgs> OnDccChatReceiveLineEvent;

        protected virtual void DccChatReceiveLineEvent(DccChatEventArgs e)
        {
            OnDccChatReceiveLineEvent?.Invoke(this, e);
            Irc.DccChatReceiveLineEvent(e);
        }

        public event EventHandler<DccSendEventArgs> OnDccSendReceiveBlockEvent;

        protected virtual void DccSendReceiveBlockEvent(DccSendEventArgs e)
        {
            OnDccSendReceiveBlockEvent?.Invoke(this, e);
            Irc.DccSendReceiveBlockEvent(e);
        }

        public event EventHandler<DccChatEventArgs> OnDccChatSentLineEvent;

        protected virtual void DccChatSentLineEvent(DccChatEventArgs e)
        {
            OnDccChatSentLineEvent?.Invoke(this, e);
            Irc.DccChatSentLineEvent(e);
        }

        public event EventHandler<DccSendEventArgs> OnDccSendSentBlockEvent;

        protected virtual void DccSendSentBlockEvent(DccSendEventArgs e)
        {
            OnDccSendSentBlockEvent?.Invoke(this, e);
            Irc.DccSendSentBlockEvent(e);
        }

        public event EventHandler<DccEventArgs> OnDccChatStopEvent;

        protected virtual void DccChatStopEvent(DccEventArgs e)
        {
            OnDccChatStopEvent?.Invoke(this, e);
            Irc.DccChatStopEvent(e);
        }

        public event EventHandler<DccEventArgs> OnDccSendStopEvent;

        protected virtual void DccSendStopEvent(DccEventArgs e)
        {
            OnDccSendStopEvent?.Invoke(this, e);
            Irc.DccSendStopEvent(e);
        }

        #endregion Public DCC Events

        internal DccConnection()
        {
            //Each DccConnection gets a Unique Identifier (just used internally until we have a TcpClient connected)
            SessionId = Session.Next;
            // If a Connection is not established within 120 Seconds we invalidate the DccConnection (see property Valid)
            Timeout = DateTime.Now.AddSeconds(120);
        }

        internal virtual void InitWork(Object stateInfo)
        {
            throw new NotSupportedException();
        }

        internal bool IsSession(long session)
        {
            return (session == SessionId);
        }

        #region Public Methods

        public void RejectRequest()
        {
            Irc.SendMessage(SendType.CtcpReply, User, "ERRMSG DCC Rejected");
            Reject = true;
            IsValid = false;
        }


        public void Disconnect()
        {
            IsConnected = false;
            IsValid = false;
        }

        public override string ToString()
        {
            return "DCC Session " + SessionId + " of " + GetType() + " is " + ((IsConnected) ? "connected to " + RemoteEndPoint.Address : "not connected") + "[" + User + "]";
        }

        #endregion Public Methods

        #region protected Helper Functions

        protected long HostToDccInt(IPAddress ip)
        {
            var bytes = ip.GetAddressBytes();

            if (bytes.Length != 4)
            {
                throw new NotImplementedException("IPv6 not supported");
            }

            long temp = 0;

#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
            temp |= (bytes[0]) << 24;
            temp |= (bytes[1]) << 16;
            temp |= (bytes[2]) << 8;
            temp |= (bytes[3]);
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand

            return temp;
        }

        protected string DccIntToHost(long ip)
        {
            var ep = new IPEndPoint(ip, 80);
            char[] sep = { '.' };
            string[] ipParts = ep.Address.ToString().Split(sep);
            return ipParts[3] + "." + ipParts[2] + "." + ipParts[1] + "." + ipParts[0];
        }

        protected byte[] GetAck(long sentBytes)
        {
            var acknowledged = new byte[4];
            acknowledged[0] = (byte)((sentBytes >> 24) % 256);
            acknowledged[1] = (byte)((sentBytes >> 16) % 256);
            acknowledged[2] = (byte)((sentBytes >> 8) % 256);
            acknowledged[3] = (byte)((sentBytes) % 256);
            return acknowledged;
        }

        protected string FilterMarker(string msg)
        {
            return msg.Where(c => c != IrcConstants.CtcpChar).Aggregate("", (current, c) => current + c);
        }

        #endregion protected Helper Functions
    }
}