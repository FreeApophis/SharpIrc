/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

namespace SharpIrc.IrcFeatures
{
    public enum DccSpeed
    {
        /// <summary>
        /// slow, ack every packet
        /// </summary>
        Rfc,
        /// <summary>
        /// hack, ignore acks, just send at max speed
        /// </summary>
        RfcSendAhead,
        /// <summary>
        /// fast, Turbo extension, no acks (Virc)
        /// </summary>
        Turbo
    }
}