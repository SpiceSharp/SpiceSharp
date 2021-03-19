using System;
using System.Collections.Generic;

namespace SpiceSharp.General
{
    /// <summary>
    /// A dictionary that can store instances, and indexes them by a type. This dictionary
    /// distinguishes itself by supporting for example inheritance on the type.
    /// </summary>
    /// <typeparam name="V">The base type for values in the type dictionary.</typeparam>
    public interface ITypeDictionary<V>
    {
        /// <summary>
        /// Gets all the keys in the dictionary.
        /// </summary>
        /// <value>
        /// The keys in the dictionary.
        /// </value>
        IEnumerable<Type> Keys { get; }

        /// <summary>
        /// Gets all the values in the dictionary.
        /// </summary>
        /// <value>
        /// The values in the dictionary.
        /// </value>
        IEnumerable<V> Values { get; }

        /// <summary>
        /// Gets the number of items in the dictionary.
        /// </summary>
        /// <value>
        /// The number of items in the dictionary.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Gets a value from the dictionary by its type.
        /// </summary>
        /// <param name="key">The type.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        /// <exception cref="AmbiguousTypeException">Thrown if <paramref name="key"/> does not point to a single instance.</exception>
        V this[Type key] { get; }

        /// <summary>
        /// Adds a value to the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        void Add(Type key, V value);

        /// <summary>
        /// Clears all values from the dictionary.
        /// </summary>
        void Clear();

        /// <summary>
        /// Tries to get a value from the dictionary indexed by the specified type.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the value was found; otherwise, <c>false</c>.
        /// </returns>
        bool TryGetValue(Type key, out V value);

        /// <summary>
        /// Determines whether the dictionary contains the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the dictionary contains the value; otherwise, <c>false</c>.
        /// </returns>
        bool Contains(V value);

        /// <summary>
        /// Removes a value from the dictionary, but only if the value was added through
        /// the same key originally to avoid inconsistencies.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the value was remove; otherwise, <c>false</c>.
        /// </returns>
        bool Remove(Type key, V value);

        /// <summary>
        /// Determines whether the dictionary contains a value with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///     <c>true</c> if the dictionary contains a value with the specified key; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsKey(Type key);

        /// <summary>
        /// Gets all values from the dictionary that the specified key can point to.
        /// </summary>
        /// <returns>The values.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        IEnumerable<V> GetAllValues(Type key);

        /// <summary>
        /// Gets the the number of values that this type points to (direct or indirect).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The number of values.</returns>
        int GetValueCount(Type key);
    }
}
