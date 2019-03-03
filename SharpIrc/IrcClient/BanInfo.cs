/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;

namespace SharpIrc.IrcClient
{
    [Serializable]
    public class BanInfo
    {
        private BanInfo()
        {
        }

        public string Channel { get; private set; }

        public string Mask { get; private set; }

        public static BanInfo Parse(IrcMessageData data)
        {
            return new BanInfo { Channel = data.RawMessageArray[3], Mask = data.RawMessageArray[4] };
        }
    }
}