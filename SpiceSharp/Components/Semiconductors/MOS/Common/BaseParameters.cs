using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    /// <summary>
    /// Common parameters for mosfet components.
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
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
        public GivenParameter<double> SourceArea { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the drain layout area.
        /// </summary>
        [ParameterName("ad"), ParameterInfo("Drain area")]
        public GivenParameter<double> DrainArea { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the source layout perimeter.
        /// </summary>
        [ParameterName("ps"), ParameterInfo("Source perimeter")]
        public GivenParameter<double> SourcePerimeter { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the drain layout perimeter.
        /// </summary>
        [ParameterName("pd"), ParameterInfo("Drain perimeter")]
        public GivenParameter<double> DrainPerimeter { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the number of squares of the source.
        /// Used in conjunction with the sheet resistance.
        /// </summary>
        [ParameterName("nrs"), ParameterInfo("Source squares")]
        public GivenParameter<double> SourceSquares { get; } = new GivenParameter<double>(1);

        /// <summary>
        /// Gets the number of squares of the drain.
        /// Used in conjunction with the sheet resistance.
        /// </summary>
        [ParameterName("nrd"), ParameterInfo("Drain squares")]
        public GivenParameter<double> DrainSquares { get; } = new GivenParameter<double>(1);

        /// <summary>
        /// Gets or sets a value indicating whether the device is on or off.
        /// </summary>
        [ParameterName("off"), ParameterInfo("Device initially off")]
        public bool Off { get; set; }

        /// <summary>
        /// Gets the initial bulk-source voltage.
        /// </summary>
        [ParameterName("icvbs"), ParameterInfo("Initial B-S voltage")]
        public GivenParameter<double> InitialVoltageBs { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the initial drain-source voltage.
        /// </summary>
        [ParameterName("icvds"), ParameterInfo("Initial D-S voltage")]
        public GivenParameter<double> InitialVoltageDs { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the initial gate-source voltage.
        /// </summary>
        [ParameterName("icvgs"), ParameterInfo("Initial G-S voltage")]
        public GivenParameter<double> InitialVoltageGs { get; } = new GivenParameter<double>();

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
                    InitialVoltageBs.Value = value[2];
                    goto case 2;
                case 2: InitialVoltageGs.Value = value[1];
                    goto case 1;
                case 1: InitialVoltageDs.Value = value[0];
                    break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
    }
}
