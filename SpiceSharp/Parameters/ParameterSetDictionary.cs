using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// A dictionary of <see cref="ParameterSet" />. Only one instance of each type is allowed.
    /// </summary>
    /// <seealso cref="TypeDictionary{P}" />
    public class ParameterSetDictionary : IParameterSetDictionary
    {
        private readonly ITypeDictionary<IParameterSet> _dictionary;

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _dictionary.Count;

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>
        /// The keys.
        /// </value>
        public IEnumerable<Type> Keys => _dictionary.Keys;

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public IEnumerable<IParameterSet> Values => _dictionary.Values;

        /// <summary>
        /// Gets the <see cref="IParameterSet"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="IParameterSet"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public IParameterSet this[Type key] => _dictionary[key];

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSetDictionary"/> class.
        /// </summary>
        public ParameterSetDictionary()
        {
            _dictionary = new TypeDictionary<IParameterSet>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSetDictionary"/> class.
        /// </summary>
        /// <param name="parameters">The type dictionary for the parameters.</param>
        public ParameterSetDictionary(ITypeDictionary<IParameterSet> parameters)
        {
            _dictionary = parameters.ThrowIfNull(nameof(parameters));
        }

        /// <summary>
        /// Clones the dictionary.
        /// </summary>
        /// <returns></returns>
        public virtual IParameterSetDictionary Clone()
        {
            var clone = (ParameterSetDictionary)Activator.CreateInstance(GetType(), _dictionary.Clone());
            return clone;
        }

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns></returns>
        ICloneable ICloneable.Clone() => Clone();

        /// <summary>
        /// Copy all parameter sets.
        /// </summary>
        /// <param name="source">The source object.</param>
        public virtual void CopyFrom(IParameterSetDictionary source)
        {
            var d = source as ParameterSetDictionary ?? throw new UnexpectedTypeMismatch(GetType(), source?.GetType());
            foreach (var ps in d.Values)
            {
                // If the parameter set doesn't exist, then we will simply clone it, else copy them
                if (!TryGetValue(ps.GetType(), out var myps))
                    Add((IParameterSet)ps.Clone());
                else
                    Reflection.CopyPropertiesAndFields(ps, myps);
            }
        }

        /// <summary>
        /// Copy all properties from another object to this one.
        /// </summary>
        /// <param name="source">The source object.</param>
        void ICloneable.CopyFrom(ICloneable source) => CopyFrom((ParameterSetDictionary)source);

        /// <summary>
        /// Call a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <returns>The current instance for chaining.</returns>
        public IParameterSetDictionary SetParameter(string name)
        {
            foreach (var ps in _dictionary.Values)
            {
                if (ps.TrySetParameter(name))
                    return this;
            }
            throw new ParameterNotFoundException(name, this);
        }

        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>The current instance for chaining.</returns>
        public IParameterSetDictionary SetParameter<P>(string name, P value)
        {
            foreach (var ps in _dictionary.Values)
            {
                if (ps.TrySetParameter(name, value))
                    return this;
            }
            throw new ParameterNotFoundException(name, this);
        }

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        public void CalculateDefaults()
        {
            foreach (var ps in _dictionary.Values)
                ps.CalculateDefaults();
        }

        /// <summary>
        /// Call a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        void IImportParameterSet.SetParameter(string name)
        {
            foreach (var ps in _dictionary.Values)
            {
                if (ps.TrySetParameter(name))
                    return;
            }
            throw new ParameterNotFoundException(name, this);
        }

        /// <summary>
        /// Tries calling a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// <c>true</c> if the method was called; otherwise <c>false</c>.
        /// </returns>
        public bool TrySetParameter(string name)
        {
            foreach (var ps in _dictionary.Values)
            {
                if (ps.TrySetParameter(name))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        void IImportParameterSet.SetParameter<P>(string name, P value)
        {
            foreach (var ps in _dictionary.Values)
            {
                if (ps.TrySetParameter(name, value))
                    return;
            }
            throw new ParameterNotFoundException(name, this);
        }

        /// <summary>
        /// Tries to set the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the parameter was set; otherwise <c>false</c>.
        /// </returns>
        public bool TrySetParameter<P>(string name, P value)
        {
            foreach (var ps in _dictionary.Values)
            {
                if (ps.TrySetParameter(name, value))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name.</param>
        /// <returns>The value.</returns>
        public P GetProperty<P>(string name)
        {
            foreach (var ps in _dictionary.Values)
            {
                if (ps.TryGetProperty(name, out P result))
                    return result;
            }
            throw new ParameterNotFoundException(name, this);
        }

        /// <summary>
        /// Tries to get the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the parameter was found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetProperty<P>(string name, out P value)
        {
            foreach (var ps in _dictionary.Values)
            {
                if (ps.TryGetProperty(name, out value))
                    return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Creates a getter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A getter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
        public Func<P> CreatePropertyGetter<P>(string name)
        {
            foreach (var ps in _dictionary.Values)
            {
                var result = ps.CreatePropertyGetter<P>(name);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Creates a setter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A setter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
        public Action<P> CreateParameterSetter<P>(string name)
        {
            foreach (var ps in _dictionary.Values)
            {
                var result = ps.CreateParameterSetter<P>(name);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="value">The value.</param>
        public void Add<V>(V value) where V : IParameterSet
            => _dictionary.Add(value);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
            => _dictionary.Clear();

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns></returns>
        public TResult GetValue<TResult>() where TResult : IParameterSet
            => _dictionary.GetValue<TResult>();

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue<TResult>(out TResult value) where TResult : IParameterSet
            => _dictionary.TryGetValue(out value);

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(Type key, out IParameterSet value)
            => _dictionary.TryGetValue(key, out value);

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(Type key)
            => _dictionary.ContainsKey(key);

        /// <summary>
        /// Determines whether the specified value contains value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value contains value; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsValue(IParameterSet value)
            => _dictionary.ContainsValue(value);

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<Type, IParameterSet>> GetEnumerator()
            => _dictionary.GetEnumerator();

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable)_dictionary).GetEnumerator();
    }
}
