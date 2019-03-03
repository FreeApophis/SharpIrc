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
        private readonly Hashtable halfops = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
        private readonly Hashtable admins = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));
        private readonly Hashtable owners = Hashtable.Synchronized(new Hashtable(StringComparer.InvariantCultureIgnoreCase));

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
        public Hashtable Halfops
        {
            get
            {
                return (Hashtable)halfops.Clone();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeHalfops
        {
            get
            {
                return halfops;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public Hashtable Admins
        {
            get
            {
                return (Hashtable)admins.Clone();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeAdmins
        {
            get
            {
                return admins;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        public Hashtable Owners
        {
            get
            {
                return (Hashtable)owners.Clone();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeOwners
        {
            get
            {
                return owners;
            }
        }
    }
}