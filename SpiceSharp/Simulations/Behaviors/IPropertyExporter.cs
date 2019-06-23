using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A class that exposes named properties.
    /// </summary>
    public interface IPropertyExporter
    {
        /// <summary>
        /// Creates a getter for a property.
        /// </summary>
        /// <typeparam name="T">The expected type.</typeparam>
        /// <param name="simulation">The simulation.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="comparer">The property name comparer.</param>
        /// <param name="function">The function that will return the value of the property.</param>
        /// <returns>
        /// <c>true</c> if the getter was created successfully; otherwise <c>false</c>.
        /// </returns>
        bool CreateExportMethod<T>(Simulation simulation, string propertyName, out Func<T> function, IEqualityComparer<string> comparer = null);
    }
}
