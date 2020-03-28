using SpiceSharp.Components;
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
        public string Source { get; }

        /// <summary>
        /// Gets the index of the variable in the solver.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealCurrentExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="source">The source name.</param>
        public RealCurrentExport(IBiasingSimulation simulation, string source)
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
            if (Simulation.EntityBehaviors.TryGetBehaviors(Source, out var ebd))
            {
                if (ebd.TryGetValue<IBranchedBehavior<double>>(out var behavior))
                {
                    var branch = behavior.Branch;
                    Extractor = () => branch.Value;
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
