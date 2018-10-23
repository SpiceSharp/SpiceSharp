using System;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that describes current frequency sweep simulation data.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class FrequencySweepDataEventArgs : EventArgs
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private readonly Simulation _simulation;

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
        /// Initializes a new instance of the <see cref="ExportDataEventArgs"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <exception cref="ArgumentNullException">simulation</exception>
        public FrequencySweepDataEventArgs(Simulation simulation)
        {
            _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
        }
    }
}
