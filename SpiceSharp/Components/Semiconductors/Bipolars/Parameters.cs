using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Bipolars
{
    /// <summary>
    /// Base parameters for a <see cref="BipolarJunctionTransistor"/>.
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    [GeneratedParameters]
    public partial class Parameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the temperature in degrees Celsius.
        /// </summary>
        /// <value>
        /// The temperature in degrees Celsius.
        /// </value>
        [ParameterName("temp"), ParameterInfo("Instance temperature", Units = "\u00b0C")]
        [DerivedProperty(), GreaterThan(Constants.CelsiusKelvin)]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets or sets the temperature parameter in degrees Kelvin.
        /// </summary>
        /// <value>
        /// The temperature in degrees Kelvin.
        /// </value>
        [GreaterThan(0)]
        private GivenParameter<double> _temperature = new GivenParameter<double>(Constants.ReferenceTemperature, false);

        /// <summary>
        /// Gets or sets the area of the transistor.
        /// </summary>
        /// <value>
        /// The area of the transistor.
        /// </value>
        [ParameterName("area"), ParameterInfo("Area factor", Units = "m^2")]
        [GreaterThan(0)]
        private double _area = 1;

        /// <summary>
        /// Gets or sets whether or not the device is initially off (non-conducting).
        /// </summary>
        /// <value>
        ///   <c>true</c> if the device is initially off; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("off"), ParameterInfo("Device initially off")]
        public bool Off { get; set; }

        /// <summary>
        /// Gets the initial base-emitter voltage parameter.
        /// </summary>
        /// <value>
        /// The initial base-emitter voltage.
        /// </value>
        [ParameterName("icvbe"), ParameterInfo("Initial B-E voltage", Units = "V")]
        public GivenParameter<double> InitialVoltageBe { get; set; }

        /// <summary>
        /// Gets the initial collector-emitter voltage parameter.
        /// </summary>
        /// <value>
        /// The initial collector-emitter voltage.
        /// </value>
        [ParameterName("icvce"), ParameterInfo("Initial C-E voltage", Units = "V")]
        public GivenParameter<double> InitialVoltageCe { get; set; }

        /// <summary>
        /// Set initial conditions of the device.
        /// </summary>
        /// <param name="value">The initial voltages (Vce, Vbe) or just (Vbe).</param>
        [ParameterName("ic"), ParameterInfo("Initial condition vector", Units = "V")]
        public void SetIc(double[] value)
        {
            value.ThrowIfNotLength(nameof(value), 1, 2);
            switch (value.Length)
            {
                case 2:
                    InitialVoltageCe = value[1];
                    goto case 1;
                case 1:
                    InitialVoltageBe = value[0];
                    break;
            }
        }
    }
}
