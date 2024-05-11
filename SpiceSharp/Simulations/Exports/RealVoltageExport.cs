using SpiceSharp.Simulations.Base;
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
        /// Gets the positive node.
        /// </summary>
        public Reference Positive { get; }

        /// <summary>
        /// Gets the reference node.
        /// </summary>
        public Reference Reference { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RealVoltageExport"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="posNode">The positive node (can be a string or a string array for a path).</param>
        /// <param name="refNode">The reference/negative node (can be a string or a string array for a path).</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="posNode"/> and <paramref name="refNode"/> are both empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="simulation"/> is <c>null</c>.</exception>
        public RealVoltageExport(IBiasingSimulation simulation, Reference posNode, Reference refNode = default)
            : base(simulation)
        {
            if (posNode.Length == 0 && refNode.Length == 0)
                throw new ArgumentException(Properties.Resources.References_IsEmptyReference);
            Positive = posNode;
            Reference = refNode;
        }

        /// <inheritdoc />
        protected override void Initialize(object sender, EventArgs e)
        {
            // Find the positive node variable
            if (Positive.Length > 0 && Reference.Length > 0)
            {
                var posVariable = Positive.GetVariable<double, IBiasingSimulationState>(Simulation);
                var refVariable = Reference.GetVariable<double, IBiasingSimulationState>(Simulation);
                Extractor = () => posVariable.Value - refVariable.Value;
            }
            else if (Positive.Length > 0)
            {
                var posVariable = Positive.GetVariable<double, IBiasingSimulationState>(Simulation);
                Extractor = () => posVariable.Value;
            }
            else
            {
                var refVariable = Reference.GetVariable<double, IBiasingSimulationState>(Simulation);
                Extractor = () => -refVariable.Value;
            }
        }

        /// <summary>
        /// Converts the export to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            if (Positive.Length > 0 && Reference.Length > 0)
                return "V({0},{1})".FormatString(Positive, Reference);
            else if (Positive.Length > 0)
                return "V({0})".FormatString(Positive);
            else
                return "V(0,{0})".FormatString(Reference);
        }
    }
}
