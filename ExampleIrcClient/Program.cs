/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using SharpIrc;
using SharpIrc.IrcClient;
using SharpIrc.IrcClient.EventArgs;

// This is an VERY basic example how your IRC application could be written
// its mainly for showing how to use the API, this program just connects sends
// a few message to a channel and waits for commands on the console
// (raw RFC commands though! it's later explained).
// There are also a few commands the IRC bot/client allows via private message.
namespace IrcClient
{
    public class Program
    {
        // make an instance of the high-level API
        public static SharpIrc.IrcClient.IrcClient Client = new SharpIrc.IrcClient.IrcClient();

        // this method we will use to analyse queries (also known as private messages)
        public static void OnQueryMessage(object sender, IrcEventArgs e)
        {
            switch (e.Data.MessageArray[0])
            {
                // debug stuff
                case "dump_channel":
                    string requestedChannel = e.Data.MessageArray[1];
                    // getting the channel (via channel sync feature)
                    Channel channel = Client.GetChannel(requestedChannel);

                    // here we send messages
                    Client.SendMessage(SendType.Message, e.Data.Nick, "<channel '" + requestedChannel + "'>");

                    Client.SendMessage(SendType.Message, e.Data.Nick, "Name: '" + channel.Name + "'");
                    Client.SendMessage(SendType.Message, e.Data.Nick, "Topic: '" + channel.Topic + "'");
                    Client.SendMessage(SendType.Message, e.Data.Nick, "Mode: '" + channel.Mode + "'");
                    Client.SendMessage(SendType.Message, e.Data.Nick, "Key: '" + channel.Key + "'");
                    Client.SendMessage(SendType.Message, e.Data.Nick, "UserLimit: '" + channel.UserLimit + "'");

                    // here we go through all users of the channel and show their
                    // hashtable key and nickname
                    string nicknameList = "";
                    nicknameList += "Users: ";
                    foreach (DictionaryEntry de in channel.Users)
                    {
                        string key = (string)de.Key;
                        var channeluser = (ChannelUser)de.Value;
                        nicknameList += "(";
                        if (channeluser.IsOp)
                        {
                            nicknameList += "@";
                        }
                        if (channeluser.IsVoice)
                        {
                            nicknameList += "+";
                        }
                        nicknameList += ")" + key + " => " + channeluser.Nick + ", ";
                    }
                    Client.SendMessage(SendType.Message, e.Data.Nick, nicknameList);

                    Client.SendMessage(SendType.Message, e.Data.Nick, "</channel>");
                    break;
                case "gc":
                    GC.Collect();
                    break;
                // typical commands
                case "join":
                    Client.RfcJoin(e.Data.MessageArray[1]);
                    break;
                case "part":
                    Client.RfcPart(e.Data.MessageArray[1]);
                    break;
                case "die":
                    Exit();
                    break;
                case "server_properties":
                    foreach (var property in Client.Properties)
                    {
                        Client.SendMessage(SendType.Message, e.Data.Nick, property.Key + " => " + property.Value);

                    }
                    break;
            }
        }

        // this method handles when we receive "ERROR" from the IRC server
        public static void OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("Error: " + e.ErrorMessage);
            Exit();
        }

        // this method will get all IRC messages
        public static void OnRawMessage(object sender, IrcEventArgs e)
        {
            Console.WriteLine("Received: " + e.Data.RawMessage);
        }

        public static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";

            // UTF-8 test
            Client.Encoding = System.Text.Encoding.UTF8;

            // wait time between messages, we can set this lower on own irc servers
            Client.SendDelay = 200;

            // we use channel sync, means we can use irc.GetChannel() and so on
            Client.ActiveChannelSyncing = true;

            // here we connect the events of the API to our written methods
            // most have own event handler types, because they ship different data
            Client.OnQueryMessage += OnQueryMessage;
            Client.OnError += OnError;
            Client.OnRawMessage += OnRawMessage;

            // the server we want to connect to, could be also a simple string
            var serverlist = new[] { "irc.freenode.org" };
            const int port = 6667;
            const string channel = "#sharpirc-test";
            try
            {
                // here we try to connect to the server and exceptions get handled
                Client.Connect(serverlist, port);
            }
            catch (ConnectionException e)
            {
                // something went wrong, the reason will be shown
                Console.WriteLine("couldn't connect! Reason: " + e.Message);
                Exit();
            }

            try
            {
                // here we logon and register our nickname and so on
                Client.Login("SharpIRC", "SharpIRC Test Bot");
                // join the channel
                Client.RfcJoin(channel);

                for (int i = 0; i < 3; i++)
                {
                    // here we send just 3 different types of messages, 3 times for
                    // testing the delay and flood protection (messagebuffer work)
                    Client.SendMessage(SendType.Message, channel, "test message (" + i + ")");
                    Client.SendMessage(SendType.Action, channel, "thinks this is cool (" + i + ")");
                    Client.SendMessage(SendType.Notice, channel, "SharpIRC rocks (" + i + ")");
                }

                // spawn a new thread to read the stdin of the console, this we use
                // for reading IRC commands from the keyboard while the IRC connection
                // stays in its own thread
                new Thread(ReadCommands).Start();

                // here we tell the IRC API to go into a receive mode, all events
                // will be triggered by _this_ thread (main thread in this case)
                // Listen() blocks by default, you can also use ListenOnce() if you
                // need that does one IRC operation and then returns, so you need then
                // an own loop
                Client.Listen();

                // when Listen() returns our IRC session is over, to be sure we call
                // disconnect manually
                Client.Disconnect();
            }
            catch (ConnectionException)
            {
                // this exception is handled because Disconnect() can throw a not
                // connected exception
                Exit();
            }
            catch (Exception e)
            {
                // this should not happen by just in case we handle it nicely
                Console.WriteLine("Error occurred! Message: " + e.Message);
                Console.WriteLine("Exception: " + e.StackTrace);
                Exit();
            }
        }

        public static void ReadCommands()
        {
            // here we read the commands from the stdin and send it to the IRC API
            // WARNING, it uses WriteLine() means you need to enter RFC commands
            // like "JOIN #test" and then "PRIVMSG #test :hello to you"
            while (true)
            {
                string cmd = Console.ReadLine();
                if (cmd.StartsWith("/list"))
                {
                    int pos = cmd.IndexOf(" ");
                    string channel = null;
                    if (pos != -1)
                    {
                        channel = cmd.Substring(pos + 1);
                    }

                    IList<ChannelInfo> channelInfos = Client.GetChannelList(channel);
                    Console.WriteLine("channel count: {0}", channelInfos.Count);
                    foreach (ChannelInfo channelInfo in channelInfos)
                    {
                        Console.WriteLine("channel: {0} user count: {1} topic: {2}", channelInfo.Channel, channelInfo.UserCount, channelInfo.Topic);
                    }
                }
                else
                {
                    Client.WriteLine(cmd);
                }
            }
        }

        public static void Exit()
        {
            // we are done, lets exit...
            Console.WriteLine("Exiting...");
            Environment.Exit(0);
        }
    }
}
