using SpiceSharp.Components;
using SpiceSharp.Simulations.Base;
using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export real currents.
    /// </summary>
    /// <seealso cref="Export{S, T}" />
    public class RealCurrentExport : Export<IBiasingSimulation, double>
    {
        /// <summary>
        /// Gets the name of the voltage source.
        /// </summary>
        public Reference Source { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexCurrentExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="source">The source name.</param>
        public RealCurrentExport(IBiasingSimulation simulation, Reference source)
            : base(simulation)
        {
            if (source.Length == 0)
                throw new ArgumentException(Properties.Resources.References_IsEmptyReference, nameof(source));
            Source = source;
        }

        /// <inheritdoc />
        protected override void Initialize(object sender, EventArgs e)
        {
            var behaviors = Source.GetContainer(Simulation);
            var behavior = behaviors.GetValue<IBranchedBehavior<double>>();
            var branch = behavior.Branch;
            Extractor = () => branch.Value;
        }

        /// <summary>
        /// Converts the export to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
            => "I({0})".FormatString(Source);
    }
}
