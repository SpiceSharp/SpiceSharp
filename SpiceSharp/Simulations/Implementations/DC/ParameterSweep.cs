using SpiceSharp.Behaviors;
using SpiceSharp.ParameterSets;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A sweep of a property of an entity.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="ISweep" />
    public class ParameterSweep : ParameterSet, ISweep
    {
        /// <summary>
        /// Gets the name of the sweep.
        /// </summary>
        /// <value>
        /// The name of the sweep.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the property that needs to be swept.
        /// </summary>
        /// <value>
        /// The property to be swept.
        /// </value>
        public string Property { get; }

        /// <summary>
        /// Gets the method called when the value has been updated.
        /// </summary>
        /// <value>
        /// The update method.
        /// </value>
        public Action<IBehaviorContainer> Update { get; }

        /// <summary>
        /// Gets or sets the points to apply.
        /// </summary>
        /// <value>
        /// The points to apply.
        /// </value>
        public IEnumerable<double> Points { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSweep"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="property">The property name.</param>
        /// <param name="points">The points.</param>
        public ParameterSweep(string name, string property, IEnumerable<double> points)
        {
            Name = name.ThrowIfNull(nameof(name));
            Property = property.ThrowIfNull(nameof(property));
            Points = points.ThrowIfNull(nameof(points));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSweep"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="points">The points.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="points"/> is <c>null</c>.</exception>
        public ParameterSweep(string name, IEnumerable<double> points)
        {
            Name = name.ThrowIfNull(nameof(name));
            Property = "dc";
            Points = points.ThrowIfNull(nameof(points));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSweep"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="property">The property.</param>
        /// <param name="points">The points.</param>
        /// <param name="update">The method used to update the entity.</param>
        public ParameterSweep(string name, string property, IEnumerable<double> points, Action<IBehaviorContainer> update)
        {
            Name = name.ThrowIfNull(nameof(name));
            Property = property.ThrowIfNull(nameof(property));
            Points = points.ThrowIfNull(nameof(points));
            Update = update;
        }

        /// <summary>
        /// Creates an enumerable that can sweep properties of the simulation.
        /// </summary>
        /// <param name="simulation">The simulation to create the points for.</param>
        /// <returns>
        /// The created sweep points.
        /// </returns>
        public IEnumerator<double> CreatePoints(IBiasingSimulation simulation)
        {
            var behaviors = simulation.EntityBehaviors[Name];
            Action<double> setter = null;
            foreach (var behavior in behaviors)
            {
                foreach (var ps in behavior.ParameterSets)
                {
                    setter = ps.CreateParameterSetter<double>(Property);
                    if (setter != null)
                        break;
                }
                if (setter != null)
                    break;
            }

            // Enumerate the points
            foreach (double pt in Points)
            {
                setter(pt);
                Update?.Invoke(behaviors);
                yield return pt;
            }
        }

        /// <inheritdoc/>
        public ISweep Clone()
            => (ISweep)MemberwiseClone();
    }
}
