using SpiceSharp.Diagnostics;
using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.ParameterSets
{
    /// <summary>
    /// Interface that indicates that a class contains parameters and/or properties
    /// that are named.
    /// </summary>
    /// <remarks>
    /// Named parameters or properties are tagged with the attribute <see cref="ParameterNameAttribute"/>.
    /// </remarks>
    public interface IParameterSet
    {
        /// <summary>
        /// Sets a parameter in the parameter set of the specified type and with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value that the parameter should be set to.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="ParameterNotFoundException">Thrown if a parameter by the specified name could not be found.</exception>
        public void SetParameter<P>(string name, P value);

        /// <summary>
        /// Tries to set a parameter in the parameter set of the specified type and with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value that the parameter should be set to.</param>
        /// <returns>
        ///   <c>true</c> if a parameter was found and set succesfully; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name" /> is <c>null</c>.</exception>
        public bool TrySetParameter<P>(string name, P value);

        /// <summary>
        /// Gets the value of a property of the specified type and with the specified name.
        /// </summary>
        /// <typeparam name="P">The property value type.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <returns>
        /// The value of the property.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="ParameterNotFoundException">Thrown if a parameter by the specified name could not be found.</exception>
        public P GetProperty<P>(string name);

        /// <summary>
        /// Tries to get the value of a property of the specified type and with the specified name.
        /// </summary>
        /// <typeparam name="P">The property value type.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property if the property was found.</param>
        /// <returns>
        ///   <c>true</c> if the property was found and returned; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public bool TryGetProperty<P>(string name, out P value);

        /// <summary>
        /// Creates an action that can set the parameter of the specified type and with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// An action that can set the parameter value, or <c>null</c> if the parameter could not be found.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Action<P> CreateParameterSetter<P>(string name);

        /// <summary>
        /// Creates a function that can get the value of a property of the specified type and with the specified name.
        /// </summary>
        /// <typeparam name="P">The property value type.</typeparam>
        /// <param name="name">The name of the property.</param>
        /// <returns>
        /// A function that can get the property value, or <c>null</c> if the property could not be found.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Func<P> CreatePropertyGetter<P>(string name);
    }
}
