using System;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class can export complex voltages.
    /// </summary>
    /// <seealso cref="Export{S, T}" />
    public class ComplexVoltageExport : Export<IFrequencySimulation, Complex>
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
        /// Initializes a new instance of the <see cref="ComplexVoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="posNode">The node name.</param>
        public ComplexVoltageExport(IFrequencySimulation simulation, string posNode)
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
        /// <param name="posNode">The positive node name.</param>
        /// <param name="negNode">The negative node name.</param>
        public ComplexVoltageExport(IFrequencySimulation simulation, string posNode, string negNode)
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
            if (Simulation is ISimulation<IVariable<Complex>> sim)
            {
                if (sim.Solved.TryGetValue(PosNode, out var _node))
                {
                    var posNode = _node;
                    if (NegNode == null)
                        Extractor = () => posNode.Value;
                    else if (sim.Solved.TryGetValue(NegNode, out _node))
                    {
                        var negNode = _node;
                        Extractor = () => posNode.Value - negNode.Value;
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void Finalize(object sender, EventArgs e)
        {
            base.Finalize(sender, e);
            PosIndex = -1;
            NegIndex = -1;
        }
    }
}
