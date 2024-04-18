using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Resistors
{
    /// <summary>
    /// Parameters for a <see cref="ResistorModel"/>.
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    [GeneratedParameters]
    public partial class ModelParameters : ParameterSet<ModelParameters>
    {
        /// <summary>
        /// Gets or sets the nominal temperature in degrees Celsius.
        /// </summary>
        /// <value>
        /// The nominal temperature in degrees Celsius.
        /// </value>
        [ParameterName("tnom"), ParameterInfo("Parameter measurement temperature", Units = "\u00b0C", Interesting = false)]
        [DerivedProperty, GreaterThan(-Constants.CelsiusKelvin)]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets or sets the nominal temperature parameter in degrees Kelvin.
        /// </summary>
        /// <value>
        /// The nominal temperature in degrees Kelvin.
        /// </value>
        [GreaterThan(0)]
        private GivenParameter<double> _nominalTemperature = new(Constants.ReferenceTemperature, false);

        /// <summary>
        /// Gets or sets the first-order temperature coefficient parameter.
        /// </summary>
        /// <value>
        /// The temperature coefficient 1.
        /// </value>
        [ParameterName("tc1"), ParameterInfo("First order temperature coefficient", Units = "\u03a9/K")]
        public double TemperatureCoefficient1 { get; set; }

        /// <summary>
        /// Gets or sets the second-order temperature coefficient parameter.
        /// </summary>
        /// <value>
        /// The temperature coefficient 2.
        /// </value>
        [ParameterName("tc2"), ParameterInfo("Second order temperature coefficient", Units = "\u03a9/K^2")]
        public double TemperatureCoefficient2 { get; set; }

        /// <summary>
        /// Gets or sets the exponential temperature coefficient parameter.
        /// </summary>
        /// <value>
        /// The exponential temperature coefficient parameter.
        /// </value>
        [ParameterName("tce"), ParameterInfo("Exponential temperature coefficient")]
        public GivenParameter<double> ExponentialCoefficient { get; set; }

        /// <summary>
        /// Gets or sets the sheet resistance.
        /// </summary>
        /// <value>
        /// The sheet resistance.
        /// </value>
        [ParameterName("rsh"), ParameterInfo("Sheet resistance", Units = "\u03a9/\u2b1c")]
        public double SheetResistance { get; set; }

        /// <summary>
        /// Gets or sets the default width.
        /// </summary>
        /// <value>
        /// The default width.
        /// </value>
        [ParameterName("defw"), ParameterInfo("Default device width", Units = "m")]
        [GreaterThan(0)]
        private double _defaultWidth = 10e-6;

        /// <summary>
        /// Gets or sets the narrowing coefficient.
        /// </summary>
        /// <value>
        /// The narrowing coefficient.
        /// </value>
        [ParameterName("narrow"), ParameterInfo("Narrowing of resistor", Units = "m")]
        public double Narrow { get; set; }
    }
}
