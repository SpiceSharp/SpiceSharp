using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// Parameters for a <see cref="Resistor" />.
    /// </summary>
	[GeneratedParameters]
    public class Parameters : ParameterSet
    {
        private GivenParameter<double> _temperature = new GivenParameter<double>(Constants.ReferenceTemperature, false);
        private double _seriesMultiplier = 1.0;
        private double _parallelMultiplier = 1.0;
        private GivenParameter<double> _length;
        private GivenParameter<double> _width = new GivenParameter<double>(1.0, false);
        private GivenParameter<double> _resistance;

        /// <summary>
        /// The minimum resistance for any resistor.
        /// </summary>
        public const double MinimumResistance = 1e-9;

        /// <summary>
        /// Gets the temperature parameter in degrees Kelvin.
        /// </summary>
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> Temperature
        {
            get => _temperature;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Temperature), 0);
                _temperature = value;
            }
        }

        /// <summary>
        /// The resistance of the resistor. If the resistance is limited to <see cref="MinimumResistance"/> to avoid
        /// numerical instability issues. If a 0 Ohm resistance is wanted, consider using an ideal voltage source instead.
        /// </summary>
        [ParameterName("resistance"), ParameterName("r"), ParameterInfo("Resistance", Units = "\u03a9", IsPrincipal = true)]
        [GreaterThanOrEquals(0), LowerLimit(MinimumResistance)]
        public GivenParameter<double> Resistance
        {
            get => _resistance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Resistance), 0);
                value = Utility.LowerLimit(value, this, nameof(Resistance), MinimumResistance);
                _resistance = value;
            }
        }

        /// <summary>
        /// Instance operating temperature
        /// </summary>
        [ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance operating temperature", Units = "\u00b0C", Interesting = false)]
        [GreaterThan(Constants.CelsiusKelvin)]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Width
        /// </summary>
        [ParameterName("w"), ParameterInfo("Width", Units = "m")]
        [GreaterThan(0)]
        public GivenParameter<double> Width
        {
            get => _width;
            set
            {
                Utility.GreaterThan(value, nameof(Width), 0);
                _width = value;
            }
        }

        /// <summary>
        /// Length
        /// </summary>
        [ParameterName("l"), ParameterInfo("Length", Units = "m")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> Length
        {
            get => _length;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Length), 0);
                _length = value;
            }
        }

        /// <summary>
        /// Parallel multiplier
        /// </summary>
        [ParameterName("m"), ParameterInfo("Parallel multiplier")]
        [GreaterThanOrEquals(0)]
        public double ParallelMultiplier
        {
            get => _parallelMultiplier;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(ParallelMultiplier), 0);
                _parallelMultiplier = value;
            }
        }

        /// <summary>
        /// Series multiplier
        /// </summary>
        [ParameterName("n"), ParameterInfo("Series multiplier")]
        [GreaterThan(0)]
        public double SeriesMultiplier
        {
            get => _seriesMultiplier;
            set
            {
                Utility.GreaterThan(value, nameof(SeriesMultiplier), 0);
                _seriesMultiplier = value;
            }
        }
    }
}