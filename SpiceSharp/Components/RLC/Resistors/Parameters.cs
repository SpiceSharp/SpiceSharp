using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Resistors
{
    /// <summary>
    /// Parameters for a <see cref="Resistor" />.
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    [GeneratedParameters]
    public partial class Parameters : ParameterSet<Parameters>
    {
        /// <summary>
        /// The minimum resistance for any resistor.
        /// </summary>
        public const double MinimumResistance = 1e-12;

        /// <summary>
        /// Gets or sets the temperature parameter in degrees Kelvin.
        /// </summary>
        /// <value>
        /// The temperature of the resistor.
        /// </value>
        [GreaterThan(0), Finite]
        private GivenParameter<double> _temperature = new(Constants.ReferenceTemperature, false);

        /// <summary>
        /// Gets or sets the resistance of the resistor.
        /// </summary>
        /// <value>
        /// The resistance.
        /// </value>
        /// <remarks>
        /// If the resistance is limited to <see cref="MinimumResistance" /> to avoid numerical instability issues. 
        /// If a 0 Ohm resistance is wanted, consider using an ideal voltage source instead.
        /// </remarks>
        [ParameterName("resistance"), ParameterName("r"), ParameterInfo("Resistance", Units = "\u03a9", IsPrincipal = true)]
        [GreaterThanOrEquals(0), LowerLimit(MinimumResistance), Finite]
        private GivenParameter<double> _resistance;

        /// <summary>
        /// Gets or sets the resistor operating temperature in degrees Celsius.
        /// </summary>
        /// <value>
        /// The resistor operating temperature in degrees Celsius.
        /// </value>
        [ParameterName("temp"), ParameterInfo("Instance operating temperature", Units = "\u00b0C", Interesting = false)]
        [DerivedProperty, GreaterThan(-Constants.CelsiusKelvin), Finite]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets or sets the width of the resistor.
        /// </summary>
        /// <value>
        /// The width of the resistor.
        /// </value>
        [ParameterName("w"), ParameterInfo("Width", Units = "m")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _width = new(1.0, false);

        /// <summary>
        /// Gets or sets the length of the resistor.
        /// </summary>
        /// <value>
        /// The length of the resistor.
        /// </value>
        [ParameterName("l"), ParameterInfo("Length", Units = "m")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _length;

        /// <summary>
        /// Gets or sets the number of resistors in parallel.
        /// </summary>
        /// <value>
        /// The number of resistors in parallel.
        /// </value>
        [ParameterName("m"), ParameterInfo("Parallel multiplier")]
        [GreaterThanOrEquals(0), Finite]
        private double _parallelMultiplier = 1.0;

        /// <summary>
        /// Gets or sets the number of resistors in series.
        /// </summary>
        /// <value>
        /// The number of resistors in series.
        /// </value>
        [ParameterName("n"), ParameterInfo("Series multiplier")]
        [GreaterThan(0), Finite]
        private double _seriesMultiplier = 1.0;
    }
}