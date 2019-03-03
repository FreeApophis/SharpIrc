/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using System.Collections;
using System.Threading;
using SharpIrc;
using SharpIrc.IrcClient;
using SharpIrc.IrcClient.EventArgs;

namespace StressTest
{
    public class StressTest
    {
        // make an instance of the high-level API
        public static IrcClient irc = new IrcClient();

        // this method we will use to analyse queries (also known as private messages)
        public static void OnQueryMessage(object sender, IrcEventArgs e)
        {
            switch (e.Data.MessageArray[0])
            {
                // debug stuff
                case "dump_channel":
                    string requested_channel = e.Data.MessageArray[1];
                    // getting the channel (via channel sync feature)
                    Channel channel = irc.GetChannel(requested_channel);

                    // here we send messages
                    irc.SendMessage(SendType.Message, e.Data.Nick, "<channel '" + requested_channel + "'>");

                    irc.SendMessage(SendType.Message, e.Data.Nick, "Name: '" + channel.Name + "'");
                    irc.SendMessage(SendType.Message, e.Data.Nick, "Topic: '" + channel.Topic + "'");
                    irc.SendMessage(SendType.Message, e.Data.Nick, "Mode: '" + channel.Mode + "'");
                    irc.SendMessage(SendType.Message, e.Data.Nick, "Key: '" + channel.Key + "'");
                    irc.SendMessage(SendType.Message, e.Data.Nick, "UserLimit: '" + channel.UserLimit + "'");

                    // here we go through all users of the channel and show their
                    // hashtable key and nickname
                    string nickname_list = "";
                    nickname_list += "Users: ";
                    IDictionaryEnumerator it = channel.Users.GetEnumerator();
                    while (it.MoveNext())
                    {
                        string key = (string)it.Key;
                        ChannelUser channeluser = (ChannelUser)it.Value;
                        nickname_list += "(";
                        if (channeluser.IsOp)
                        {
                            nickname_list += "@";
                        }
                        if (channeluser.IsVoice)
                        {
                            nickname_list += "+";
                        }
                        nickname_list += ")" + key + " => " + channeluser.Nick + ", ";
                    }
                    irc.SendMessage(SendType.Message, e.Data.Nick, nickname_list);

                    irc.SendMessage(SendType.Message, e.Data.Nick, "</channel>");
                    break;
                case "gc":
                    GC.Collect();
                    break;
                // typical commands
                case "join":
                    irc.RfcJoin(e.Data.MessageArray[1]);
                    break;
                case "part":
                    irc.RfcPart(e.Data.MessageArray[1]);
                    break;
                case "die":
                    Exit();
                    break;
            }
        }

        public static void OnError(object sender, ErrorEventArgs e)
        {
            System.Console.WriteLine("Error: " + e.ErrorMessage);
            Exit();
        }

        public static void OnRawMessage(object sender, IrcEventArgs e)
        {
            System.Console.WriteLine("Received: " + e.Data.RawMessage);
        }

        public static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";
            irc.SendDelay = 400;
            irc.ActiveChannelSyncing = true;

            irc.OnQueryMessage += new EventHandler<IrcEventArgs>(OnQueryMessage);
            irc.OnError += new EventHandler<ErrorEventArgs>(OnError);
            irc.OnRawMessage += new EventHandler<IrcEventArgs>(OnRawMessage);

            string[] serverlist;
            // the server we want to connect to, could be also a simple string
            serverlist = new string[] { "irc.freshirc.com" };
            int port = 6667;
            string channel = "#OCS";
            try
            {
                irc.Connect(serverlist, port);
            }
            catch (ConnectionException e)
            {
                System.Console.WriteLine("couldn't connect! Reason: " + e.Message);
                Exit();
            }

            try
            {
                irc.Login("SharpIRC", "SharpIRC Test Bot");
                irc.RfcJoin(channel);
                new Thread(new ThreadStart(ReadCommands)).Start();
                irc.Listen();
                irc.Disconnect();
            }
            catch (ConnectionException)
            {
                Exit();
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Error occurred! Message: " + e.Message);
                System.Console.WriteLine("Exception: " + e.StackTrace);
                Exit();
            }
        }

        public static void ReadCommands()
        {
            while (true)
            {
                irc.WriteLine(System.Console.ReadLine());
            }
        }

        public static void Exit()
        {
            System.Console.WriteLine("Exiting...");
            System.Environment.Exit(0);
        }
    }
}
