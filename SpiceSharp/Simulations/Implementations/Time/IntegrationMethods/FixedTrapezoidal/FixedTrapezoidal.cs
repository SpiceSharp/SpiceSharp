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
            set => _step = value.Finite(nameof(Step)).GreaterThan(nameof(Step), 0.0);
        }
        private double _step;

        /// <summary>
        /// Gets the xmu constant.
        /// </summary>
        /// <value>
        /// The xmu constant.
        /// </value>
        [ParameterName("xmu"), ParameterInfo("The xmu parameter.")]
        public double Xmu
        {
            get => _xmu;
            set
            {
                if (double.IsNaN(value))
                    throw new ArgumentOutOfRangeException(nameof(Xmu), value, Properties.Resources.Parameters_NotGreaterOrEqual.FormatString(0.0));
                _xmu = value.GreaterThanOrEquals(nameof(Xmu), 0.0).LessThan(nameof(Xmu), 1.0);
            }
        }
        private double _xmu = 0.5;

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
