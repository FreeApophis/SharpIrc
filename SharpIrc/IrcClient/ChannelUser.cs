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
        private readonly IrcUser ircUser;

        /// <summary>
        ///
        /// </summary>
        /// <param name="channel"> </param>
        /// <param name="ircuser"> </param>
        internal ChannelUser(string channel, IrcUser ircuser)
        {
            Channel = channel;
            ircUser = ircuser;
        }

        /// <summary>
        /// Gets the channel name
        /// </summary>
        public string Channel { get; private set; }

        /// <summary>
        /// Gets the server operator status of the user
        /// </summary>
        public bool IsIrcOp
        {
            get { return ircUser.IsIrcOp; }
        }

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
        public bool IsAway
        {
            get { return ircUser.IsAway; }
        }

        /// <summary>
        /// Gets the underlaying IrcUser object
        /// </summary>
        public IrcUser IrcUser
        {
            get { return ircUser; }
        }

        /// <summary>
        /// Gets the nickname of the user
        /// </summary>
        public string Nick
        {
            get { return ircUser.Nick; }
        }

        /// <summary>
        /// Gets the identity (username) of the user, which is used by some IRC networks for authentication.
        /// </summary>
        public string Ident
        {
            get { return ircUser.Ident; }
        }

        /// <summary>
        /// Gets the hostname of the user,
        /// </summary>
        public string Host
        {
            get { return ircUser.Host; }
        }

        /// <summary>
        /// Gets the supposed real name of the user.
        /// </summary>
        public string Realname
        {
            get { return ircUser.Realname; }
        }

        /// <summary>
        /// Gets the server the user is connected to.
        /// </summary>
        /// <value> </value>
        public string Server
        {
            get { return ircUser.Server; }
        }

        /// <summary>
        /// Gets or sets the count of hops between you and the user's server
        /// </summary>
        public int HopCount
        {
            get { return ircUser.HopCount; }
        }

        /// <summary>
        /// Gets the list of channels the user has joined
        /// </summary>
        public string[] JoinedChannels
        {
            get { return ircUser.JoinedChannels; }
        }
    }
}