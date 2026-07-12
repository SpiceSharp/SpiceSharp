using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    /// <summary>
    /// A class that implements the trapezoidal integration method as
    /// implemented by Spice 3f5.
    /// </summary>
    /// <seealso cref="SpiceMethod" />
    [GeneratedParameters]
    public partial class Trapezoidal : SpiceMethod
    {
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
        /// Creates an instance of the integration method for an associated <see cref="IBiasingSimulationState" />.
        /// </summary>
        /// <param name="state">The simulation that provides the biasing state.</param>
        /// <returns>
        /// The integration method.
        /// </returns>
        public override IIntegrationMethod Create(IBiasingSimulationState state) => new Instance(this, state);
    }
}
