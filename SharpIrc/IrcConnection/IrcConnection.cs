/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 *
 * Copyright (c) 2003-2009 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
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
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using apophis.SharpIRC.IrcCommands;
using apophis.SharpIRC.StarkSoftProxy;

namespace apophis.SharpIRC.IrcConnection
{
    /// <summary>
    /// 
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    public class IrcConnection : MarshalByRefObject
    {
        private string[] addressList = { "localhost" };
        private int currentAddress;
        private StreamReader reader;
        private StreamWriter writer;
        private readonly ReadThread readThread;
        private readonly WriteThread writeThread;
        private readonly IdleWorkerThread idleWorkerThread;
        private TcpClient tcpClient;
        private readonly Hashtable sendBuffer = Hashtable.Synchronized(new Hashtable());
        private bool isConnectionError;
        private bool isDisconnecting;
        private bool autoRetry;
        private DateTime lastPingSent;
        private DateTime lastPongReceived;
        private TimeSpan lag;

        /// <event cref="OnReadLine">
        /// Raised when a \r\n terminated line is read from the socket
        /// </event>
        public event EventHandler<ReadLineEventArgs> OnReadLine;

        /// <event cref="OnWriteLine">
        /// Raised when a \r\n terminated line is written to the socket
        /// </event>
        public event EventHandler<WriteLineEventArgs> OnWriteLine;

        /// <event cref="OnConnecting">
        /// Raised before the connect attempt
        /// </event>
        public event EventHandler OnConnecting;

        /// <event cref="OnConnected">
        /// Raised on successful connect
        /// </event>
        public event EventHandler OnConnected;

        /// <event cref="OnDisconnecting">
        /// Raised before the connection is closed
        /// </event>
        public event EventHandler OnDisconnecting;

        /// <event cref="OnDisconnected">
        /// Raised when the connection is closed
        /// </event>
        public event EventHandler OnDisconnected;

        /// <event cref="OnConnectionError">
        /// Raised when the connection got into an error state
        /// </event>
        public event EventHandler OnConnectionError;

        /// <event cref="OnAutoConnectError">
        /// Raised when the connection got into an error state during auto connect loop
        /// </event>
        public event EventHandler<AutoConnectErrorEventArgs> OnAutoConnectError;

        /// <summary>
        /// When a connection error is detected this property will return true
        /// </summary>
        protected bool IsConnectionError
        {
            get
            {
                lock (this)
                {
                    return isConnectionError;
                }
            }
            set
            {
                lock (this)
                {
                    isConnectionError = value;
                }
            }
        }

        protected bool IsDisconnecting
        {
            get
            {
                lock (this)
                {
                    return isDisconnecting;
                }
            }
            set
            {
                lock (this)
                {
                    isDisconnecting = value;
                }
            }
        }

        /// <summary>
        /// Gets the current address of the connection
        /// </summary>
        public string Address
        {
            get { return addressList[currentAddress]; }
        }

        /// <summary>
        /// Gets the address list of the connection
        /// </summary>
        public string[] AddressList
        {
            get { return addressList; }
        }

        /// <summary>
        /// Gets the used port of the connection
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// By default nothing is done when the library looses the connection
        /// to the server.
        /// Default: false
        /// </summary>
        /// <value>
        /// true, if the library should reconnect on lost connections
        /// false, if the library should not take care of it
        /// </value>
        public bool AutoReconnect { get; set; }

        /// <summary>
        /// If the library should retry to connect when the connection fails.
        /// Default: false
        /// </summary>
        /// <value>
        /// true, if the library should retry to connect
        /// false, if the library should not retry
        /// </value>
        public bool AutoRetry
        {
            get { return autoRetry; }
            set
            {
                autoRetry = value;
            }
        }

        /// <summary>
        /// Delay between retry attempts in Connect() in seconds.
        /// Default: 30
        /// </summary>
        public int AutoRetryDelay { get; set; }

        /// <summary>
        /// Maximum number of retries to connect to the server
        /// Default: 3
        /// </summary>
        public int AutoRetryLimit { get; set; }

        /// <summary>
        /// Returns the current amount of reconnect attempts
        /// Default: 3
        /// </summary>
        public int AutoRetryAttempt { get; private set; }

        /// <summary>
        /// To prevent flooding the IRC server, it's required to delay each
        /// message, given in milliseconds.
        /// Default: 200
        /// </summary>
        public int SendDelay { get; set; }

        /// <summary>
        /// On successful registration on the IRC network, this is set to true.
        /// </summary>
        public bool IsRegistered { get; private set; }

        /// <summary>
        /// On successful connect to the IRC server, this is set to true.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Gets the SharpIRC version number
        /// </summary>
        public string VersionNumber { get; private set; }

        /// <summary>
        /// Gets the full SharpIRC version string
        /// </summary>
        public string VersionString { get; private set; }

        /// <summary>
        /// Encoding which is used for reading and writing to the socket
        /// Default: encoding of the system
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Enables/disables using SSL for the connection
        /// Default: false
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// Specifies if the certificate of the server is validated
        /// Default: true
        /// </summary>
        public bool ValidateServerCertificate { get; set; }

        /// <summary>
        /// Specifies the client certificate used for the SSL connection
        /// Default: null
        /// </summary>
        public X509Certificate SslClientCertificate { get; set; }

        /// <summary>
        /// Timeout in seconds for receiving data from the socket
        /// Default: 600
        /// </summary>
        public int SocketReceiveTimeout { get; set; }

        /// <summary>
        /// Timeout in seconds for sending data to the socket
        /// Default: 600
        /// </summary>
        public int SocketSendTimeout { get; set; }

        /// <summary>
        /// Interval in seconds to run the idle worker
        /// Default: 60
        /// </summary>
        public int IdleWorkerInterval { get; set; }

        /// <summary>
        /// Interval in seconds to send a PING
        /// Default: 60
        /// </summary>
        public int PingInterval { get; set; }

        /// <summary>
        /// Timeout in seconds for server response to a PING
        /// Default: 600
        /// </summary>
        public int PingTimeout { get; set; }

        /// <summary>
        /// Latency between client and the server
        /// </summary>
        public TimeSpan Lag
        {
            get
            {
                if (lastPingSent > lastPongReceived)
                {
                    // there is an outstanding ping, thus we don't have a current lag value
                    return DateTime.Now - lastPingSent;
                }

                return lag;
            }
        }


        /// <summary>
        /// If you want to use a Proxy, set the ProxyHost to Host of the Proxy you want to use.
        /// </summary>
        public string ProxyHost { get; set; }

        /// <summary>
        /// If you want to use a Proxy, set the ProxyPort to Port of the Proxy you want to use.
        /// </summary>
        public int ProxyPort { get; set; }

        /// <summary>
        /// Standard Setting is to use no Proxy Server, if you Set this to any other value,
        /// you have to set the ProxyHost and ProxyPort aswell (and give credentials if needed)
        /// Default: ProxyType.None
        /// </summary>
        public ProxyType ProxyType { get; set; }

        /// <summary>
        /// Username to your Proxy Server
        /// </summary>
        public string ProxyUsername { get; set; }

        /// <summary>
        /// Password to your Proxy Server
        /// </summary>
        public string ProxyPassword { get; set; }

        /// <summary>
        /// Initializes the message queues, read and write thread
        /// </summary>
        public IrcConnection()
        {
            ProxyType = ProxyType.None;
            PingTimeout = 300;
            PingInterval = 60;
            IdleWorkerInterval = 60;
            SocketSendTimeout = 600;
            SocketReceiveTimeout = 600;
            Encoding = Encoding.Default;
            AutoRetryLimit = 3;
            AutoRetryDelay = 30;
            SendDelay = 200;
            sendBuffer[Priority.High] = Queue.Synchronized(new Queue());
            sendBuffer[Priority.AboveMedium] = Queue.Synchronized(new Queue());
            sendBuffer[Priority.Medium] = Queue.Synchronized(new Queue());
            sendBuffer[Priority.BelowMedium] = Queue.Synchronized(new Queue());
            sendBuffer[Priority.Low] = Queue.Synchronized(new Queue());

            // setup own callbacks
            OnReadLine += SimpleParser;
            OnConnectionError += _OnConnectionError;

            readThread = new ReadThread(this);
            writeThread = new WriteThread(this);
            idleWorkerThread = new IdleWorkerThread(this);

            Assembly assembly = Assembly.GetAssembly(GetType());
            AssemblyName assemblyName = assembly.GetName(false);

            var pr = (AssemblyProductAttribute)assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];

            VersionNumber = assemblyName.Version.ToString();
            VersionString = pr.Product + " " + VersionNumber;
        }

