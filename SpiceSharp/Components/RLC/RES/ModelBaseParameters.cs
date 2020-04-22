using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// Parameters for a <see cref="ResistorModel"/>
    /// </summary>
    [GeneratedParameters]
    public class ModelBaseParameters : ParameterSet
    {
        private double _defaultWidth = 10e-6;
        private GivenParameter<double> _nominalTemperature = new GivenParameter<double>(Constants.ReferenceTemperature, false);

        /// <summary>
        /// Gets or sets the nominal temperature in degrees Celsius.
        /// </summary>
        [ParameterName("tnom"), DerivedProperty(), ParameterInfo("Parameter measurement temperature", Units = "\u00b0C", Interesting = false)]
        [GreaterThan(Constants.CelsiusKelvin)]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the nominal temperature parameter in degrees Kelvin.
        /// </summary>
        [GreaterThan(0)]
        public GivenParameter<double> NominalTemperature
        {
            get => _nominalTemperature;
            set
            {
                Utility.GreaterThan(value, nameof(NominalTemperature), 0);
                _nominalTemperature = value;
            }
        }

        /// <summary>
        /// Gets the first-order temperature coefficient parameter.
        /// </summary>
        [ParameterName("tc1"), ParameterInfo("First order temperature coefficient", Units = "\u03a9/K")]
        public double TemperatureCoefficient1 { get; set; }

        /// <summary>
        /// Gets the second-order temperature coefficient parameter.
        /// </summary>
        [ParameterName("tc2"), ParameterInfo("Second order temperature coefficient", Units = "\u03a9/K^2")]
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
        [ParameterName("defw"), ParameterInfo("Default device width", Units = "m")]
        [GreaterThan(0)]
        public double DefaultWidth
        {
            get => _defaultWidth;
            set
            {
                Utility.GreaterThan(value, nameof(DefaultWidth), 0);
                _defaultWidth = value;
            }
        }

        /// <summary>
        /// Gets the narrowing coefficient parameter.
        /// </summary>
        [ParameterName("narrow"), ParameterInfo("Narrowing of resistor", Units = "m")]
        public double Narrow { get; set; }
    }
}
