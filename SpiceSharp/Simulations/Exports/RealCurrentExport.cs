using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export real currents.
    /// </summary>
    /// <seealso cref="Export{T}" />
    public class RealCurrentExport : Export<double>
    {
        /// <summary>
        /// Gets the identifier of the voltage source.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets the index of the variable in the solver.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Check if the simulation is a base simulation.
        /// </summary>
        /// <param name="simulation"></param>
        /// <returns></returns>
        protected override bool IsValidSimulation(Simulation simulation) => simulation is BaseSimulation;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealCurrentExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="source">The source identifier.</param>
        public RealCurrentExport(BaseSimulation simulation, string source)
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
            // Create our extractor!
            var state = ((BaseSimulation) Simulation).RealState.ThrowIfNull("real state");
            if (Simulation.EntityBehaviors.TryGetBehaviors(Source, out var ebd))
            {
                if (ebd.TryGetValue(typeof(Components.VoltageSourceBehaviors.BiasingBehavior), out var behavior))
                {
                    Index = ((Components.VoltageSourceBehaviors.BiasingBehavior) behavior).BranchEq;
                    Extractor = () => state.Solution[Index];
                }
            }
        }

        /// <summary>
        /// Finalizes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">the <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void Finalize(object sender, EventArgs e)
        {
            base.Finalize(sender, e);
            Index = -1;
        }
    }
}
