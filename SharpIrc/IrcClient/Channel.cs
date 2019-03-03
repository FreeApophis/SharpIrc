/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using System.Collections;
using System.Collections.Specialized;
using static System.String;

namespace SharpIrc.IrcClient
{
    /// <summary>
    ///
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    public class Channel
    {
        private DateTime _activeSyncStop;

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"> </param>
        internal Channel(string name)
        {
            UnsafeOps = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
            UnsafeUsers = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
            UnsafeVoices = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));

            Mode = Empty;
            Topic = Empty;
            Bans = new StringCollection();
            Key = Empty;
            Name = name;
            ActiveSyncStart = DateTime.Now;
        }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public string Name { get; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public string Key { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public Hashtable Users => (Hashtable)UnsafeUsers.Clone();

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeUsers { get; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public Hashtable Ops => (Hashtable)UnsafeOps.Clone();

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeOps { get; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public Hashtable Voices => (Hashtable)UnsafeVoices.Clone();

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeVoices { get; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public StringCollection Bans { get; }

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
        public DateTime ActiveSyncStart { get; }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public DateTime ActiveSyncStop
        {
            get => _activeSyncStop;
            set
            {
                _activeSyncStop = value;
                ActiveSyncTime = _activeSyncStop.Subtract(ActiveSyncStart);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public TimeSpan ActiveSyncTime { get; private set; }

        public bool IsSynced { get; set; }
    }
}