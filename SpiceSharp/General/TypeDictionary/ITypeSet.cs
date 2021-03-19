using System;
using System.Collections.Generic;

namespace SpiceSharp.General
{
    /// <summary>
    /// A set of instances that can also be found by their type.
    /// </summary>
    /// <typeparam name="V">The base value type.</typeparam>
    public interface ITypeSet<V> : ICollection<V>
    {
        /// <summary>
        /// Occurs if a type could not be found.
        /// </summary>
        event EventHandler<TypeNotFoundEventArgs<V>> TypeNotFound;

        /// <summary>
        /// Tries getting a value from the set with the specified type.
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the value was found; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="AmbiguousTypeException">Thrown if the type could not be resolved to a single instance.</exception>
        bool TryGetValue<TResult>(out TResult value) where TResult : V;

        /// <summary>
        /// Gets a value from the set with the specified type.
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <returns>The value.</returns>
        /// <exception cref="AmbiguousTypeException">Thrown if the type could not be resolved to a single instance.</exception>
        TResult GetValue<TResult>() where TResult : V;

        /// <summary>
        /// Determines whether this instance contains a value of the specified type.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>
        ///   <c>true</c> if this instance contains type; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsType<TResult>() where TResult : V;

        /// <summary>
        /// Determines whether this instance contains a value of the specified type.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key contains type; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is <c>null</c>.</exception>
        bool ContainsType(Type key);
    }
}
