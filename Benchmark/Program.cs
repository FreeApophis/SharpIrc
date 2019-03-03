/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using SharpIrc;
using SharpIrc.IrcClient;
using SharpIrc.IrcCommands;
using SharpIrc.IrcConnection;

namespace Benchmark
{
    public class Program
    {
        private const string Server = "irc.freenode.net";
        const int Port = 6667;
        const string Nick = "SharpIRC";
        const string RealName = "SharpIRC Benchmark Bot";
        const string Channel = "#C#";

        public static void Main(string[] args)
        {
            Thread.Sleep(5000);

            var start = DateTime.UtcNow;
            TcpClientList();
            var end = DateTime.UtcNow;
            Console.WriteLine("TcpClientList() took " + end.Subtract(start).TotalSeconds + " sec");
            Thread.Sleep(5000);

            start = DateTime.UtcNow;
            IrcConnectionList();
            end = DateTime.UtcNow;
            Console.WriteLine("IrcConnectionList() took " + end.Subtract(start).TotalSeconds + " sec");
            Thread.Sleep(5000);

            start = DateTime.UtcNow;
            IrcClientList();
            end = DateTime.UtcNow;
            Console.WriteLine("IrcClientList() took " + end.Subtract(start).TotalSeconds + " sec");
        }

        public static void TcpClientList()
        {
            TcpClient tc = new TcpClient(Server, Port);
            StreamReader sr = new StreamReader(tc.GetStream());
            StreamWriter sw = new StreamWriter(tc.GetStream());
            sw.Write(Rfc2812.Nick(Nick) + "\r\n");
            sw.Write(Rfc2812.User(Nick, 0, RealName) + "\r\n");
            sw.Flush();

            while (true)
            {
                var line = sr.ReadLine();
                if (line != null)
                {
                    var linear = line.Split(new[] { ' ' });
                    if (linear.Length >= 2 && linear[1] == "001")
                    {
                        sw.Write(Rfc2812.List(Channel) + "\r\n");
                        sw.Flush();
                    }
                    if (linear.Length >= 5 && linear[1] == "322")
                    {
                        Console.WriteLine("On the IRC channel " + Channel + " are " + linear[4] + " users");
                        sr.Close();
                        sw.Close();
                        tc.Close();
                        break;
                    }
                }
            }
        }

        public static void IrcClientList()
        {
            IrcClient irc = new IrcClient();
            irc.OnRawMessage += IrcClientListCallback;
            irc.Connect(Server, Port);
            irc.Login(Nick, RealName);
            irc.RfcList(Channel);
            irc.Listen();
        }

        public static void IrcClientListCallback(object sender, IrcEventArgs e)
        {
            if (e.Data.ReplyCode == ReplyCode.List)
            {
                Console.WriteLine("On the IRC channel " + Channel + " are " + e.Data.RawMessageArray[4] + " users");
                e.Data.Irc.Disconnect();
            }
        }

        public static void IrcConnectionList()
        {
            IrcConnection irc = new IrcConnection();
            irc.OnReadLine += IrcConnectionListCallback;
            irc.Connect(Server, Port);
            irc.WriteLine(Rfc2812.Nick(Nick), Priority.Critical);
            irc.WriteLine(Rfc2812.User(Nick, 0, RealName), Priority.Critical);
            irc.WriteLine(Rfc2812.List(Channel));
            irc.Listen();
        }

        public static void IrcConnectionListCallback(object sender, ReadLineEventArgs e)
        {
            string[] linear = e.Line.Split(new[] { ' ' });
            if (linear.Length >= 5 && linear[1] == "322")
            {
                Console.WriteLine("On the IRC channel " + Channel + " are " + linear[4] + " users");
                ((IrcConnection)sender).Disconnect();
            }
        }
    }
}
