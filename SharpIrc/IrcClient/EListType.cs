using System;

namespace SharpIrc.IrcClient
{
    /// <summary>
    /// M = mask search,
    /// N = !mask search
    /// U = usercount search (< >)
    /// C = creation time search (C< C>)
    /// T = topic search (T< T>)
    /// </summary>
    [Flags]
    public enum EListType
    {

        M,
        N,
        U,
        C,
        T
    }
}