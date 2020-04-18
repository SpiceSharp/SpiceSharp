using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="Diode" />
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the area parameter.
        /// </summary>
        [ParameterName("area"), ParameterInfo("Area factor", Units = "m^2")]
        [GreaterThanOrEquals(0)]
        public double Area { get; set; } = 1;

        /// <summary>
        /// Gets or sets whether or not the diode is initially off (non-conducting).
        /// </summary>
        [ParameterName("off"), ParameterInfo("Initially off")]
        public bool Off { get; set; }

        /// <summary>
        /// Gets or sets the initial condition.
        /// </summary>
        [ParameterName("ic"), ParameterInfo("Initial device voltage", Units = "V")]
        public double InitCond { get; set; }
        
        /// <summary>
        /// Gets or sets the temperature in degrees Celsius.
        /// </summary>
        [ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance temperature", Units = "\u00b0C")]
        [GreaterThan(Constants.CelsiusKelvin)]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the temperature parameter in degrees Kelvin.
        /// </summary>
        [GreaterThan(0)]
        public GivenParameter<double> Temperature { get; set; } = new GivenParameter<double>(Constants.ReferenceTemperature, false);

        /// <summary>
        /// Gets or sets the parallel multiplier.
        /// </summary>
        /// <value>
        /// The parallel multiplier.
        /// </value>
        [ParameterName("m"), ParameterInfo("Parallel multiplier")]
        [GreaterThanOrEquals(0)]
        public double ParallelMultiplier { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the series multiplier.
        /// </summary>
        /// <value>
        /// The series multiplier.
        /// </value>
        [ParameterName("n"), ParameterInfo("Series multiplier")]
        [GreaterThan(0)]
        public double SeriesMultiplier { get; set; } = 1.0;
    }
}
