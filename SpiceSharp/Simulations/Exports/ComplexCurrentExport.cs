using System;
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
        /// Gets the identifier of the voltage source.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets the index in the of the current variable.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealCurrentExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="source">The source identifier.</param>
        public ComplexCurrentExport(IFrequencySimulation simulation, string source)
            : base(simulation)
        {
            Source = source.ThrowIfNull(nameof(source));
            Index = -1;
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            Simulation.GetState(out IComplexSimulationState state);
            if (Simulation.EntityBehaviors.TryGetBehaviors(Source, out var ebd))
            {
                if (ebd.TryGetValue(typeof(Components.VoltageSourceBehaviors.FrequencyBehavior), out var behavior))
                {
                    Index = state.Map[((Components.VoltageSourceBehaviors.FrequencyBehavior) behavior).Branch];
                    Extractor = () => state.Solution[Index];
                }
            }
        }

        /// <summary>
        /// Finalizes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void Finalize(object sender, EventArgs e)
        {
            base.Finalize(sender, e);
            Index = -1;
        }
    }
}
