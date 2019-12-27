using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A sweep of a voltage source or a current source.
    /// </summary>
    /// <seealso cref="ISweep" />
    public class SourceSweep : ParameterSet, ISweep
    {
        /// <summary>
        /// Gets the name of the sweep.
        /// </summary>
        /// <value>
        /// The name of the sweep.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the points to apply.
        /// </summary>
        /// <value>
        /// The points to apply.
        /// </value>
        public IEnumerable<double> Points { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceSweep"/> class.
        /// </summary>
        public SourceSweep(string name, IEnumerable<double> points)
        {
            Name = name.ThrowIfNull(nameof(name));
            Points = points.ThrowIfNull(nameof(points));
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
            var behavior = simulation.EntityBehaviors[Name];
            if (!behavior.TryGetProperty<Parameter<double>>("dc", out var parameter))
                throw new SpiceSharpException(Properties.Resources.Simulations_DC_CannotFindSource.FormatString(Name));
            foreach (var pt in Points)
            {
                parameter.Value = pt;
                yield return pt;
            }
        }
    }
}
