/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

namespace SharpIrc.IrcClient
{
    /// <summary>
    ///
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    public class NonRfcChannelUser : ChannelUser
    {

        /// <summary>
        ///
        /// </summary>
        /// <param name="channel"> </param>
        /// <param name="ircuser"> </param>
        internal NonRfcChannelUser(string channel, IrcUser ircuser)
            : base(channel, ircuser)
        {
        }


        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public bool IsHalfop { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public bool IsAdmin { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public bool IsOwner { get; set; }
    }
}