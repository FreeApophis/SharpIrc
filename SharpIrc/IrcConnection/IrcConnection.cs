/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
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
using SharpIrc.IrcCommands;
using Starksoft.Net.Proxy;

namespace SharpIrc.IrcConnection
{
    /// <summary>
    ///
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    public class IrcConnection : MarshalByRefObject
    {
        private int _currentAddress;
        private StreamReader _reader;
        private StreamWriter _writer;
        private readonly ReadThread _readThread;
        private readonly WriteThread _writeThread;
        private readonly IdleWorkerThread _idleWorkerThread;
        private TcpClient _tcpClient;
        private readonly Hashtable _sendBuffer = Hashtable.Synchronized(new Hashtable());
        private bool _isConnectionError;
        private bool _isDisconnecting;
        private DateTime _lastPingSent;
        private DateTime _lastPongReceived;
        private TimeSpan _lag;

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
                    return _isConnectionError;
                }
            }
            set
            {
                lock (this)
                {
                    _isConnectionError = value;
                }
            }
        }

        protected bool IsDisconnecting
        {
            get
            {
                lock (this)
                {
                    return _isDisconnecting;
                }
            }
            set
            {
                lock (this)
                {
                    _isDisconnecting = value;
                }
            }
        }

        /// <summary>
        /// Gets the current address of the connection
        /// </summary>
        public string Address => AddressList[_currentAddress];

        /// <summary>
        /// Gets the address list of the connection
        /// </summary>
        public string[] AddressList { get; private set; } = { "localhost" };

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
        public bool AutoRetry { get; set; }

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
        public string VersionNumber { get; }

        /// <summary>
        /// Gets the full SharpIRC version string
        /// </summary>
        public string VersionString { get; }

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
                if (_lastPingSent > _lastPongReceived)
                {
                    // there is an outstanding ping, thus we don't have a current lag value
                    return DateTime.Now - _lastPingSent;
                }

                return _lag;
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
        /// you have to set the ProxyHost and ProxyPort as well (and give credentials if needed)
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
            _sendBuffer[Priority.High] = Queue.Synchronized(new Queue());
            _sendBuffer[Priority.AboveMedium] = Queue.Synchronized(new Queue());
            _sendBuffer[Priority.Medium] = Queue.Synchronized(new Queue());
            _sendBuffer[Priority.BelowMedium] = Queue.Synchronized(new Queue());
            _sendBuffer[Priority.Low] = Queue.Synchronized(new Queue());

            // setup own callbacks
            OnReadLine += SimpleParser;
            OnConnectionError += _OnConnectionError;

            _readThread = new ReadThread(this);
            _writeThread = new WriteThread(this);
            _idleWorkerThread = new IdleWorkerThread(this);

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
        /// <param name="addresses">List of servers to connect to</param>
        /// <param name="port">Port number to connect to</param>
        /// <exception cref="CouldNotConnectException">The connection failed</exception>
        /// <exception cref="AlreadyConnectedException">If there is already an active connection</exception>
        public void Connect(string[] addresses, int port)
        {
            if (IsConnected)
            {
                throw new AlreadyConnectedException("Already connected to: " + Address + ":" + Port);
            }

            AutoRetryAttempt++;

            AddressList = (string[])addresses.Clone();
            Port = port;

            OnConnecting?.Invoke(this, EventArgs.Empty);
            try
            {
                IPAddress ip = Dns.Resolve(Address).AddressList[0];

                _tcpClient = new TcpClient { NoDelay = true };
                _tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
                // set timeout, after this the connection will be aborted
                _tcpClient.ReceiveTimeout = SocketReceiveTimeout * 1000;
                _tcpClient.SendTimeout = SocketSendTimeout * 1000;

                if (ProxyType != ProxyType.None)
                {
                    IProxyClient proxyClient;
                    var proxyFactory = new ProxyClientFactory();

                    // HACK: map our ProxyType to Starksoft's ProxyType
                    var proxyType = (ProxyType)Enum.Parse(typeof(ProxyType), ProxyType.ToString(), true);

                    if (ProxyUsername == null && ProxyPassword == null)
                    {
                        proxyClient = proxyFactory.CreateProxyClient(proxyType);
                    }
                    else
                    {
                        proxyClient = proxyFactory.CreateProxyClient(proxyType, ProxyHost, ProxyPort, ProxyUsername, ProxyPassword);
                    }

                    _tcpClient.Connect(ProxyHost, ProxyPort);
                    proxyClient.TcpClient = _tcpClient;
                    proxyClient.CreateConnection(ip.ToString(), port);
                }
                else
                {
                    _tcpClient.Connect(ip, port);
                }

                Stream stream = _tcpClient.GetStream();
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
                _reader = new StreamReader(stream, Encoding);
                _writer = new StreamWriter(stream, Encoding);

                if (Encoding.GetPreamble().Length > 0)
                {
                    // HACK: we have an encoding that has some kind of preamble
                    // like UTF-8 has a BOM, this will confuse the IRCd!
                    // Thus we send a \r\n so the IRCd can safely ignore that
                    // garbage.
                    _writer.WriteLine();
                    // make sure we flush the BOM+CRLF correctly
                    _writer.Flush();
                }

                // Connection was successful, resetting the connect counter
                AutoRetryAttempt = 0;

                // updating the connection error state, so connecting is possible again
                IsConnectionError = false;
                IsConnected = true;

                // lets power up our threads
                _readThread.Start();
                _writeThread.Start();
                _idleWorkerThread.Start();

                OnConnected?.Invoke(this, EventArgs.Empty);
            }
            catch (AuthenticationException ex)
            {
                throw new CouldNotConnectException("Could not connect to: " + Address + ":" + Port + " " + ex.Message,
                                                   ex);
            }
            catch (Exception e)
            {
                if (_reader != null)
                {
                    try
                    {
                        _reader.Close();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
                if (_writer != null)
                {
                    try
                    {
                        _writer.Close();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }

                _tcpClient?.Close();
                IsConnected = false;
                IsConnectionError = true;

                if (e is CouldNotConnectException)
                {
                    // error was fatal, bail out
                    throw;
                }

                if (AutoRetry && (AutoRetryLimit == -1 || AutoRetryLimit == 0 || AutoRetryLimit <= AutoRetryAttempt))
                {
                    OnAutoConnectError?.Invoke(this, new AutoConnectErrorEventArgs(Address, Port, e));
                    Thread.Sleep(AutoRetryDelay * 1000);
                    NextAddress();
                    // FIXME: this is recursion
                    Connect(AddressList, Port);
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
            Connect(AddressList, Port);
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

            OnDisconnecting?.Invoke(this, EventArgs.Empty);

            IsDisconnecting = true;

            _readThread.Stop();
            _writeThread.Stop();
            _tcpClient.Close();
            IsConnected = false;
            IsRegistered = false;

            IsDisconnecting = false;

            OnDisconnected?.Invoke(this, EventArgs.Empty);

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
                       _readThread.Queue.Count == 0)
                {
                    Thread.Sleep(10);
                }
            }

            if (IsConnected &&
                _readThread.Queue.Count > 0)
            {
                data = (string)(_readThread.Queue.Dequeue());
            }

            if (!string.IsNullOrEmpty(data))
            {
                OnReadLine?.Invoke(this, new ReadLineEventArgs(data));
            }

            if (IsConnectionError &&
                !IsDisconnecting)
            {
                OnConnectionError?.Invoke(this, EventArgs.Empty);
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
                ((Queue)_sendBuffer[priority]).Enqueue(data);
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
                    _writer.Write(data + "\r\n");
                    _writer.Flush();
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

                OnWriteLine?.Invoke(this, new WriteLineEventArgs(data));
                return true;
            }

            return false;
        }

        private void NextAddress()
        {
            _currentAddress++;
            if (_currentAddress >= AddressList.Length)
            {
                _currentAddress = 0;
            }
        }

        private void SimpleParser(object sender, ReadLineEventArgs args)
        {
            string rawLien = args.Line;
            string[] rawLineEx = rawLien.Split(' ');
            string messageCode;

            if (rawLien[0] == ':')
            {
                messageCode = rawLineEx[1];

                ReplyCode replyCode = ReplyCode.Null;
                try
                {
                    replyCode = (ReplyCode)int.Parse(messageCode);
                }
                catch (FormatException)
                {
                }

                if (replyCode != ReplyCode.Null)
                {
                    switch (replyCode)
                    {
                        case ReplyCode.Welcome:
                            IsRegistered = true;
                            break;
                    }
                }
                else
                {
                    switch (rawLineEx[1])
                    {
                        case "PONG":
                            DateTime now = DateTime.Now;
                            _lastPongReceived = now;
                            _lag = now - _lastPingSent;

                            break;
                    }
                }
            }
            else
            {
                messageCode = rawLineEx[0];
                switch (messageCode)
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
            private readonly IrcConnection _connection;
            private Thread _thread;

            public Queue Queue { get; }

            /// <summary>
            ///
            /// </summary>
            /// <param name="connection"></param>
            public ReadThread(IrcConnection connection)
            {
                Queue = Queue.Synchronized(new Queue());
                _connection = connection;
            }

            /// <summary>
            ///
            /// </summary>
            public void Start()
            {
                _thread = new Thread(Worker)
                {
                    Name = "ReadThread (" + _connection.Address + ":" + _connection.Port + ")",
                    IsBackground = true
                };
                _thread.Start();
            }

            /// <summary>
            ///
            /// </summary>
            public void Stop()
            {
                _thread.Abort();
                // make sure we close the stream after the thread is gone, else
                // the thread will think the connection is broken!
                _thread.Join();

                try
                {
                    _connection._reader.Close();
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
                        while (_connection.IsConnected && ((data = _connection._reader.ReadLine()) != null))
                        {
                            Queue.Enqueue(data);
                        }
                    }
                    catch (IOException)
                    {
                    }
                    finally
                    {
                        // only flag this as connection error if we are not
                        // cleanly disconnecting
                        if (!_connection.IsDisconnecting)
                        {
                            _connection.IsConnectionError = true;
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        private class WriteThread
        {
            private readonly IrcConnection _connection;
            private int _aboveMediumCount;
            private int _aboveMediumSentCount;
            private const int AboveMediumThresholdCount = 4;
            private int _belowMediumCount;
            private int _belowMediumSentCount;
            private const int BelowMediumThresholdCount = 1;
            private int _burstCount;
            private int _highCount;
            private int _lowCount;
            private int _mediumCount;
            private int _mediumSentCount;
            private const int MediumThresholdCount = 2;
            private Thread _thread;

            /// <summary>
            ///
            /// </summary>
            /// <param name="connection"></param>
            public WriteThread(IrcConnection connection)
            {
                _connection = connection;
            }

            /// <summary>
            ///
            /// </summary>
            public void Start()
            {
                _thread = new Thread(_Worker)
                {
                    Name = "WriteThread (" + _connection.Address + ":" + _connection.Port + ")",
                    IsBackground = true
                };
                _thread.Start();
            }

            /// <summary>
            ///
            /// </summary>
            public void Stop()
            {

                _thread.Abort();
                // make sure we close the stream after the thread is gone, else
                // the thread will think the connection is broken!
                _thread.Join();

                try
                {
                    _connection._writer.Close();
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
                        while (_connection.IsConnected)
                        {
                            _CheckBuffer();
                            Thread.Sleep(_connection.SendDelay);
                        }
                    }
                    catch (IOException)
                    {
                    }
                    finally
                    {
                        // only flag this as connection error if we are not
                        // cleanly disconnecting
                        if (!_connection.IsDisconnecting)
                        {
                            _connection.IsConnectionError = true;
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
                catch (Exception)
                {
                }
            }

            #region WARNING: complex scheduler, don't even think about changing it!

            // WARNING: complex scheduler, don't even think about changing it!
            private void _CheckBuffer()
            {
                // only send data if we are successfully registered on the IRC network
                if (!_connection.IsRegistered)
                {
                    return;
                }

                _highCount = ((Queue)_connection._sendBuffer[Priority.High]).Count;
                _aboveMediumCount = ((Queue)_connection._sendBuffer[Priority.AboveMedium]).Count;
                _mediumCount = ((Queue)_connection._sendBuffer[Priority.Medium]).Count;
                _belowMediumCount = ((Queue)_connection._sendBuffer[Priority.BelowMedium]).Count;
                _lowCount = ((Queue)_connection._sendBuffer[Priority.Low]).Count;

                if (_CheckHighBuffer() &&
                    _CheckAboveMediumBuffer() &&
                    _CheckMediumBuffer() &&
                    _CheckBelowMediumBuffer() &&
                    _CheckLowBuffer())
                {
                    // everything is sent, resetting all counters
                    _aboveMediumSentCount = 0;
                    _mediumSentCount = 0;
                    _belowMediumSentCount = 0;
                    _burstCount = 0;
                }

                if (_burstCount < 3)
                {
                    _burstCount++;
                    //_CheckBuffer();
                }
            }

            private bool _CheckHighBuffer()
            {
                if (_highCount > 0)
                {
                    var data = (string)((Queue)_connection._sendBuffer[Priority.High]).Dequeue();
                    if (_connection._WriteLine(data) == false)
                    {
                        ((Queue)_connection._sendBuffer[Priority.High]).Enqueue(data);
                    }

                    if (_highCount > 1)
                    {
                        // there is more data to send
                        return false;
                    }
                }

                return true;
            }

            private bool _CheckAboveMediumBuffer()
            {
                if ((_aboveMediumCount > 0) &&
                    (_aboveMediumSentCount < AboveMediumThresholdCount))
                {
                    var data = (string)((Queue)_connection._sendBuffer[Priority.AboveMedium]).Dequeue();
                    if (_connection._WriteLine(data) == false)
                    {
                        ((Queue)_connection._sendBuffer[Priority.AboveMedium]).Enqueue(data);
                    }
                    _aboveMediumSentCount++;

                    if (_aboveMediumSentCount < AboveMediumThresholdCount)
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool _CheckMediumBuffer()
            {
                if ((_mediumCount > 0) &&
                    (_mediumSentCount < MediumThresholdCount))
                {
                    var data = (string)((Queue)_connection._sendBuffer[Priority.Medium]).Dequeue();
                    if (_connection._WriteLine(data) == false)
                    {
                        ((Queue)_connection._sendBuffer[Priority.Medium]).Enqueue(data);
                    }
                    _mediumSentCount++;

                    if (_mediumSentCount < MediumThresholdCount)
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool _CheckBelowMediumBuffer()
            {
                if ((_belowMediumCount > 0) &&
                    (_belowMediumSentCount < BelowMediumThresholdCount))
                {
                    var data = (string)((Queue)_connection._sendBuffer[Priority.BelowMedium]).Dequeue();
                    if (_connection._WriteLine(data) == false)
                    {
                        ((Queue)_connection._sendBuffer[Priority.BelowMedium]).Enqueue(data);
                    }
                    _belowMediumSentCount++;

                    if (_belowMediumSentCount < BelowMediumThresholdCount)
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool _CheckLowBuffer()
            {
                if (_lowCount > 0)
                {
                    if ((_highCount > 0) ||
                        (_aboveMediumCount > 0) ||
                        (_mediumCount > 0) ||
                        (_belowMediumCount > 0))
                    {
                        return true;
                    }

                    var data = (string)((Queue)_connection._sendBuffer[Priority.Low]).Dequeue();
                    if (_connection._WriteLine(data) == false)
                    {
                        ((Queue)_connection._sendBuffer[Priority.Low]).Enqueue(data);
                    }

                    if (_lowCount > 1)
                    {
                        return false;
                    }
                }

                return true;
            }

            // END OF WARNING, below this you can read/change again ;)

            #endregion WARNING: complex scheduler, don't even think about changing it!
        }

        /// <summary>
        ///
        /// </summary>
        private class IdleWorkerThread
        {
            private readonly IrcConnection _connection;
            private Thread _thread;

            /// <summary>
            ///
            /// </summary>
            /// <param name="connection"></param>
            public IdleWorkerThread(IrcConnection connection)
            {
                _connection = connection;
            }

            /// <summary>
            ///
            /// </summary>
            public void Start()
            {
                DateTime now = DateTime.Now;
                _connection._lastPingSent = now;
                _connection._lastPongReceived = now;

                _thread = new Thread(_Worker)
                {
                    Name = "IdleWorkerThread (" + _connection.Address + ":" + _connection.Port + ")",
                    IsBackground = true
                };
                _thread.Start();
            }

            /// <summary>
            ///
            /// </summary>
            public void Stop()
            {
                _thread.Abort();
            }

            private void _Worker()
            {
                try
                {
                    while (_connection.IsConnected)
                    {
                        Thread.Sleep(_connection.IdleWorkerInterval);

                        // only send active pings if we are registered
                        if (!_connection.IsRegistered)
                        {
                            continue;
                        }

                        DateTime now = DateTime.Now;
                        var lastPingSent = (int)(now - _connection._lastPingSent).TotalSeconds;
                        var lastPongReceived = (int)(now - _connection._lastPongReceived).TotalSeconds;
                        // determines if the response time is ok
                        if (lastPingSent < _connection.PingTimeout)
                        {
                            if (_connection._lastPingSent > _connection._lastPongReceived)
                            {
                                // there is a pending ping request, we have to wait
                                continue;
                            }

                            // determines if it need to send another ping yet
                            if (lastPongReceived > _connection.PingInterval)
                            {
                                _connection.WriteLine(Rfc2812.Ping(_connection.Address), Priority.Critical);
                                _connection._lastPingSent = now;
                                //_Connection._LastPongReceived = now;
                            } // else connection is fine, just continue
                        }
                        else
                        {
                            if (_connection.IsDisconnecting)
                            {
                                break;
                            }
                            // only flag this as connection error if we are not
                            // cleanly disconnecting
                            _connection.IsConnectionError = true;
                            break;
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
                catch (Exception)
                {
                }
            }
        }
    }
}