using System;

namespace SpiceSharp.Reflection
{
    /// <summary>
    /// An interface that describes a type that can export parameters of a specific type.
    /// </summary>
    /// <typeparam name="T">The base valye type.</typeparam>
    public interface IPropertyExporter<out T> : IMemberMap
    {
        /// <summary>
        /// Tries getting the value of a parameter.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name.</param>
        /// <param name="isValid">If set to <c>true</c>, the parameter was returned succesfully.</param>
        /// <returns>
        /// The parameter value.
        /// </returns>
        /// <remarks>
        /// This method does not follow the regular TryGet convention due to the interface
        /// being covariant.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>value</c>.</exception>
        T TryGet(object source, string name, out bool isValid);

        /// <summary>
        /// Creates a getter for parameter with the specified name.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// The function that returns the parameter; or <c>null</c> if the parameter doesn't exist.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        Func<T> CreateGetter(object source, string name);
    }
}
