using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    /// <summary>
    /// A configuration that allows doing transient analysis using a fixed-timestep backward Euler integration method.
    /// This method is one of the fastest, but can also lead to inaccurate results.
    /// </summary>
    /// <seealso cref="TimeParameters" />
    public partial class FixedEuler : TimeParameters
    {
        /// <summary>
        /// Gets or sets the step.
        /// </summary>
        /// <value>
        /// The step.
        /// </value>
        /// <exception cref="ArgumentException">Thrown if the timestep is negative or zero.</exception>
        [ParameterName("step"), ParameterInfo("The fixed timestep used.")]
        public double Step
        {
            get => _step;
            set
            {
                if (value <= 0.0)
                    throw new ArgumentException(Properties.Resources.Simulations_Time_TimestepInvalid);
                _step = value;
            }
        }
        private double _step;

        /// <summary>
        /// Creates an instance of the integration method for an associated <see cref="IBiasingSimulationState" />.
        /// </summary>
        /// <param name="simulation">The simulation that provides the biasing state.</param>
        /// <returns>
        /// The integration method.
        /// </returns>
        public override IIntegrationMethod Create(IStateful<IBiasingSimulationState> simulation)
            => new Instance(this, simulation);
    }
}
