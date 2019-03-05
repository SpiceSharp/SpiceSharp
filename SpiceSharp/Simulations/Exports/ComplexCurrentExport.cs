using System;
using System.Numerics;
using SpiceSharp.Algebra.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export complex currents.
    /// </summary>
    /// <seealso cref="Export{T}" />
    public class ComplexCurrentExport : Export<PreciseComplex>
    {
        /// <summary>
        /// Gets the identifier of the voltage source.
        /// </summary>
        /// <value>
        /// The voltage source identifier.
        /// </value>
        public string Source { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealCurrentExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="source">The source identifier.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public ComplexCurrentExport(Simulation simulation, string source)
            : base(simulation)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            // Create our extractor!
            var state = ((BaseSimulation) Simulation).RealState;
            if (Simulation.EntityBehaviors.TryGetBehaviors(Source, out var ebd))
            {
                if (ebd.TryGetValue(typeof(Components.VoltageSourceBehaviors.FrequencyBehavior), out var behavior))
                {
                    var index = ((Components.VoltageSourceBehaviors.FrequencyBehavior) behavior).BranchEq;
                    Extractor = () => state.Solution[index];
                }
            }
        }
    }
}
