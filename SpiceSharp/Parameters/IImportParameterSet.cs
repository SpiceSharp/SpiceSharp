using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.ParameterSets
{
    /// <summary>
    /// An <see cref="IParameterSet"/> that supports importing or setting parameters by specifying the
    /// name of the parameter. All parameters are of the same type.
    /// </summary>
    /// <typeparam name="P">The type of the parameter values.</typeparam>
    /// <remarks>
    /// Properties can be named using the <see cref="ParameterNameAttribute" />. This interface
    /// can be used to link the names to these properties. The recomended way to implement it is
    /// through the Spice# source generator.
    /// </remarks>
    public interface IImportParameterSet<in P> : IParameterSet
    {
        /// <summary>
        /// Creates a setter for a parameter with the specified name.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A setter if the parameter exists; otherwise <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        Action<P> GetParameterSetter(string name);
    }
}
