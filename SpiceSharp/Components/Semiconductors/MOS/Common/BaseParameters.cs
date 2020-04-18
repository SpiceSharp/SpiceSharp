using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    /// <summary>
    /// Common parameters for mosfet components.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public abstract class BaseParameters : ParameterSet
    {
        private double _drainSquares = 1;
        private double _sourceSquares = 1;
        private double _drainPerimeter;
        private double _sourcePerimeter;
        private double _drainArea;
        private double _sourceArea;
        private GivenParameter<double> _length = new GivenParameter<double>(1e-4, false);
        private GivenParameter<double> _width = new GivenParameter<double>(1e-4, false);
        private GivenParameter<double> _temperature = new GivenParameter<double>(Constants.ReferenceTemperature, false);

        /// <summary>
        /// Gets or sets the temperature in degrees celsius.
        /// </summary>
        [ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance operating temperature", Units = "\u00b0C")]
        [GreaterThan(Constants.CelsiusKelvin)]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the temperature in Kelvin.
        /// </summary>
        [GreaterThan(0)]
        public GivenParameter<double> Temperature
        {
            get => _temperature;
            set
            {
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(Temperature), value, 0));
                _temperature = value;
            }
        }

        /// <summary>
        /// Gets the mosfet width.
        /// </summary>
        [ParameterName("w"), ParameterInfo("Width", Units = "m")]
        [GreaterThan(0)]
        public GivenParameter<double> Width
        {
            get => _width;
            set
            {
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(Width), value, 0));
                _width = value;
            }
        }

        /// <summary>
        /// Gets the mosfet length.
        /// </summary>
        [ParameterName("l"), ParameterInfo("Length", Units = "m")]
        [GreaterThan(0)]
        public GivenParameter<double> Length
        {
            get => _length;
            set
            {
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(Length), value, 0));
                _length = value;
            }
        }

        /// <summary>
        /// Gets the source layout area.
        /// </summary>
        [ParameterName("as"), ParameterInfo("Source area", Units = "m^2")]
        [GreaterThanOrEquals(0)]
        public double SourceArea
        {
            get => _sourceArea;
            set
            {
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(SourceArea), value, 0));
                _sourceArea = value;
            }
        }

        /// <summary>
        /// Gets the drain layout area.
        /// </summary>
        [ParameterName("ad"), ParameterInfo("Drain area", Units = "m^2")]
        [GreaterThanOrEquals(0)]
        public double DrainArea
        {
            get => _drainArea;
            set
            {
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(DrainArea), value, 0));
                _drainArea = value;
            }
        }

        /// <summary>
        /// Gets the source layout perimeter.
        /// </summary>
        [ParameterName("ps"), ParameterInfo("Source perimeter", Units = "m")]
        [GreaterThanOrEquals(0)]
        public double SourcePerimeter
        {
            get => _sourcePerimeter;
            set
            {
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(SourcePerimeter), value, 0));
                _sourcePerimeter = value;
            }
        }

        /// <summary>
        /// Gets the drain layout perimeter.
        /// </summary>
        [ParameterName("pd"), ParameterInfo("Drain perimeter", Units = "m")]
        [GreaterThanOrEquals(0)]
        public double DrainPerimeter
        {
            get => _drainPerimeter;
            set
            {
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(DrainPerimeter), value, 0));
                _drainPerimeter = value;
            }
        }

        /// <summary>
        /// Gets the number of squares of the source.
        /// Used in conjunction with the sheet resistance.
        /// </summary>
        [ParameterName("nrs"), ParameterInfo("Source squares")]
        [GreaterThanOrEquals(0)]
        public double SourceSquares
        {
            get => _sourceSquares;
            set
            {
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(SourceSquares), value, 0));
                _sourceSquares = value;
            }
        }

        /// <summary>
        /// Gets the number of squares of the drain.
        /// Used in conjunction with the sheet resistance.
        /// </summary>
        [ParameterName("nrd"), ParameterInfo("Drain squares")]
        [GreaterThanOrEquals(0)]
        public double DrainSquares
        {
            get => _drainSquares;
            set
            {
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(DrainSquares), value, 0));
                _drainSquares = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the device is on or off.
        /// </summary>
        [ParameterName("off"), ParameterInfo("Device initially off")]
        public bool Off { get; set; }

        /// <summary>
        /// Gets the initial bulk-source voltage.
        /// </summary>
        [ParameterName("icvbs"), ParameterInfo("Initial B-S voltage", Units = "V")]
        public double InitialVoltageBs { get; set; }

        /// <summary>
        /// Gets the initial drain-source voltage.
        /// </summary>
        [ParameterName("icvds"), ParameterInfo("Initial D-S voltage", Units = "V")]
        public double InitialVoltageDs { get; set; }

        /// <summary>
        /// Gets the initial gate-source voltage.
        /// </summary>
        [ParameterName("icvgs"), ParameterInfo("Initial G-S voltage", Units = "V")]
        public double InitialVoltageGs { get; set; }

        /// <summary>
        /// Set the initial conditions of the device.
        /// </summary>
        [ParameterName("ic"), ParameterInfo("Vector of D-S, G-S, B-S voltages")]
        public void SetIc(double[] value)
        {
            value.ThrowIfNull(nameof(value));

            switch (value.Length)
            {
                case 3:
                    InitialVoltageBs = value[2];
                    goto case 2;
                case 2:
                    InitialVoltageGs = value[1];
                    goto case 1;
                case 1:
                    InitialVoltageDs = value[0];
                    break;
                default:
                    throw new BadParameterException(nameof(value));
            }
        }
    }
}
