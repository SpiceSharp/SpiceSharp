using System;
using System.Collections.Generic;
using System.Reflection;

namespace SpiceSharp.General
{
    /// <summary>
    /// Holds information about a specified member of the class and its attributes.
    /// </summary>
    public class CachedMemberInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedMemberInfo"/> class.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="attributes"></param>
        public CachedMemberInfo(MemberInfo member, IEnumerable<Attribute> attributes)
        {
            Member = member;
            Attributes = new List<Attribute>(attributes);
        }

        /// <summary>
        /// Gets the reference to the member.
        /// </summary>
        public MemberInfo Member { get; private set; }

        /// <summary>
        /// Gets the cached list of attributes for the member.
        /// </summary>
        public List<Attribute> Attributes { get; private set; }
    }
}
