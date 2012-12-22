/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 *
 * Copyright (c) 2008 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
 * Copyright (c) 2008-2013 Thomas Bruderer <apophis@apophis.ch> <http://www.apophis.ch>
 * 
 * Full LGPL License: <http://www.gnu.org/licenses/lgpl.txt>
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */


using System;

namespace apophis.SharpIRC.IrcClient
{
    [Serializable]
    public class WhoInfo
    {
        private WhoInfo()
        {
        }

        public string Channel { get; private set; }

        public string Ident { get; private set; }

        public string Host { get; private set; }

        public string Server { get; private set; }

        public string Nick { get; private set; }

        public int HopCount { get; private set; }

        public string Realname { get; private set; }

        public bool IsAway { get; private set; }

        public bool IsOp { get; private set; }

        public bool IsVoice { get; private set; }

        public bool IsIrcOp { get; private set; }

        public bool IsRegistered { get; private set; }

        public static WhoInfo Parse(IrcMessageData data)
        {
            var whoInfo = new WhoInfo
            {
                Channel = data.RawMessageArray[3],
                Ident = data.RawMessageArray[4],
                Host = data.RawMessageArray[5],
                Server = data.RawMessageArray[6],
                Nick = data.RawMessageArray[7],
                Realname = String.Join(" ", data.MessageArray, 1, data.MessageArray.Length - 1)
            };

            // skip hop count

            int hopcount = 0;
            string hopcountStr = data.MessageArray[0];
            try
            {
                hopcount = int.Parse(hopcountStr);
            }
            catch (FormatException ex)
            {
            }

            string usermode = data.RawMessageArray[8];
            bool op = false;
            bool voice = false;
            bool ircop = false;
            bool away = false;
            bool registered = false;

            foreach (char c in usermode)
            {
                switch (c)
                {
                    case 'H':
                        away = false;
                        break;
                    case 'G':
                        away = true;
                        break;
                    case '@':
                        op = true;
                        break;
                    case '+':
                        voice = true;
                        break;
                    case '*':
                        ircop = true;
                        break;
                    case 'r':
                        registered = true;
                        break;
                }
            }
            whoInfo.IsAway = away;
            whoInfo.IsOp = op;
            whoInfo.IsVoice = voice;
            whoInfo.IsIrcOp = ircop;
            whoInfo.HopCount = hopcount;
            whoInfo.IsRegistered = registered;

            return whoInfo;
        }
    }
}