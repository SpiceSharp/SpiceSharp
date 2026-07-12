using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    /// <summary>
    /// A configuration that allows doing transient analysis using a fixed-timestep backward Euler integration method.
    /// This method is one of the fastest, but can also lead to more inaccurate results.
    /// Reducing the timestep will improve the truncation errors, but can worsen numerical accuracy.
    /// </summary>
    /// <seealso cref="TimeParameters" />
    [GeneratedParameters]
    public partial class FixedEuler : TimeParameters
    {
        /// <summary>
        /// Gets or sets the timestep.
        /// </summary>
        /// <value>
        /// The timestep.
        /// </value>
        /// <exception cref="ArgumentException">Thrown if the timestep is negative or zero.</exception>
        [ParameterName("step"), ParameterInfo("The fixed timestep used.")]
        public double Step
        {
            get => _step;
            set => _step = value.Finite(nameof(Step)).GreaterThan(nameof(Step), 0.0);
        }
        private double _step;

        /// <summary>
        /// Creates an instance of the integration method.
        /// </summary>
        /// <param name="state">The biasing simulation state that will be used as a base.</param>
        /// <returns>
        /// The integration method.
        /// </returns>
        public override IIntegrationMethod Create(IBiasingSimulationState state)
        {
            _step.Finite(nameof(Step)).GreaterThan(nameof(Step), 0.0);
            return new Instance(this);
        }
    }
}
