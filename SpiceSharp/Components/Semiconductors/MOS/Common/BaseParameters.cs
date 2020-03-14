using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    /// <summary>
    /// Common parameters for mosfet components.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public abstract class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the temperature in degrees celsius.
        /// </summary>
        [ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance operating temperature")]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature.Value = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the temperature in Kelvin.
        /// </summary>
        public GivenParameter<double> Temperature { get; } = new GivenParameter<double>(Constants.ReferenceTemperature);

        /// <summary>
        /// Gets the mosfet width.
        /// </summary>
        [ParameterName("w"), ParameterInfo("Width")]
        public GivenParameter<double> Width { get; } = new GivenParameter<double>(1e-4);

        /// <summary>
        /// Gets the mosfet length.
        /// </summary>
        [ParameterName("l"), ParameterInfo("Length")]
        public GivenParameter<double> Length { get; } = new GivenParameter<double>(1e-4);

        /// <summary>
        /// Gets the source layout area.
        /// </summary>
        [ParameterName("as"), ParameterInfo("Source area")]
        public double SourceArea { get; set; }

        /// <summary>
        /// Gets the drain layout area.
        /// </summary>
        [ParameterName("ad"), ParameterInfo("Drain area")]
        public double DrainArea { get; set; }

        /// <summary>
        /// Gets the source layout perimeter.
        /// </summary>
        [ParameterName("ps"), ParameterInfo("Source perimeter")]
        public double SourcePerimeter { get; set; }

        /// <summary>
        /// Gets the drain layout perimeter.
        /// </summary>
        [ParameterName("pd"), ParameterInfo("Drain perimeter")]
        public double DrainPerimeter { get; set; }

        /// <summary>
        /// Gets the number of squares of the source.
        /// Used in conjunction with the sheet resistance.
        /// </summary>
        [ParameterName("nrs"), ParameterInfo("Source squares")]
        public double SourceSquares { get; set; } = 1;

        /// <summary>
        /// Gets the number of squares of the drain.
        /// Used in conjunction with the sheet resistance.
        /// </summary>
        [ParameterName("nrd"), ParameterInfo("Drain squares")]
        public double DrainSquares { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether the device is on or off.
        /// </summary>
        [ParameterName("off"), ParameterInfo("Device initially off")]
        public bool Off { get; set; }

        /// <summary>
        /// Gets the initial bulk-source voltage.
        /// </summary>
        [ParameterName("icvbs"), ParameterInfo("Initial B-S voltage")]
        public double InitialVoltageBs { get; set; }

        /// <summary>
        /// Gets the initial drain-source voltage.
        /// </summary>
        [ParameterName("icvds"), ParameterInfo("Initial D-S voltage")]
        public double InitialVoltageDs { get; set; }

        /// <summary>
        /// Gets the initial gate-source voltage.
        /// </summary>
        [ParameterName("icvgs"), ParameterInfo("Initial G-S voltage")]
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
                case 2: InitialVoltageGs = value[1];
                    goto case 1;
                case 1: InitialVoltageDs = value[0];
                    break;
                default:
                    throw new BadParameterException(nameof(value));
            }
        }
    }
}
