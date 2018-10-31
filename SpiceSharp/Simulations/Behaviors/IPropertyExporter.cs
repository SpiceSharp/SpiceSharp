using System;
using System.Collections.Generic;
using System.Text;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A class that exposes named properties.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPropertyExporter<out T> where T : struct
    {

        /// <summary>
        /// Creates a getter for a specified property.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        Func<T> CreateGetter(Simulation simulation, string propertyName);

        /// <summary>
        /// Creates a getter for a specified property.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="comparer">The property name comparer.</param>
        /// <returns></returns>
        Func<T> CreateGetter(Simulation simulation, string propertyName, IEqualityComparer<string> comparer);
    }
}
