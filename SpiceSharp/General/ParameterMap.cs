using System;
using System.Collections.Generic;
using System.Reflection;

namespace SpiceSharp.General
{
    /// <summary>
    /// A cached map of type members. Can be used to quickly map parameter names to
    /// <see cref="MemberDescription"/> objects, or to enumerate all members in a class.
    /// </summary>
    public class ParameterMap
    {
        private Dictionary<string, MemberDescription> _members;
        private HashSet<MemberDescription> _values;

        /// <summary>
        /// Gets the principal parameter.
        /// </summary>
        /// <value>
        /// The principal parameter.
        /// </value>
        public MemberDescription Principal { get; }

        /// <summary>
        /// Enumerate all members in the map.
        /// </summary>
        /// <value>
        /// The members.
        /// </value>
        public IEnumerable<MemberDescription> Members => _values;

        /// <summary>
        /// Gets the comparer used for parameter names.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        public IEqualityComparer<string> Comparer => _members.Comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterMap"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="comparer">The comparer used.</param>
        public ParameterMap(Type type, IEqualityComparer<string> comparer)
        {
            _members = new Dictionary<string, MemberDescription>(comparer);
            _values = new HashSet<MemberDescription>();
            foreach (var member in type.GetTypeInfo().GetMembers(BindingFlags.Instance | BindingFlags.Public))
            {
                var mt = member.MemberType;
                if (mt != MemberTypes.Property &&
                    mt != MemberTypes.Field &&
                    mt != MemberTypes.Method)
                    continue;

                var desc = new MemberDescription(member);
                _values.Add(desc);

                // Add named parameters
                if (desc.Names.Length > 0)
                {
                    foreach (var name in desc.Names)
                    {
                        try
                        {
                            _members.Add(name, desc);
                        }
                        catch (ArgumentException)
                        {
                            throw new CircuitException("Duplicate parameter with the name '{0}'".FormatString(name));
                        }
                    }
                }
                if (desc.IsPrincipal)
                    Principal = desc;
            }
        }

        /// <summary>
        /// Remaps the parameter map using a new comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public void Remap(IEqualityComparer<string> comparer)
        {
            var nmap = new Dictionary<string, MemberDescription>(comparer);
            foreach (var type in _members)
            {   
                foreach (var desc in nmap.Values)
                {
                    foreach (var name in desc.Names)
                        nmap.Add(name, desc);
                }
            }
            _members.Clear();
            _members = nmap;
        }

        /// <summary>
        /// Gets the <see cref="MemberDescription"/> that matches a specified name.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <returns>The description of the parameter.</returns>
        public MemberDescription Get(string name)
        {
            if (_members.TryGetValue(name, out var result))
                return result;

            // Get the matching name
            return null;
        }
    }
}
