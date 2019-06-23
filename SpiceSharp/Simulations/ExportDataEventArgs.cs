using System;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that describes exported simulation data. Can be used by simulations to pass exported simulation data as an event argument.
    /// This class contains some helper methods for extracting data from the simulation.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ExportDataEventArgs : EventArgs
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private readonly Simulation _simulation;

        /// <summary>
        /// Gets the time if the simulation supports it.
        /// </summary>
        /// <value>
        /// The time or NaN if the simulation does not use time.
        /// </value>
        public double Time
        {
            get
            {
                if (_simulation is TimeSimulation timeSimulation && timeSimulation.Method != null)
                    return timeSimulation.Method.Time;
                return double.NaN;
            }
        }

        /// <summary>
        /// Gets the frequency if the simulation supports it.
        /// </summary>
        /// <value>
        /// The frequency in Hertz or NaN if the simulation does not use frequency.
        /// </value>
        public double Frequency
        {
            get
            {
                if (_simulation is FrequencySimulation frequencySimulation && frequencySimulation.ComplexState != null)
                {
                    if (!frequencySimulation.ComplexState.Laplace.Real.Equals(0.0))
                        return double.NaN;
                    return frequencySimulation.ComplexState.Laplace.Imaginary / (2.0 * Math.PI);
                }

                return double.NaN;
            }
        }

        /// <summary>
        /// Gets the laplace variable if the simulation supports it.
        /// </summary>
        /// <value>
        /// The laplace variable or NaN if the simulation does not use the laplace variable.
        /// </value>
        public Complex Laplace
        {
            get
            {
                if (_simulation is FrequencySimulation frequencySimulation && frequencySimulation.ComplexState != null)
                    return frequencySimulation.ComplexState.Laplace;
                return double.NaN;
            }
        }

        /// <summary>
        /// Gets the current sweep value if the simulation is a <see cref="DC" /> analysis.
        /// </summary>
        /// <value>
        /// The sweep value or NaN if the simulation is not a DC sweep analysis.
        /// </value>
        public double SweepValue
        {
            get
            {
                if (_simulation is DC dc && dc.Sweeps.Count > 0)
                    return dc.Sweeps.Top.CurrentValue;
                return double.NaN;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportDataEventArgs"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <exception cref="ArgumentNullException">simulation</exception>
        public ExportDataEventArgs(Simulation simulation)
        {
            _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
        }

        /// <summary>
        /// Gets the voltage at a specified node.
        /// </summary>
        /// <param name="node">The node identifier.</param>
        /// <returns>The extracted voltage.</returns>
        /// <remarks>
        /// For better performance, consider using <see cref="RealVoltageExport" />.
        /// </remarks>
        /// <seealso cref="RealVoltageExport" />
        public double GetVoltage(string node) => GetVoltage(node, null);

        /// <summary>
        /// Gets the differential voltage between two specified nodes.
        /// </summary>
        /// <param name="positive">The identifier of the node at the positive probe.</param>
        /// <param name="negative">The identifier of the node at the negative probe.</param>
        /// <returns>The extracted voltage.</returns>
        /// <exception cref="ArgumentNullException">positive</exception>
        /// <exception cref="CircuitException">Simulation does not support real voltages</exception>
        public double GetVoltage(string positive, string negative)
        {
            positive.ThrowIfNull(nameof(positive));

            if (!(_simulation is BaseSimulation bs))
                throw new CircuitException("Simulation does not support real voltages.");
            var state = bs.RealState;

            // Get the voltage of the positive node
            var index = _simulation.Variables.GetNode(positive).Index;
            var voltage = state.Solution[index];

            // Subtract negative node if necessary
            if (negative != null)
            {
                index = _simulation.Variables.GetNode(negative).Index;
                voltage -= state.Solution[index];
            }

            return voltage;
        }

        /// <summary>
        /// Gets the complex voltage at a specific node.
        /// </summary>
        /// <param name="node">The node identifier.</param>
        /// <returns>The extracted voltage.</returns>
        /// <remarks>
        /// For better performance, consider using <see cref="ComplexVoltageExport"/>
        /// </remarks>
        /// <seealso cref="ComplexVoltageExport"/>
        public Complex GetComplexVoltage(string node) => GetComplexVoltage(node, null);

        /// <summary>
        /// Gets the differential complex voltage between two specified nodes.
        /// </summary>
        /// <param name="positive">The identifier of the node at the positive probe.</param>
        /// <param name="negative">The identifier of the node at the negative probe.</param>
        /// <returns>
        /// The extracted voltage.
        /// </returns>
        /// <exception cref="ArgumentNullException">positive</exception>
        /// <exception cref="CircuitException">Simulation does not support complex voltages</exception>
        public Complex GetComplexVoltage(string positive, string negative)
        {
            positive.ThrowIfNull(nameof(positive));

            if (!(_simulation is FrequencySimulation fs))
                throw new CircuitException("Simulation does not support complex voltages.");
            var state = fs.ComplexState;

            // Get the voltage of the positive node
            var index = _simulation.Variables.GetNode(positive).Index;
            var voltage = state.Solution[index];

            // Subtract negative node if necessary
            if (negative != null)
            {
                index = _simulation.Variables.GetNode(negative).Index;
                voltage -= state.Solution[index];
            }

            return voltage;
        }
    }
}
