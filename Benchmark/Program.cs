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
        const string SERVER = "irc.freenode.net";
        //const string SERVER   = "irc.freenet.de";
        //const string SERVER   = "10.1.0.101";
        const int PORT = 6667;
        const string NICK = "SharpIRCB";
        const string REALNAME = "SharpIRC Benchmark Bot";
        const string CHANNEL = "#C#";

        public static void Main(string[] args)
        {
            Thread.Sleep(5000);

            DateTime start, end;

            start = DateTime.UtcNow;
            TcpClientList();
            end = DateTime.UtcNow;
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
            TcpClient tc = new TcpClient(SERVER, PORT);
            StreamReader sr = new StreamReader(tc.GetStream());
            StreamWriter sw = new StreamWriter(tc.GetStream());
            sw.Write(Rfc2812.Nick(NICK) + "\r\n");
            sw.Write(Rfc2812.User(NICK, 0, REALNAME) + "\r\n");
            sw.Flush();

            string line;
            string[] linear;
            while (true)
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    linear = line.Split(new char[] { ' ' });
                    if (linear.Length >= 2 && linear[1] == "001")
                    {
                        sw.Write(Rfc2812.List(CHANNEL) + "\r\n");
                        sw.Flush();
                    }
                    if (linear.Length >= 5 && linear[1] == "322")
                    {
                        Console.WriteLine("On the IRC channel " + CHANNEL + " are " + linear[4] + " users");
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
            irc.OnRawMessage += new EventHandler<IrcEventArgs>(IrcClientListCallback);
            irc.Connect(SERVER, PORT);
            irc.Login(NICK, REALNAME);
            irc.RfcList(CHANNEL);
            irc.Listen();
        }

        public static void IrcClientListCallback(object sender, IrcEventArgs e)
        {
            if (e.Data.ReplyCode == ReplyCode.List)
            {
                Console.WriteLine("On the IRC channel " + CHANNEL + " are " + e.Data.RawMessageArray[4] + " users");
                e.Data.Irc.Disconnect();
            }
        }

        public static void IrcConnectionList()
        {
            IrcConnection irc = new IrcConnection();
            irc.OnReadLine += new EventHandler<ReadLineEventArgs>(IrcConnectionListCallback);
            irc.Connect(SERVER, PORT);
            irc.WriteLine(Rfc2812.Nick(NICK), Priority.Critical);
            irc.WriteLine(Rfc2812.User(NICK, 0, REALNAME), Priority.Critical);
            irc.WriteLine(Rfc2812.List(CHANNEL));
            irc.Listen();
        }

        public static void IrcConnectionListCallback(object sender, ReadLineEventArgs e)
        {
            string[] linear = e.Line.Split(new char[] { ' ' });
            if (linear.Length >= 5 && linear[1] == "322")
            {
                Console.WriteLine("On the IRC channel " + CHANNEL + " are " + linear[4] + " users");
                ((IrcConnection)sender).Disconnect();
            }
        }
    }
}
