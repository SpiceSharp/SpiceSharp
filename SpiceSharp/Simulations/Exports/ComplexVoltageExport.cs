using System;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export complex voltages.
    /// </summary>
    /// <seealso cref="Export{T}" />
    public class ComplexVoltageExport : Export<Complex>
    {
        /// <summary>
        /// Gets the identifier of the positive node.
        /// </summary>
        public string PosNode { get; }

        /// <summary>
        /// Gets the index of the positive node variable.
        /// </summary>
        public int PosIndex { get; private set; }

        /// <summary>
        /// Gets the identifier of the negative node.
        /// </summary>
        public string NegNode { get; }

        /// <summary>
        /// Gets the index of the negative node variable.
        /// </summary>
        public int NegIndex { get; private set; }

        /// <summary>
        /// Gets the amplitude in decibels (dB).
        /// </summary>
        public double Decibels
        {
            get
            {
                var result = Value;
                return 10.0 * Math.Log10(result.Real * result.Real + result.Imaginary * result.Imaginary);
            }
        }

        /// <summary>
        /// Gets the phase in radians.
        /// </summary>
        public double Phase
        {
            get
            {
                var result = Value;
                return Math.Atan2(result.Imaginary, result.Real);
            }
        }

        /// <summary>
        /// Check if the simulation is a <see cref="FrequencySimulation" />.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <returns></returns>
        protected override bool IsValidSimulation(Simulation simulation) => simulation is FrequencySimulation;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexVoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="posNode">The node identifier.</param>
        public ComplexVoltageExport(FrequencySimulation simulation, string posNode)
            : base(simulation)
        {
            PosNode = posNode.ThrowIfNull(nameof(posNode));
            PosIndex = -1;
            NegNode = null;
            NegIndex = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexVoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="posNode">The positive node identifier.</param>
        /// <param name="negNode">The negative node identifier.</param>
        public ComplexVoltageExport(FrequencySimulation simulation, string posNode, string negNode)
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
            // Create our extractor!
            var state = ((FrequencySimulation) Simulation).ComplexState.ThrowIfNull("complex state");
            if (Simulation.Variables.TryGetNode(PosNode, out var posNode))
            {
                PosIndex = posNode.Index;
                if (NegNode == null)
                    Extractor = () => state.Solution[PosIndex];
                else if (Simulation.Variables.TryGetNode(NegNode, out var negNode))
                {
                    NegIndex = negNode.Index;
                    Extractor = () => state.Solution[PosIndex] - state.Solution[NegIndex];
                }
            }
        }

        /// <summary>
        /// Finalizes the export.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void Finalize(object sender, EventArgs e)
        {
            base.Finalize(sender, e);
            PosIndex = -1;
            NegIndex = -1;
        }
    }
}
