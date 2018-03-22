using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Export for real voltages
    /// </summary>
    public class RealVoltageExport : Export<double>
    {
        /// <summary>
        /// Gets the identifier of the positive node
        /// </summary>
        public Identifier PosNode { get; }

        /// <summary>
        /// Gets the identifier of the negative node
        /// </summary>
        public Identifier NegNode { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="posNode">Positive node</param>
        public RealVoltageExport(Simulation simulation, Identifier posNode)
            : base(simulation)
        {
            PosNode = posNode ?? throw new ArgumentNullException(nameof(posNode));
            NegNode = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="posNode">Positive node</param>
        /// <param name="negNode">Negative (reference) node</param>
        public RealVoltageExport(Simulation simulation, Identifier posNode, Identifier negNode)
            : base(simulation)
        {
            PosNode = posNode ?? throw new ArgumentNullException(nameof(posNode));
            NegNode = negNode;
        }


        /// <summary>
        /// Initialize the export
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            // Create our extractor!
            var state = Simulation.States.Get<RealState>();
            if (Simulation.Nodes.TryGetNode(PosNode, out Node posNode))
            {
                int posNodeIndex = posNode.Index;
                if (NegNode == null)
                    Extractor = () => state.Solution[posNodeIndex];
                else if (Simulation.Nodes.TryGetNode(NegNode, out Node negNode))
                {
                    int negNodeIndex = negNode.Index;
                    Extractor = () => state.Solution[posNodeIndex] - state.Solution[negNodeIndex];
                }
            }
        }
    }
}
