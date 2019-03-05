using System;
using System.Numerics;
using SpiceSharp.Algebra.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export complex voltages.
    /// </summary>
    /// <seealso cref="Export{T}" />
    public class ComplexVoltageExport : Export<PreciseComplex>
    {
        /// <summary>
        /// Gets the identifier of the positive node.
        /// </summary>
        /// <value>
        /// The positive node identifier.
        /// </value>
        public string PosNode { get; }

        /// <summary>
        /// Gets the identifier of the negative node.
        /// </summary>
        /// <value>
        /// The negative node identifier.
        /// </value>
        public string NegNode { get; }

        /// <summary>
        /// Gets the amplitude in decibels (dB).
        /// </summary>
        /// <value>
        /// The amplitude.
        /// </value>
        public double Decibels
        {
            get
            {
                var result = Value;
                return 10.0 * Math.Log10((double)result.Real * (double)result.Real + (double)result.Imaginary * (double)result.Imaginary);
            }
        }

        /// <summary>
        /// Gets the phase in radians.
        /// </summary>
        /// <value>
        /// The phase.
        /// </value>
        public double Phase
        {
            get
            {
                var result = Value;
                return Math.Atan2((double)result.Imaginary, (double)result.Real);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexVoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="posNode">The node identifier.</param>
        /// <exception cref="ArgumentNullException">posNode</exception>
        public ComplexVoltageExport(Simulation simulation, string posNode)
            : base(simulation)
        {
            PosNode = posNode ?? throw new ArgumentNullException(nameof(posNode));
            NegNode = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexVoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="posNode">The positive node identifier.</param>
        /// <param name="negNode">The negative node identifier.</param>
        /// <exception cref="ArgumentNullException">posNode</exception>
        public ComplexVoltageExport(Simulation simulation, string posNode, string negNode)
            : base(simulation)
        {
            PosNode = posNode ?? throw new ArgumentNullException(nameof(posNode));
            NegNode = negNode;
        }

        /// <summary>
        /// Initializes the export.
        /// </summary>
        /// <param name="sender">The object (simulation) sending the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs" /> instance containing the event data.</param>
        protected override void Initialize(object sender, EventArgs e)
        {
            // Create our extractor!
            var state = ((FrequencySimulation) Simulation).ComplexState;
            if (Simulation.Variables.TryGetNode(PosNode, out var posNode))
            {
                var posNodeIndex = posNode.Index;
                if (NegNode == null)
                    Extractor = () => state.Solution[posNodeIndex];
                else if (Simulation.Variables.TryGetNode(NegNode, out var negNode))
                {
                    var negNodeIndex = negNode.Index;
                    Extractor = () => state.Solution[posNodeIndex] - state.Solution[negNodeIndex];
                }
            }
        }
    }
}
