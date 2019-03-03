/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System.Linq;

namespace SharpIrc.IrcClient
{
    /// <summary>
    /// This class manages the user information.
    /// </summary>
    /// <remarks>
    /// only used with channel sync
    /// <seealso cref="IrcClient.ActiveChannelSyncing">
    ///   IrcClient.ActiveChannelSyncing
    /// </seealso>
    /// </remarks>
    /// <threadsafety static="true" instance="true" />
    public class IrcUser
    {
        private readonly IrcClient ircClient;

        internal IrcUser(string nickname, IrcClient ircclient)
        {
            HopCount = -1;
            ircClient = ircclient;
            Nick = nickname;
        }

        /// <summary>
        /// Gets or sets the nickname of the user.
        /// </summary>
        /// <remarks>
        /// Do _not_ set this value, it will break channel sync!
        /// </remarks>
        public string Nick { get; set; }

        /// <summary>
        /// Gets or sets the identity (username) of the user which is used by some IRC networks for authentication.
        /// </summary>
        /// <remarks>
        /// Do _not_ set this value, it will break channel sync!
        /// </remarks>
        public string Ident { get; set; }

        /// <summary>
        /// Gets or sets the hostname of the user.
        /// </summary>
        /// <remarks>
        /// Do _not_ set this value, it will break channel sync!
        /// </remarks>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the supposed real name of the user.
        /// </summary>
        /// <remarks>
        /// Do _not_ set this value, it will break channel sync!
        /// </remarks>
        public string Realname { get; set; }

        /// <summary>
        /// Gets or sets the server operator status of the user
        /// </summary>
        /// <remarks>
        /// Do _not_ set this value, it will break channel sync!
        /// </remarks>
        public bool IsIrcOp { get; set; }

        /// <summary>
        /// Gets or sets the registered status of the user
        /// </summary>
        public bool IsRegistered { get; internal set; }

        /// <summary>
        /// Gets or sets away status of the user
        /// </summary>
        /// <remarks>
        /// Do _not_ set this value, it will break channel sync!
        /// </remarks>
        public bool IsAway { get; set; }

        /// <summary>
        /// Gets or sets the server the user is connected to
        /// </summary>
        /// <remarks>
        /// Do _not_ set this value, it will break channel sync!
        /// </remarks>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the count of hops between you and the user's server
        /// </summary>
        /// <remarks>
        /// Do _not_ set this value, it will break channel sync!
        /// </remarks>
        public int HopCount { get; set; }

        /// <summary>
        /// Gets the list of channels the user has joined
        /// </summary>
        public string[] JoinedChannels
        {
            get
            {
                string[] channels = ircClient.GetChannels();

                return channels
                    .Select(c => ircClient.GetChannel(c))
                    .Where(c => c.UnsafeUsers.ContainsKey(Nick))
                    .Select(c => c.Name).ToArray();
            }
        }
    }
}