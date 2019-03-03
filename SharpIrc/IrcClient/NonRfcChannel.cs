/*
 * SharpIRC- IRC library for .NET/C# <https://github.com/FreeApophis/sharpIRC>
 */

using System;
using System.Collections;

namespace SharpIrc.IrcClient
{
    /// <summary>
    ///
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    public class NonRfcChannel : Channel
    {
        private readonly Hashtable _halfops = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
        private readonly Hashtable _admins = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
        private readonly Hashtable _owners = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"> </param>
        internal NonRfcChannel(string name)
            : base(name)
        {
        }


        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public Hashtable Halfops => (Hashtable)_halfops.Clone();

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeHalfops => _halfops;

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public Hashtable Admins => (Hashtable)_admins.Clone();

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeAdmins => _admins;

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public Hashtable Owners => (Hashtable)_owners.Clone();

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeOwners => _owners;
    }
}