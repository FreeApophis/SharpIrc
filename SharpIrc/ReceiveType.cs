/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

namespace SharpIrc
{
    /// <summary>
    ///
    /// </summary>
    public enum ReceiveType
    {
        Info,
        Login,
        Motd,
        List,
        Join,
        Kick,
        Part,
        Invite,
        Quit,
        Who,
        WhoIs,
        WhoWas,
        Name,
        Topic,
        BanList,
        NickChange,
        TopicChange,
        UserMode,
        UserModeChange,
        ChannelMode,
        ChannelModeChange,
        ChannelMessage,
        ChannelAction,
        ChannelNotice,
        QueryMessage,
        QueryAction,
        QueryNotice,
        CtcpReply,
        CtcpRequest,
        Error,
        ErrorMessage,
        Other,
        Unknown
    }
}