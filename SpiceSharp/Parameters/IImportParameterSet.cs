using System;

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
        /// Call a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        void SetParameter(string name);

        /// <summary>
        /// Tries calling a parameter method with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// <c>true</c> if the method was called; otherwise <c>false</c>.
        /// </returns>
        bool TrySetParameter(string name);

        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        void SetParameter<P>(string name, P value);

        /// <summary>
        /// Tries to set the value of the parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the parameter was set; otherwise <c>false</c>.
        /// </returns>
        bool TrySetParameter<P>(string name, P value);

        /// <summary>
        /// Creates a setter for a parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A setter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
        Action<P> CreateParameterSetter<P>(string name);
    }
}
