using System;
using SpiceSharp.Diagnostics;

namespace SpiceSharp
{
    /// <summary>
    /// A parameter set that supports importing or setting parameters.
    /// </summary>
    /// <remarks>
    /// Parameters can be named using the <see cref="Attributes.ParameterNameAttribute" />.
    /// </remarks>
    public interface IImportParameterSet
    {
        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="ParameterNotFoundException">Thrown if the parameter could not be found.</exception>
        void SetParameter<P>(string name, P value);

        /// <summary>
        /// Tries to set the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the parameter was set; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        bool TrySetParameter<P>(string name, P value);

        /// <summary>
        /// Creates a setter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A setter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        Action<P> CreateParameterSetter<P>(string name);
    }

    /// <summary>
    /// Describes an interface that can chain parameter method calls.
    /// </summary>
    /// <typeparam name="T">The class type that will be chained.</typeparam>
    /// <seealso cref="IParameterSet" />
    public interface IImportParameterSet<T> : IImportParameterSet
    {
        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The current instance for chaining.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="ParameterNotFoundException">Thrown if the parameter could not be found.</exception>
        new T SetParameter<P>(string name, P value);
    }
}
