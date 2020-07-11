using System;
using System.Collections.Generic;

namespace SpiceSharp.Reflection
{
    /// <summary>
    /// A class that contains the members of a certain type.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="IParameterImporter{T}" />
    /// <seealso cref="IPropertyExporter{T}" />
    public class TypedMemberMap<T> : IParameterImporter<T>, IPropertyExporter<T>
    {
        private readonly Dictionary<string, MemberDescription> _members;

        /// <inheritdoc/>
        public IEqualityComparer<string> Comparer => _members.Comparer;

        /// <inheritdoc/>
        public IEnumerable<MemberDescription> Members => _members.Values;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedMemberMap{T}" /> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public TypedMemberMap(IEqualityComparer<string> comparer)
        {
            _members = new Dictionary<string, MemberDescription>(comparer);
        }

        /// <inheritdoc/>
        public void Add(MemberDescription member)
        {
            if (member.ParameterType == typeof(T) || member.PropertyType == typeof(T))
            {
                foreach (var name in member.Names)
                {
                    if (!_members.TryGetValue(name, out var existing))
                        _members.Add(name, member);
                    else if (!ReferenceEquals(member, existing))
                        throw new ArgumentException(Properties.Resources.Reflection_SameName.FormatString(name, member.Member.Name, existing.Member.Name));
                }
            }
        }

        /// <inheritdoc/>
        public bool TrySet(object source, string name, T value)
        {
            if (_members.TryGetValue(name, out var result))
                return result.TrySet(source, value);
            return false;
        }

        /// <inheritdoc/>
        public T TryGet(object source, string name, out bool isValid)
        {
            if (_members.TryGetValue(name, out var result))
            {
                isValid = result.TryGet(source, out T value);
                return value;
            }
            isValid = false;
            return default;
        }

        /// <inheritdoc/>
        public Func<T> CreateGetter(object source, string name)
        {
            if (_members.TryGetValue(name, out var result))
                return result.CreateGetter<T>(source);
            return null;
        }

        /// <inheritdoc/>
        public Action<T> CreateSetter(object source, string name)
        {
            if (_members.TryGetValue(name, out var result))
                return result.CreateSetter<T>(source);
            return null;
        }
    }
}
