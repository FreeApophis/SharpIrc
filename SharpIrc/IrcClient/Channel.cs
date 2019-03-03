/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using System.Collections;
using System.Collections.Specialized;

namespace SharpIrc.IrcClient
{
    /// <summary>
    ///
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    public class Channel
    {
        private DateTime activeSyncStop;

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"> </param>
        internal Channel(string name)
        {
            UnsafeOps = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
            UnsafeUsers = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
            UnsafeVoices = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));

            Mode = String.Empty;
            Topic = String.Empty;
            Bans = new StringCollection();
            Key = String.Empty;
            Name = name;
            ActiveSyncStart = DateTime.Now;
        }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public string Name { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public string Key { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public Hashtable Users
        {
            get { return (Hashtable)UnsafeUsers.Clone(); }
        }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeUsers { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public Hashtable Ops
        {
            get { return (Hashtable)UnsafeOps.Clone(); }
        }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeOps { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public Hashtable Voices
        {
            get { return (Hashtable)UnsafeVoices.Clone(); }
        }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeVoices { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public StringCollection Bans { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public string Topic { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public int UserLimit { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public string Mode { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public DateTime ActiveSyncStart { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public DateTime ActiveSyncStop
        {
            get { return activeSyncStop; }
            set
            {
                activeSyncStop = value;
                ActiveSyncTime = activeSyncStop.Subtract(ActiveSyncStart);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public TimeSpan ActiveSyncTime { get; private set; }

        public bool IsSycned { get; set; }
    }
}