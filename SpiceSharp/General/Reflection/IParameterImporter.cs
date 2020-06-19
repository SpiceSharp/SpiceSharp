using System;

namespace SpiceSharp.Reflection
{
    /// <summary>
    /// An interface that describes a type that can import parameters of a specific type.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public interface IParameterImporter<in T> : IMemberMap
    {
        /// <summary>
        /// Tries setting a parameter value.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>
        ///   <c>true</c> if the parameter was set succesfully; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if the value is invalid for the parameter.</exception>
        bool TrySet(object source, string name, T value);

        /// <summary>
        /// Creates a setter for the parameter with the specified name.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// The action that sets the parameter; or <c>null</c> if the parameter does not exist.
        /// </returns>
        Action<T> CreateSetter(object source, string name);
    }
}
