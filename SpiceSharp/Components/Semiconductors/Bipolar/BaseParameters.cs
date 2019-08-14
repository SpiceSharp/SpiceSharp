using SpiceSharp.Attributes;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the temperature in degrees Celsius.
        /// </summary>
        [ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance temperature")]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature.Value = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the temperature parameter in degrees Kelvin.
        /// </summary>
        public GivenParameter<double> Temperature { get; } = new GivenParameter<double>(Constants.ReferenceTemperature);

        /// <summary>
        /// Gets the area parameter.
        /// </summary>
        [ParameterName("area"), ParameterInfo("Area factor")]
        public GivenParameter<double> Area { get; } = new GivenParameter<double>(1);

        /// <summary>
        /// Gets or sets whether or not the device is initially off (non-conducting).
        /// </summary>
        [ParameterName("off"), ParameterInfo("Device initially off")]
        public bool Off { get; set; }

        /// <summary>
        /// Gets the initial base-emitter voltage parameter.
        /// </summary>
        [ParameterName("icvbe"), ParameterInfo("Initial B-E voltage")]
        public GivenParameter<double> InitialVoltageBe { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the initial collector-emitter voltage parameter.
        /// </summary>
        [ParameterName("icvce"), ParameterInfo("Initial C-E voltage")]
        public GivenParameter<double> InitialVoltageCe { get; } = new GivenParameter<double>();

        /// <summary>
        /// Set initial conditions of the device.
        /// </summary>
        /// <param name="value">The initial voltages (Vce, Vbe) or just (Vbe).</param>
        [ParameterName("ic"), ParameterInfo("Initial condition vector")]
        public void SetIc(double[] value)
        {
            value.ThrowIfNull(nameof(value));

            switch (value.Length)
            {
                case 2:
                    InitialVoltageCe.Value = value[1];
                    goto case 1;
                case 1:
                    InitialVoltageBe.Value = value[0];
                    break;
                default:
                    throw new CircuitException("Bad parameter");
            }
        }
    }
}
