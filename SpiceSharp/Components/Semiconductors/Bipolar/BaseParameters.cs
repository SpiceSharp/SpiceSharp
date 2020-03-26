using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

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
        [ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance temperature", Units = "\u00b0C")]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the temperature parameter in degrees Kelvin.
        /// </summary>
        public GivenParameter<double> Temperature { get; set; } = new GivenParameter<double>(Constants.ReferenceTemperature, false);

        /// <summary>
        /// Gets the area parameter.
        /// </summary>
        [ParameterName("area"), ParameterInfo("Area factor", Units = "m^2")]
        public double Area { get; set; } = 1;

        /// <summary>
        /// Gets or sets whether or not the device is initially off (non-conducting).
        /// </summary>
        [ParameterName("off"), ParameterInfo("Device initially off")]
        public bool Off { get; set; }

        /// <summary>
        /// Gets the initial base-emitter voltage parameter.
        /// </summary>
        [ParameterName("icvbe"), ParameterInfo("Initial B-E voltage", Units = "V")]
        public double InitialVoltageBe { get; set; }

        /// <summary>
        /// Gets the initial collector-emitter voltage parameter.
        /// </summary>
        [ParameterName("icvce"), ParameterInfo("Initial C-E voltage", Units = "V")]
        public double InitialVoltageCe { get; set; }

        /// <summary>
        /// Set initial conditions of the device.
        /// </summary>
        /// <param name="value">The initial voltages (Vce, Vbe) or just (Vbe).</param>
        [ParameterName("ic"), ParameterInfo("Initial condition vector", Units = "V")]
        public void SetIc(double[] value)
        {
            value.ThrowIfNull(nameof(value));

            switch (value.Length)
            {
                case 2:
                    InitialVoltageCe = value[1];
                    goto case 1;
                case 1:
                    InitialVoltageBe = value[0];
                    break;
                default:
                    throw new BadParameterException(nameof(value));
            }
        }
    }
}
