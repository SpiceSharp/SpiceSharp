using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharp.Reflection
{
    /// <summary>
    /// A cached map of type members. Can be used to map parameter names to
    /// <see cref="MemberDescription"/> instances.
    /// </summary>
    public class ParameterMap
    {
        private static readonly ConcurrentDictionary<Type, Func<IEqualityComparer<string>, IMemberMap>> _factories
            = new ConcurrentDictionary<Type, Func<IEqualityComparer<string>, IMemberMap>>();

        private readonly Dictionary<Type, IMemberMap> _memberMaps = new Dictionary<Type, IMemberMap>();

        private static IMemberMap CreateMap(Type type, IEqualityComparer<string> comparer)
        {
            type.ThrowIfNull(nameof(type));
            var factory = _factories.GetOrAdd(type, type =>
            {
                // This is quite expensive, so we cache the creation of maps into a function
                var ntype = typeof(TypedMemberMap<>).MakeGenericType(type);
                var param = Expression.Parameter(typeof(IEqualityComparer<string>), "comparer");
                return Expression.Lambda<Func<IEqualityComparer<string>, IMemberMap>>(
                    Expression.New(ntype.GetTypeInfo().GetConstructor(new[] { typeof(IEqualityComparer<string>) }), param),
                    param
                    ).Compile();
            });
            return factory(comparer);
        }

        /// <inheritdoc/>
        public IEnumerable<MemberDescription> Members
        {
            get
            {
                foreach (var map in _memberMaps.Values.Distinct())
                {
                    foreach (var member in map.Members.Distinct())
                        yield return member;
                }
            }
        }

        /// <inheritdoc/>
        public IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterMap"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="comparer">The comparer used.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="type"/> is <c>null</c>.</exception>
        public ParameterMap(Type type, IEqualityComparer<string> comparer)
        {
            type.ThrowIfNull(nameof(type));

            foreach (var member in type.GetTypeInfo().GetMembers(BindingFlags.Instance | BindingFlags.Public))
            {
                // Ignore unsupported members
                var mt = member.MemberType;
                if (mt != MemberTypes.Property &&
                    mt != MemberTypes.Field &&
                    mt != MemberTypes.Method)
                    continue;
                var desc = new MemberDescription(member);
                if (desc.Names.Count == 0)
                    continue;
                var descType = desc.ParameterType;
                if (descType == typeof(void))
                    descType = desc.PropertyType;
                if (descType == typeof(void))
                    continue;

                // Add a map
                if (!_memberMaps.TryGetValue(descType, out var members))
                {
                    // Create a new instance
                    members = CreateMap(descType, comparer);
                    _memberMaps.Add(descType, members);
                }
                members.Add(desc);
            }
        }

        /// <summary>
        /// Remaps the parameter map using a new comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public void Remap(IEqualityComparer<string> comparer)
        {
            var d = new Dictionary<Type, IMemberMap>();
            foreach (var map in _memberMaps)
            {
                var nmap = CreateMap(map.Key, comparer);
                foreach (var desc in map.Value.Members)
                    nmap.Add(desc);
                d.Add(map.Key, nmap);
            }
            _memberMaps.Clear();
            foreach (var pair in d)
                _memberMaps.Add(pair.Key, pair.Value);
        }

        /// <summary>
        /// Tries to set the value of a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter value type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the parameter was set succesfully; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public bool TrySet<P>(object source, string name, P value)
        {
            if (_memberMaps.TryGetValue(typeof(P), out var direct))
            {
                if (direct is IParameterImporter<P> importer)
                {
                    if (importer.TrySet(source, name, value))
                        return true;
                }
            }
            foreach (var map in _memberMaps.Values)
            {
                if (map is IParameterImporter<P> importer)
                {
                    if (importer.TrySet(source, name, value))
                        return true;
                }
            }

            // Fall back to a given parameter
            if (_memberMaps.TryGetValue(typeof(GivenParameter<P>), out var gpm) &&
                gpm is IParameterImporter<GivenParameter<P>> gimporter)
            {
                if (gimporter.TrySet(source, name, value))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a setter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter value type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="name">The parameter name.</param>
        /// <returns>
        /// The setter; or <c>null</c> if the parameter was not found.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Action<P> CreateSetter<P>(object source, string name)
        {
            Action<P> result = null;
            if (_memberMaps.TryGetValue(typeof(P), out var direct))
            {
                if (direct is IParameterImporter<P> importer)
                    result = importer.CreateSetter(source, name);
            }
            if (result != null)
            {
                foreach (var map in _memberMaps)
                {
                    if (map.Value is IParameterImporter<P> importer)
                    {
                        result = importer.CreateSetter(source, name);
                        if (result != null)
                            return result;
                    }
                }
            }

            // Try once more with GivenParameter
            if (result == null)
            {
                if (_memberMaps.TryGetValue(typeof(GivenParameter<P>), out var gpm) &&
                    gpm is IParameterImporter<GivenParameter<P>> importer)
                {
                    var gresult = importer.CreateSetter(source, name);
                    if (gresult != null)
                        result = v => gresult(v);
                }
            }
            return result;
        }

        /// <summary>
        /// Tries to get the value of a property with the specified name.
        /// </summary>
        /// <typeparam name="P">The property value type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the property was found; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public bool TryGet<P>(object source, string name, out P value)
        {
            bool isValid;
            if (_memberMaps.TryGetValue(typeof(P), out var direct))
            {
                if (direct is IPropertyExporter<P> exporter)
                {
                    value = exporter.TryGet(source, name, out isValid);
                    if (isValid)
                        return true;
                }
            }
            foreach (var map in _memberMaps.Values)
            {
                if (map is IPropertyExporter<P> exporter)
                {
                    value = exporter.TryGet(source, name, out isValid);
                    if (isValid)
                        return true;
                }
            }

            // Try again with GivenParameter
            if (_memberMaps.TryGetValue(typeof(GivenParameter<P>), out var gpm) &&
                gpm is IPropertyExporter<GivenParameter<P>> gexporter)
            {
                value = gexporter.TryGet(source, name, out isValid).Value;
                if (isValid)
                    return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Creates a getter for a property with the specified name.
        /// </summary>
        /// <typeparam name="P">The property value type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="name">The property name.</param>
        /// <returns>
        /// The getter; or <c>null</c> if the property was not found.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Func<P> CreateGetter<P>(object source, string name)
        {
            Func<P> result = null;
            if (_memberMaps.TryGetValue(typeof(P), out var direct))
            {
                if (direct is IPropertyExporter<P> exporter)
                    result = exporter.CreateGetter(source, name);
            }
            if (result != null)
            {
                foreach (var map in _memberMaps.Values)
                {
                    if (map is IPropertyExporter<P> exporter)
                    {
                        result = exporter.CreateGetter(source, name);
                        if (result != null)
                            return result;
                    }
                }
            }

            // Try again with GivenParameter
            if (result == null)
            {
                if (_memberMaps.TryGetValue(typeof(GivenParameter<P>), out var gpm) &&
                    gpm is IPropertyExporter<GivenParameter<P>> exporter)
                {
                    var gresult = exporter.CreateGetter(source, name);
                    if (gresult != null)
                        result = () => gresult().Value;
                }
            }

            return result;
        }
    }
}
