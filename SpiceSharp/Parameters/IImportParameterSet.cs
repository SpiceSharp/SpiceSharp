using SpiceSharp.Diagnostics;
using System;

namespace SpiceSharp.ParameterSets
{
    /// <summary>
    /// An <see cref="IParameterSet"/> that supports importing or setting parameters by specifying the
    /// name of the parameter. All parameters are of the same type.
    /// </summary>
    /// <typeparam name="P">The type of the parameter values.</typeparam>
    /// <remarks>
    /// Parameters can be named using the <see cref="ParameterNameAttribute" />. This interface
    /// can be used to avoid reflection and speed up setting parameters, but the code base
    /// increases.
    /// </remarks>
    public interface IImportParameterSet<in P> : IParameterSet
    {
        /// <summary>
        /// Sets the value of the parameter with the specified name.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="ParameterNotFoundException">Thrown if the parameter could not be found.</exception>
        void SetParameter(string name, P value);

        /// <summary>
        /// Tries to set the value of the parameter with the specified name.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the parameter was set; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        bool TrySetParameter(string name, P value);

        /// <summary>
        /// Creates a setter for a parameter with the specified name.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A setter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        Action<P> CreateParameterSetter(string name);
    }
}
