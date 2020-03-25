using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// Parameters for a <see cref="ResistorModel"/>
    /// </summary>
    public class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the nominal temperature in degrees Celsius.
        /// </summary>
        [ParameterName("tnom"), DerivedProperty(), ParameterInfo("Parameter measurement temperature", Units.Celsius, Interesting = false)]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the nominal temperature parameter in degrees Kelvin.
        /// </summary>
        public GivenParameter<double> NominalTemperature { get; set; } = new GivenParameter<double>(Constants.ReferenceTemperature, false);

        /// <summary>
        /// Gets the first-order temperature coefficient parameter.
        /// </summary>
        [ParameterName("tc1"), ParameterInfo("First order temperature coefficient", Units.OhmPerKelvin)]
        public double TemperatureCoefficient1 { get; set; }

        /// <summary>
        /// Gets the second-order temperature coefficient parameter.
        /// </summary>
        [ParameterName("tc2"), ParameterInfo("Second order temperature coefficient", Units.OhmPerKelvin2)]
        public double TemperatureCoefficient2 { get; set; }

        /// <summary>
        /// Gets the exponential temperature coefficient parameter.
        /// </summary>
        [ParameterName("tce"), ParameterInfo("Exponential temperature coefficient")]
        public GivenParameter<double> ExponentialCoefficient { get; set; }

        /// <summary>
        /// Gets the sheet resistance parameter.
        /// </summary>
        [ParameterName("rsh"), ParameterInfo("Sheet resistance")]
        public double SheetResistance { get; set; }

        /// <summary>
        /// Gets the default width parameter.
        /// </summary>
        [ParameterName("defw"), ParameterInfo("Default device width", Units.Meter)]
        public double DefaultWidth { get; set; } = 10e-6;

        /// <summary>
        /// Gets the narrowing coefficient parameter.
        /// </summary>
        [ParameterName("narrow"), ParameterInfo("Narrowing of resistor", Units.Meter)]
        public double Narrow { get; set; }
    }
}
