using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export real voltages.
    /// </summary>
    /// <seealso cref="Export{S, T}" />
    public class RealVoltageExport : Export<IBiasingSimulation, double>
    {
        /// <summary>
        /// Gets the name of the positive node.
        /// </summary>
        public string PosNode { get; }

        /// <summary>
        /// Gets the index of the positive node variable.
        /// </summary>
        public int PosIndex { get; private set; }

        /// <summary>
        /// Gets the name of the negative node.
        /// </summary>
        public string NegNode { get; }

        /// <summary>
        /// gets the index of the negative node variable.
        /// </summary>
        public int NegIndex { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealVoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="posNode">The node name.</param>
        public RealVoltageExport(IBiasingSimulation simulation, string posNode)
            : base(simulation)
        {
            PosNode = posNode.ThrowIfNull(nameof(posNode));
            PosIndex = -1;
            NegNode = null;
            NegIndex = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealVoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="posNode">The positive node name.</param>
        /// <param name="negNode">The negative node name.</param>
        public RealVoltageExport(IBiasingSimulation simulation, string posNode, string negNode)
            : base(simulation)
        {
            PosNode = posNode.ThrowIfNull(nameof(posNode));
            PosIndex = -1;
            NegNode = negNode;
            NegIndex = -1;
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            if (Simulation is ISimulation<IVariable<double>> sim)
            {
                if (sim.Solved.TryGetValue(PosNode, out var node))
                {
                    var posNode = node;
                    if (NegNode == null)
                        Extractor = () => posNode.Value;
                    else if (sim.Solved.TryGetValue(NegNode, out node))
                    {
                        var negNode = node;
                        Extractor = () => posNode.Value - negNode.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Finalizes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void Finalize(object sender, EventArgs e)
        {
            base.Finalize(sender, e);
            PosIndex = -1;
            NegIndex = -1;
        }
    }
}
