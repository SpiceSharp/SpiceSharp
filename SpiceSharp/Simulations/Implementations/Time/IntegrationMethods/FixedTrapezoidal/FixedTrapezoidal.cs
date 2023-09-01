using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    /// <summary>
    /// A configuration that allows doing transient analysis using a fixed-timestep trapezoidal integration method.
    /// This method is pretty fast, but can also lead to more inaccurate results.
    /// Reducing the timestep will improve the truncation errors, but can worsen numerical accuracy.
    /// </summary>
    /// <seealso cref="TimeParameters"/>
    [GeneratedParameters]
    public partial class FixedTrapezoidal : TimeParameters
    {
        /// <summary>
        /// Gets or sets the timestep.
        /// </summary>
        /// <value>
        /// The timestep.
        /// </value>
        [ParameterName("step"), ParameterInfo("The fixed timestep used.")]
        public double Step
        {
            get => _step;
            set
            {
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Simulations_Time_TimestepInvalid);
                _step = value;
            }
        }
        private double _step;

        /// <summary>
        /// Gets the xmu constant.
        /// </summary>
        /// <value>
        /// The xmu constant.
        /// </value>
        [ParameterName("xmu"), ParameterInfo("The xmu parameter.")]
        public double Xmu { get; set; } = 0.5;

        /// <summary>
        /// Creates an instance of the integration method.
        /// </summary>
        /// <param name="state">The biasing simulation state that will be used as a base.</param>
        /// <returns>
        /// The integration method.
        /// </returns>
        public override IIntegrationMethod Create(IBiasingSimulationState state) => new Instance(this);
    }
}
