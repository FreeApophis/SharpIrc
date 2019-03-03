/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using SharpIrc.IrcClient.EventArgs;

namespace SharpIrc.IrcFeatures
{
    /// <summary>
    /// Delegates to handle individual ctcp commands
    /// </summary>
    public delegate void CtcpDelegate(CtcpEventArgs eventArgs);
}