/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;

namespace SharpIrc.IrcClient
{
    [Flags]
    public enum ChannelModeType
    {
        WithUserhostParameter,
        WithAlwaysParamter,
        WithSetOnlyParameter,
        WithoutParameter
    }
}