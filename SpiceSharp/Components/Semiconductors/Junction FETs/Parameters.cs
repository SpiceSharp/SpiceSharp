using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.JFETs
{
    /// <summary>
    /// Base parameters for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public partial class Parameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the temperature in degrees celsius.
        /// </summary>
        /// <value>
        /// The temperature in degrees Celsius.
        /// </value>
        [ParameterName("temp"), ParameterInfo("Instance temperature", Units = "\u00b0C")]
        [GreaterThan(Constants.CelsiusKelvin)]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets or sets the temperature in Kelvin.
        /// </summary>
        /// <value>
        /// The temperature in degrees Kelvin.
        /// </value>
        [GreaterThan(0)]
        private GivenParameter<double> _temperature = new GivenParameter<double>(300.15, false);

        /// <summary>
        /// Gets or sets the area.
        /// </summary>
        /// <value>
        /// The area.
        /// </value>
        [ParameterName("area"), ParameterInfo("Area factor", Units = "m^2")]
        [GreaterThanOrEquals(0)]
        private double _area = 1;

        /// <summary>
        /// Gets or sets the initial drain-source voltage.
        /// </summary>
        /// <value>
        /// The initial drain-source voltage.
        /// </value>
        [ParameterName("ic-vds"), ParameterInfo("Initial D-S voltage", Units = "V")]
        public GivenParameter<double> InitialVds { get; set; }

        /// <summary>
        /// Gets or sets the initial gate-source voltage.
        /// </summary>
        /// <value>
        /// The initial gate-source voltage.
        /// </value>
        [ParameterName("ic-vgs"), ParameterInfo("Initial G-S voltage", Units = "V")]
        public GivenParameter<double> InitialVgs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is off.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the JFET is initially off; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("off"), ParameterInfo("Device initially off")]
        public bool Off { get; set; }

        /// <summary>
        /// Sets the initial conditions of the JFET.
        /// </summary>
        /// <param name="ic">The initial conditions.</param>
        [ParameterName("ic"), ParameterInfo("Initial VDS,VGS vector")]
        public void SetIc(double[] ic)
        {
            ic.ThrowIfNotLength(nameof(ic), 1, 2);
            switch (ic.Length)
            {
                case 2:
                    InitialVgs = ic[1];
                    goto case 1;
                case 1:
                    InitialVds = ic[0];
                    break;
            }
        }
    }
}
