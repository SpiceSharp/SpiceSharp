using SpiceSharp.General;
using System;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// A dictionary that can store objects, and indexes them by their type.
    /// </summary>
    /// <typeparam name="K">The base type for keys in the type dictionary.</typeparam>
    /// <typeparam name="V">The base type for values in the type dictionary.</typeparam>
    public interface ITypeDictionary<K, V> : IEnumerable<KeyValuePair<Type, V>>, ICloneable
    {
        /// <summary>
        /// Gets the number of elements contained in the <see cref="ITypeDictionary{K,V}"/>.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Gets the value with the specified key.
        /// </summary>
        /// <value>
        /// The key type.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if <paramref name="key"/> was not found.</exception>
        /// <exception cref="AmbiguousTypeException">If there are multiple values of type <paramref name="key"/>.</exception>
        V this[Type key] { get; }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>
        /// The keys.
        /// </value>
        IEnumerable<Type> Keys { get; }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        IEnumerable<V> Values { get; }

        /// <summary>
        /// Adds the specified value to the dictionary.
        /// </summary>
        /// <typeparam name="Key">The key type.</typeparam>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if a value of type <typeparamref name="V"/> was already added.</exception>
        void Add<Key>(V value) where Key : K;

        /// <summary>
        /// Removes the value with the specified key.
        /// </summary>
        /// <typeparam name="Key">The key type.</typeparam>
        /// <returns>
        ///     <c>true</c> if a value was removed; otherwise, <c>false</c>.
        /// </returns>
        bool Remove<Key>() where Key : K;

        /// <summary>
        /// Clears all items in the dictionary.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the strongly typed value from the dictionary.
        /// </summary>
        /// <typeparam name="Key">The key.</typeparam>
        /// <returns>The result.</returns>
        /// <exception cref="TypeNotFoundException">Thrown if a value of type <typeparamref name="Key"/> could not be found.</exception>
        /// <exception cref="AmbiguousTypeException">Thrown if there are multiple values of type <typeparamref name="Key"/>.</exception>
        V GetValue<Key>() where Key : K;

        /// <summary>
        /// Gets all strongly typed values from the dictionary.
        /// </summary>
        /// <typeparam name="Key">The key type.</typeparam>
        /// <returns>The results.</returns>
        IEnumerable<V> GetAllValues<Key>() where Key : K;

        /// <summary>
        /// Tries to get a strongly typed value from the dictionary.
        /// </summary>
        /// <typeparam name="Key">The key type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if a value was found for the specified type; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="AmbiguousTypeException">If the value could not be uniquely determined.</exception>
        bool TryGetValue<Key>(out V value) where Key : K;

        /// <summary>
        /// Tries to get a value from the dictionary.
        /// </summary>
        /// <param name="key">The key type.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the value was resolved; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="AmbiguousTypeException">If there are multiple values of type <paramref name="key"/>.</exception>
        bool TryGetValue(Type key, out V value);

        /// <summary>
        /// Determines whether the dictionary contains one or more values of the specified type.
        /// </summary>
        /// <typeparam name="Key">The key type.</typeparam>
        /// <returns>
        ///     <c>true</c> if the dictionary contains one or more values of the specified type; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsKey<Key>() where Key : K;

        /// <summary>
        /// Determines whether the dictionary contains one or more values of the specified type.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the dictionary contains one ore more values of the specified type; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        bool ContainsKey(Type key);

        /// <summary>
        /// Determines whether the dictionary contains the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the dictionary contains the specified value; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
        bool ContainsValue(V value);
    }
}
