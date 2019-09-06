using System;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// A dictionary that can store objects, and indexes them by their type.
    /// </summary>
    /// <typeparam name="T">The common base type of all objects.</typeparam>
    public interface ITypeDictionary<T> : IEnumerable<KeyValuePair<Type, T>>
    {
        /// <summary>
        /// Gets the number of elements contained in the <see cref="ITypeDictionary{T}"/>.
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
        /// <returns></returns>
        T this[Type key] { get; }

        /// <summary>
        /// Adds the specified value to the dictionary.
        /// </summary>
        /// <param name="value">The value.</param>
        void Add(T value);

        /// <summary>
        /// Clears all items in the dictionary.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the strongly typed value from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>The result.</returns>
        TResult GetValue<TResult>() where TResult : T;

        /// <summary>
        /// Tries to get a strongly typed value from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        bool TryGetValue<TResult>(out TResult value) where TResult : T;

        /// <summary>
        /// Tries to get a value from the dictionary.
        /// </summary>
        /// <param name="key">The key type.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        bool TryGetValue(Type key, out T value);

        /// <summary>
        /// Determines whether the dictionary contains a value of the specified type.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsKey(Type key);
    }
}