        /// <overloads>this method has 2 overloads</overloads>
        /// <summary>
        /// Connects to the specified server and port, when the connection fails
        /// the next server in the list will be used.
        /// </summary>
        /// <param name="addresslist">List of servers to connect to</param>
        /// <param name="port">Portnumber to connect to</param>
        /// <exception cref="CouldNotConnectException">The connection failed</exception>
        /// <exception cref="AlreadyConnectedException">If there is already an active connection</exception>
        public void Connect(string[] addresslist, int port)
        {
            if (IsConnected)
            {
                throw new AlreadyConnectedException("Already connected to: " + Address + ":" + Port);
            }

            AutoRetryAttempt++;

            addressList = (string[])addresslist.Clone();
            Port = port;

            if (OnConnecting != null)
            {
                OnConnecting(this, EventArgs.Empty);
            }
            try
            {
                IPAddress ip = Dns.Resolve(Address).AddressList[0];

                tcpClient = new TcpClient { NoDelay = true };
                tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
                // set timeout, after this the connection will be aborted
                tcpClient.ReceiveTimeout = SocketReceiveTimeout * 1000;
                tcpClient.SendTimeout = SocketSendTimeout * 1000;

                if (ProxyType != ProxyType.None)
                {
                    IProxyClient proxyClient;
                    var proxyFactory = new ProxyClientFactory();

                    // HACK: map our ProxyType to Starksoft's ProxyType
                    var proxyType = (StarkSoftProxy.ProxyType)Enum.Parse(typeof(ProxyType), ProxyType.ToString(), true);

                    if (ProxyUsername == null && ProxyPassword == null)
                    {
                        proxyClient = proxyFactory.CreateProxyClient(proxyType);
                    }
                    else
                    {
                        proxyClient = proxyFactory.CreateProxyClient(proxyType, ProxyHost, ProxyPort, ProxyUsername, ProxyPassword);
                    }

                    tcpClient.Connect(ProxyHost, ProxyPort);
                    proxyClient.TcpClient = tcpClient;
                    proxyClient.CreateConnection(ip.ToString(), port);
                }
                else
                {
                    tcpClient.Connect(ip, port);
                }

                Stream stream = tcpClient.GetStream();
                if (UseSsl)
                {
                    RemoteCertificateValidationCallback certValidation = ValidateServerCertificate
                        ? ((sender, certificate, chain, sslPolicyErrors) => sslPolicyErrors == SslPolicyErrors.None)
                        : (RemoteCertificateValidationCallback)((sender, certificate, chain, errors) => true);

                    var sslStream = new SslStream(stream, false, certValidation);
                    try
                    {
                        if (SslClientCertificate != null)
                        {
                            sslStream.AuthenticateAsClient(Address, new X509Certificate2Collection { SslClientCertificate }, SslProtocols.Default, false);
                        }
                        else
                        {
                            sslStream.AuthenticateAsClient(Address);
                        }
                    }
                    catch (IOException ex)
                    {
                        throw new CouldNotConnectException("Could not connect to: " + Address + ":" + Port + " " + ex.Message, ex);
                    }
                    stream = sslStream;
                }
                reader = new StreamReader(stream, Encoding);
                writer = new StreamWriter(stream, Encoding);

                if (Encoding.GetPreamble().Length > 0)
                {
                    // HACK: we have an encoding that has some kind of preamble
                    // like UTF-8 has a BOM, this will confuse the IRCd!
                    // Thus we send a \r\n so the IRCd can safely ignore that
                    // garbage.
                    writer.WriteLine();
                    // make sure we flush the BOM+CRLF correctly
                    writer.Flush();
                }

                // Connection was succeful, reseting the connect counter
                AutoRetryAttempt = 0;

                // updating the connection error state, so connecting is possible again
                IsConnectionError = false;
                IsConnected = true;

                // lets power up our threads
                readThread.Start();
                writeThread.Start();
                idleWorkerThread.Start();

                if (OnConnected != null)
                {
                    OnConnected(this, EventArgs.Empty);
                }
            }
            catch (AuthenticationException ex)
            {
                throw new CouldNotConnectException("Could not connect to: " + Address + ":" + Port + " " + ex.Message,
                                                   ex);
            }
            catch (Exception e)
            {
                if (reader != null)
                {
                    try
                    {
                        reader.Close();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
                if (writer != null)
                {
                    try
                    {
                        writer.Close();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
                if (tcpClient != null)
                {
                    tcpClient.Close();
                }
                IsConnected = false;
                IsConnectionError = true;

                if (e is CouldNotConnectException)
                {
                    // error was fatal, bail out
                    throw;
                }

                if (autoRetry && (AutoRetryLimit == -1 || AutoRetryLimit == 0 || AutoRetryLimit <= AutoRetryAttempt))
                {
                    if (OnAutoConnectError != null)
                    {
                        OnAutoConnectError(this, new AutoConnectErrorEventArgs(Address, Port, e));
                    }
                    Thread.Sleep(AutoRetryDelay * 1000);
                    NextAddress();
                    // FIXME: this is recursion
                    Connect(addressList, Port);
                }
                else
                {
                    throw new CouldNotConnectException(
                        "Could not connect to: " + Address + ":" + Port + " " + e.Message, e);
                }
            }
        }

        /// <summary>
        /// Connects to the specified server and port.
        /// </summary>
        /// <param name="address">Server address to connect to</param>
        /// <param name="port">Port number to connect to</param>
        public void Connect(string address, int port)
        {
            Connect(new[] { address }, port);
        }

        /// <summary>
        /// Reconnects to the server
        /// </summary>
        /// <exception cref="NotConnectedException">
        /// If there was no active connection
        /// </exception>
        /// <exception cref="CouldNotConnectException">
        /// The connection failed
        /// </exception>
        /// <exception cref="AlreadyConnectedException">
        /// If there is already an active connection
        /// </exception>
        public void Reconnect()
        {
            Disconnect();
            Connect(addressList, Port);
        }

        /// <summary>
        /// Disconnects from the server
        /// </summary>
        /// <exception cref="NotConnectedException">
        /// If there was no active connection
        /// </exception>
        public void Disconnect()
        {
            if (!IsConnected)
            {
                throw new NotConnectedException(
                    "The connection could not be disconnected because there is no active connection");
            }

            if (OnDisconnecting != null)
            {
                OnDisconnecting(this, EventArgs.Empty);
            }

            IsDisconnecting = true;

            readThread.Stop();
            writeThread.Stop();
            tcpClient.Close();
            IsConnected = false;
            IsRegistered = false;

            IsDisconnecting = false;

            if (OnDisconnected != null)
            {
                OnDisconnected(this, EventArgs.Empty);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blocking"></param>
        public void Listen(bool blocking)
        {
            if (blocking)
            {
                while (IsConnected)
                {
                    ReadLine(true);
                }
            }
            else
            {
                while (ReadLine(false).Length > 0)
                {
                    // loop as long as we receive messages
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Listen()
        {
            Listen(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blocking"></param>
        public void ListenOnce(bool blocking)
        {
            ReadLine(blocking);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ListenOnce()
        {
            ListenOnce(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blocking"></param>
        /// <returns></returns>
        public string ReadLine(bool blocking)
        {
            string data = "";
            if (blocking)
            {
                // block till the queue has data, but bail out on connection error
                while (IsConnected &&
                       !IsConnectionError &&
                       readThread.Queue.Count == 0)
                {
                    Thread.Sleep(10);
                }
            }

            if (IsConnected &&
                readThread.Queue.Count > 0)
            {
                data = (string)(readThread.Queue.Dequeue());
            }

            if (!string.IsNullOrEmpty(data))
            {
                if (OnReadLine != null)
                {
                    OnReadLine(this, new ReadLineEventArgs(data));
                }
            }

            if (IsConnectionError &&
                !IsDisconnecting &&
                OnConnectionError != null)
            {
                OnConnectionError(this, EventArgs.Empty);
            }

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="priority"></param>
        public void WriteLine(string data, Priority priority)
        {
            if (priority == Priority.Critical)
            {
                if (!IsConnected)
                {
                    throw new NotConnectedException();
                }

                _WriteLine(data);
            }
            else
            {
                ((Queue)sendBuffer[priority]).Enqueue(data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void WriteLine(string data)
        {
            WriteLine(data, Priority.Medium);
        }

        private bool _WriteLine(string data)
        {
            if (IsConnected)
            {
                try
                {
                    writer.Write(data + "\r\n");
                    writer.Flush();
                }
                catch (IOException)
                {
                    IsConnectionError = true;
                    return false;
                }
                catch (ObjectDisposedException)
                {
                    IsConnectionError = true;
                    return false;
                }

                if (OnWriteLine != null)
                {
                    OnWriteLine(this, new WriteLineEventArgs(data));
                }
                return true;
            }

            return false;
        }

        private void NextAddress()
        {
            currentAddress++;
            if (currentAddress >= addressList.Length)
            {
                currentAddress = 0;
            }
        }

        private void SimpleParser(object sender, ReadLineEventArgs args)
        {
            string rawline = args.Line;
            string[] rawlineex = rawline.Split(new[] { ' ' });
            string messagecode;

            if (rawline[0] == ':')
            {
                messagecode = rawlineex[1];

                ReplyCode replycode = ReplyCode.Null;
                try
                {
                    replycode = (ReplyCode)int.Parse(messagecode);
                }
                catch (FormatException)
                {
                }

                if (replycode != ReplyCode.Null)
                {
                    switch (replycode)
                    {
                        case ReplyCode.Welcome:
                            IsRegistered = true;
                            break;
                    }
                }
                else
                {
                    switch (rawlineex[1])
                    {
                        case "PONG":
                            DateTime now = DateTime.Now;
                            lastPongReceived = now;
                            lag = now - lastPingSent;

                            break;
                    }
                }
            }
            else
            {
                messagecode = rawlineex[0];
                switch (messagecode)
                {
                    case "ERROR":
                        // FIXME: handle server errors differently than connection errors!
                        //IsConnectionError = true;
                        break;
                }
            }
        }

        private void _OnConnectionError(object sender, EventArgs e)
        {
            try
            {
                if (AutoReconnect)
                {
                    // lets try to recover the connection
                    Reconnect();
                }
                else
                {
                    // make sure we clean up
                    Disconnect();
                }
            }
            catch (ConnectionException)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class ReadThread
        {
            private readonly IrcConnection connection;
            private Thread thread;

            public Queue Queue { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="connection"></param>
            public ReadThread(IrcConnection connection)
            {
                Queue = Queue.Synchronized(new Queue());
                this.connection = connection;
            }

            /// <summary>
            /// 
            /// </summary>
            public void Start()
            {
                thread = new Thread(Worker)
                {
                    Name = "ReadThread (" + connection.Address + ":" + connection.Port + ")",
                    IsBackground = true
                };
                thread.Start();
            }

            /// <summary>
            /// 
            /// </summary>
            public void Stop()
            {
                thread.Abort();
                // make sure we close the stream after the thread is gone, else
                // the thread will think the connection is broken!
                thread.Join();

                try
                {
                    connection.reader.Close();
                }
                catch (ObjectDisposedException)
                {
                }
            }

            private void Worker()
            {
                try
                {
                    try
                    {
                        string data;
                        while (connection.IsConnected && ((data = connection.reader.ReadLine()) != null))
                        {
                            Queue.Enqueue(data);
                        }
                    }
                    catch (IOException e)
                    {
                    }
                    finally
                    {
                        // only flag this as connection error if we are not
                        // cleanly disconnecting
                        if (!connection.IsDisconnecting)
                        {
                            connection.IsConnectionError = true;
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
                catch (Exception ex)
                {
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class WriteThread
        {
            private readonly IrcConnection connection;
            private int aboveMediumCount;
            private int aboveMediumSentCount;
            private const int AboveMediumThresholdCount = 4;
            private int belowMediumCount;
            private int belowMediumSentCount;
            private const int BelowMediumThresholdCount = 1;
            private int burstCount;
            private int highCount;
            private int lowCount;
            private int mediumCount;
            private int mediumSentCount;
            private const int MediumThresholdCount = 2;
            private Thread thread;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="connection"></param>
            public WriteThread(IrcConnection connection)
            {
                this.connection = connection;
            }

            /// <summary>
            /// 
            /// </summary>
            public void Start()
            {
                thread = new Thread(_Worker)
                {
                    Name = "WriteThread (" + connection.Address + ":" + connection.Port + ")",
                    IsBackground = true
                };
                thread.Start();
            }

            /// <summary>
            /// 
            /// </summary>
            public void Stop()
            {

                thread.Abort();
                // make sure we close the stream after the thread is gone, else
                // the thread will think the connection is broken!
                thread.Join();

                try
                {
                    connection.writer.Close();
                }
                catch (ObjectDisposedException)
                {
                }
            }

            private void _Worker()
            {
                try
                {
                    try
                    {
                        while (connection.IsConnected)
                        {
                            _CheckBuffer();
                            Thread.Sleep(connection.SendDelay);
                        }
                    }
                    catch (IOException e)
                    {
                    }
                    finally
                    {
                        // only flag this as connection error if we are not
                        // cleanly disconnecting
                        if (!connection.IsDisconnecting)
                        {
                            connection.IsConnectionError = true;
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
                catch (Exception ex)
                {
                }
            }

            #region WARNING: complex scheduler, don't even think about changing it!

            // WARNING: complex scheduler, don't even think about changing it!
            private void _CheckBuffer()
            {
                // only send data if we are succefully registered on the IRC network
                if (!connection.IsRegistered)
                {
                    return;
                }

                highCount = ((Queue)connection.sendBuffer[Priority.High]).Count;
                aboveMediumCount = ((Queue)connection.sendBuffer[Priority.AboveMedium]).Count;
                mediumCount = ((Queue)connection.sendBuffer[Priority.Medium]).Count;
                belowMediumCount = ((Queue)connection.sendBuffer[Priority.BelowMedium]).Count;
                lowCount = ((Queue)connection.sendBuffer[Priority.Low]).Count;

                if (_CheckHighBuffer() &&
                    _CheckAboveMediumBuffer() &&
                    _CheckMediumBuffer() &&
                    _CheckBelowMediumBuffer() &&
                    _CheckLowBuffer())
                {
                    // everything is sent, resetting all counters
                    aboveMediumSentCount = 0;
                    mediumSentCount = 0;
                    belowMediumSentCount = 0;
                    burstCount = 0;
                }

                if (burstCount < 3)
                {
                    burstCount++;
                    //_CheckBuffer();
                }
            }

            private bool _CheckHighBuffer()
            {
                if (highCount > 0)
                {
                    var data = (string)((Queue)connection.sendBuffer[Priority.High]).Dequeue();
                    if (connection._WriteLine(data) == false)
                    {
                        ((Queue)connection.sendBuffer[Priority.High]).Enqueue(data);
                    }

                    if (highCount > 1)
                    {
                        // there is more data to send
                        return false;
                    }
                }

                return true;
            }

            private bool _CheckAboveMediumBuffer()
            {
                if ((aboveMediumCount > 0) &&
                    (aboveMediumSentCount < AboveMediumThresholdCount))
                {
                    var data = (string)((Queue)connection.sendBuffer[Priority.AboveMedium]).Dequeue();
                    if (connection._WriteLine(data) == false)
                    {
                        ((Queue)connection.sendBuffer[Priority.AboveMedium]).Enqueue(data);
                    }
                    aboveMediumSentCount++;

                    if (aboveMediumSentCount < AboveMediumThresholdCount)
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool _CheckMediumBuffer()
            {
                if ((mediumCount > 0) &&
                    (mediumSentCount < MediumThresholdCount))
                {
                    var data = (string)((Queue)connection.sendBuffer[Priority.Medium]).Dequeue();
                    if (connection._WriteLine(data) == false)
                    {
                        ((Queue)connection.sendBuffer[Priority.Medium]).Enqueue(data);
                    }
                    mediumSentCount++;

                    if (mediumSentCount < MediumThresholdCount)
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool _CheckBelowMediumBuffer()
            {
                if ((belowMediumCount > 0) &&
                    (belowMediumSentCount < BelowMediumThresholdCount))
                {
                    var data = (string)((Queue)connection.sendBuffer[Priority.BelowMedium]).Dequeue();
                    if (connection._WriteLine(data) == false)
                    {
                        ((Queue)connection.sendBuffer[Priority.BelowMedium]).Enqueue(data);
                    }
                    belowMediumSentCount++;

                    if (belowMediumSentCount < BelowMediumThresholdCount)
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool _CheckLowBuffer()
            {
                if (lowCount > 0)
                {
                    if ((highCount > 0) ||
                        (aboveMediumCount > 0) ||
                        (mediumCount > 0) ||
                        (belowMediumCount > 0))
                    {
                        return true;
                    }

                    var data = (string)((Queue)connection.sendBuffer[Priority.Low]).Dequeue();
                    if (connection._WriteLine(data) == false)
                    {
                        ((Queue)connection.sendBuffer[Priority.Low]).Enqueue(data);
                    }

                    if (lowCount > 1)
                    {
                        return false;
                    }
                }

                return true;
            }

            // END OF WARNING, below this you can read/change again ;)

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        private class IdleWorkerThread
        {
            private readonly IrcConnection connection;
            private Thread thread;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="connection"></param>
            public IdleWorkerThread(IrcConnection connection)
            {
                this.connection = connection;
            }

            /// <summary>
            /// 
            /// </summary>
            public void Start()
            {
                DateTime now = DateTime.Now;
                connection.lastPingSent = now;
                connection.lastPongReceived = now;

                thread = new Thread(_Worker)
                {
                    Name = "IdleWorkerThread (" + connection.Address + ":" + connection.Port + ")",
                    IsBackground = true
                };
                thread.Start();
            }

            /// <summary>
            /// 
            /// </summary>
            public void Stop()
            {
                thread.Abort();
            }

            private void _Worker()
            {
                try
                {
                    while (connection.IsConnected)
                    {
                        Thread.Sleep(connection.IdleWorkerInterval);

                        // only send active pings if we are registered
                        if (!connection.IsRegistered)
                        {
                            continue;
                        }

                        DateTime now = DateTime.Now;
                        var lastPingSent = (int)(now - connection.lastPingSent).TotalSeconds;
                        var lastPongRcvd = (int)(now - connection.lastPongReceived).TotalSeconds;
                        // determins if the resoponse time is ok
                        if (lastPingSent < connection.PingTimeout)
                        {
                            if (connection.lastPingSent > connection.lastPongReceived)
                            {
                                // there is a pending ping request, we have to wait
                                continue;
                            }

                            // determines if it need to send another ping yet
                            if (lastPongRcvd > connection.PingInterval)
                            {
                                connection.WriteLine(Rfc2812.Ping(connection.Address), Priority.Critical);
                                connection.lastPingSent = now;
                                //_Connection._LastPongReceived = now;
                            } // else connection is fine, just continue
                        }
                        else
                        {
                            if (connection.IsDisconnecting)
                            {
                                break;
                            }
                            // only flag this as connection error if we are not
                            // cleanly disconnecting
                            connection.IsConnectionError = true;
                            break;
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}