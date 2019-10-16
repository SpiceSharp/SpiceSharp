﻿using System;
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
        private Simulation _simulation;

        /// <summary>
        /// Gets the time if the simulation supports it.
        /// </summary>
        public double Time
        {
            get
            {
                if (_simulation.States.TryGetValue<ITimeSimulationState>(out var state))
                    return state.Method.Time;
                return double.NaN;
            }
        }

        /// <summary>
        /// Gets the frequency if the simulation supports it.
        /// </summary>
        public double Frequency
        {
            get
            {
                if (_simulation.States.TryGetValue<IComplexSimulationState>(out var state))
                {
                    if (!state.Laplace.Real.Equals(0.0))
                        return double.NaN;
                    return state.Laplace.Imaginary / 2.0 / Math.PI;
                }
                return double.NaN;
            }
        }

        /// <summary>
        /// Gets the laplace variable if the simulation supports it.
        /// </summary>
        public Complex Laplace
        {
            get
            {
                if (_simulation.States.TryGetValue<IComplexSimulationState>(out var state))
                    return state.Laplace;
                return double.NaN;
            }
        }

        /// <summary>
        /// Gets the current sweep value if the simulation is a <see cref="DC" /> analysis.
        /// </summary>
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
        public double GetVoltage(string positive, string negative)
        {
            positive.ThrowIfNull(nameof(positive));

            if (_simulation.States.TryGetValue<IBiasingSimulationState>(out var state))
            {
                // Get the voltage of the positive node
                var node = _simulation.Variables[positive];
                var index = state.Map[node];
                var voltage = state.Solution[index];

                // Subtract negative node if necessary
                if (negative != null)
                {
                    node = _simulation.Variables[negative];
                    index = state.Map[node];
                    voltage -= state.Solution[index];
                }

                return voltage;
            }
            return double.NaN;
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
        public Complex GetComplexVoltage(string positive, string negative)
        {
            positive.ThrowIfNull(nameof(positive));
            if (_simulation.States.TryGetValue<IComplexSimulationState>(out var state))
            {
                // Get the voltage of the positive node
                var node = _simulation.Variables[positive];
                var index = state.Map[node];
                var voltage = state.Solution[index];

                // Subtract negative node if necessary
                if (negative != null)
                {
                    node = _simulation.Variables[negative];
                    index = state.Map[node];
                    voltage -= state.Solution[index];
                }

                return voltage;
            }
            return double.NaN;
        }
    }
}
