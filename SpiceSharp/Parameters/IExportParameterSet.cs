using SpiceSharp.Diagnostics;
using System;

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
    /// can be used to avoid reflection and speed up getting properties, but the code size
    /// increases.
    /// </remarks>
    public interface IExportPropertySet<out P> : IParameterSet
    {
        /// <summary>
        /// Gets the value of the parameter with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        /// <exception cref="ParameterNotFoundException">Thrown if the property could not be found.</exception>
        P GetProperty(string name);

        /// <summary>
        /// Tries to get the value of the parameter with the specified name.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="isValid">If <c>true</c>, the property was found and the returned value is valid.</param>
        /// <returns>
        /// <c>true</c> if the parameter was found; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        P TryGetProperty(string name, out bool isValid);

        /// <summary>
        /// Creates a getter for a parameter with the specified name.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A getter for the parameter value if it exists; otherwise <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        Func<P> CreatePropertyGetter(string name);
    }
}
