using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.ParameterSets
{
    /// <summary>
    /// An <see cref="IParameterSet"/> that support exporting or getting their values by specifying the
    /// name of the property. All properties are of the same type.
    /// </summary>
    /// <typeparam name="P">The type of the properties.</typeparam>
    /// <seealso cref="IParameterSet" />
    /// <remarks>
    /// Properties can be named using the <see cref="ParameterNameAttribute" />. This interface
    /// can be used to link the names to these properties. The recomended way to implement it is
    /// through the Spice# source generator.
    /// </remarks>
    public interface IExportPropertySet<out P> : IParameterSet
    {
        /// <summary>
        /// Creates a getter for a parameter with the specified name.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A getter for the parameter value if it exists; otherwise <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        Func<P> GetPropertyGetter(string name);
    }
}
