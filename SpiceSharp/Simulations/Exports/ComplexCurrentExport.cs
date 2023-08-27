using SpiceSharp.Behaviors;
using SpiceSharp.Components;
using SpiceSharp.Components.Subcircuits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export complex currents.
    /// </summary>
    /// <seealso cref="Export{S, T}" />
    public class ComplexCurrentExport : Export<IFrequencySimulation, Complex>
    {
        /// <summary>
        /// Gets the name of the voltage source.
        /// </summary>
        public IReadOnlyList<string> SourcePath { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexCurrentExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="source">The source name.</param>
        public ComplexCurrentExport(IFrequencySimulation simulation, string source)
            : base(simulation)
        {
            source.ThrowIfNull(nameof(source));
            SourcePath = new[] { source };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexCurrentExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="sourcePath">The source name.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="simulation"/> is <c>null</c>, or if <paramref name="sourcePath"/> is <c>null</c> or empty.</exception>
        public ComplexCurrentExport(IFrequencySimulation simulation, IEnumerable<string> sourcePath)
            : base(simulation)
        {
            SourcePath = sourcePath.ThrowIfEmpty(nameof(sourcePath)).ToArray();
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            var behaviorContainer = Simulation.EntityBehaviors[SourcePath[0]];
            for (int i = 1; i < SourcePath.Count; i++)
            {
                var behavior = behaviorContainer.GetValue<EntitiesBehavior>();
                behaviorContainer = behavior.LocalBehaviors[SourcePath[i]];
            }

            // Find a branched behavior for the current
            var branchedBehavior = behaviorContainer.GetValue<IBranchedBehavior<Complex>>();
            var branch = branchedBehavior.Branch;
            Extractor = () => branch.Value;
        }
    }
}
