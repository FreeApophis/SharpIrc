/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

namespace SharpIrc.IrcClient
{
    /// <summary>
    /// This class manages the information of a user within a channel.
    /// </summary>
    /// <remarks>
    /// only used with channel sync
    /// </remarks>
    /// <threadsafety static="true" instance="true" />
    public class ChannelUser
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="channel"> </param>
        /// <param name="ircUser"> </param>
        internal ChannelUser(string channel, IrcUser ircUser)
        {
            Channel = channel;
            IrcUser = ircUser;
        }

        /// <summary>
        /// Gets the channel name
        /// </summary>
        public string Channel { get; }

        /// <summary>
        /// Gets the server operator status of the user
        /// </summary>
        public bool IsIrcOp => IrcUser.IsIrcOp;

        /// <summary>
        /// Gets or sets the op flag of the user (+o)
        /// </summary>
        /// <remarks>
        /// only used with channel sync
        /// </remarks>
        public bool IsOp { get; set; }

        /// <summary>
        /// Gets or sets the voice flag of the user (+v)
        /// </summary>
        /// <remarks>
        /// only used with channel sync
        /// </remarks>
        public bool IsVoice { get; set; }

        /// <summary>
        /// Gets the away status of the user
        /// </summary>
        public bool IsAway => IrcUser.IsAway;

        /// <summary>
        /// Gets the underlaying IrcUser object
        /// </summary>
        public IrcUser IrcUser { get; }

        /// <summary>
        /// Gets the nickname of the user
        /// </summary>
        public string Nick => IrcUser.Nick;

        /// <summary>
        /// Gets the identity (username) of the user, which is used by some IRC networks for authentication.
        /// </summary>
        public string Ident => IrcUser.Ident;

        /// <summary>
        /// Gets the hostname of the user,
        /// </summary>
        public string Host => IrcUser.Host;

        /// <summary>
        /// Gets the supposed real name of the user.
        /// </summary>
        public string RealName => IrcUser.RealName;

        /// <summary>
        /// Gets the server the user is connected to.
        /// </summary>
        /// <value> </value>
        public string Server => IrcUser.Server;

        /// <summary>
        /// Gets or sets the count of hops between you and the user's server
        /// </summary>
        public int HopCount => IrcUser.HopCount;

        /// <summary>
        /// Gets the list of channels the user has joined
        /// </summary>
        public string[] JoinedChannels => IrcUser.JoinedChannels;
    }
}