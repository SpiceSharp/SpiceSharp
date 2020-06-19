
/* Unmerged change from project 'SpiceSharp (net45)'
Before:
using System;
using System.Collections.Generic;
using SpiceSharp.General;
using SpiceSharp.Diagnostics;
After:
using SpiceSharp.Diagnostics;
using SpiceSharp.General;
using System;
using System.Collections.Diagnostics;
*/

/* Unmerged change from project 'SpiceSharp (netstandard2.0)'
Before:
using System;
using System.Collections.Generic;
using SpiceSharp.General;
using SpiceSharp.Diagnostics;
After:
using SpiceSharp.Diagnostics;
using SpiceSharp.General;
using System;
using System.Collections.Diagnostics;
*/

/* Unmerged change from project 'SpiceSharp (netcoreapp2.0)'
Before:
using System;
using System.Collections.Generic;
using SpiceSharp.General;
using SpiceSharp.Diagnostics;
After:
using SpiceSharp.Diagnostics;
using SpiceSharp.General;
using System;
using System.Collections.Diagnostics;
*/
using SpiceSharp.General;
using System;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// A dictionary that can store objects, and indexes them by their type.
    /// </summary>
    /// <typeparam name="T">The common base type of all objects.</typeparam>
    public interface ITypeDictionary<T> : IEnumerable<KeyValuePair<Type, T>>, ICloneable
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
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if <paramref name="key"/> was not found.</exception>
        /// <exception cref="AmbiguousTypeException">If there are multiple values of type <paramref name="key"/>.</exception>
        T this[Type key] { get; }

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
        IEnumerable<T> Values { get; }

        /// <summary>
        /// Adds the specified value to the dictionary.
        /// </summary>
        /// <typeparam name="V">The value type.</typeparam>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if a value of type <typeparamref name="V"/> was already added.</exception>
        void Add<V>(V value) where V : T;

        /// <summary>
        /// Removes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the value was removed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
        bool Remove(T value);

        /// <summary>
        /// Clears all items in the dictionary.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets the strongly typed value from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>The result.</returns>
        /// <exception cref="TypeNotFoundException">Thrown if a value of type <typeparamref name="TResult"/> could not be found.</exception>
        /// <exception cref="AmbiguousTypeException">If there are multiple values of type <typeparamref name="TResult"/>.</exception>
        TResult GetValue<TResult>() where TResult : T;

        /// <summary>
        /// Gets all strongly typed values from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>The results.</returns>
        IEnumerable<TResult> GetAllValues<TResult>() where TResult : T;

        /// <summary>
        /// Tries to get a strongly typed value from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified key contains the type; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="AmbiguousTypeException">If the value could not be uniquely determined.</exception>
        bool TryGetValue<TResult>(out TResult value) where TResult : T;

        /// <summary>
        /// Tries to get a value from the dictionary.
        /// </summary>
        /// <param name="key">The key type.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the value was resolved; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="AmbiguousTypeException">If there are multiple values of type <paramref name="key"/>.</exception>
        bool TryGetValue(Type key, out T value);

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
        bool ContainsValue(T value);
    }
}
