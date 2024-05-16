using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Diodes
{
    /// <summary>
    /// Base parameters for a <see cref="Diode" />
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public partial class Parameters : ParameterSet<Parameters>
    {
        /// <summary>
        /// Gets or sets the area.
        /// </summary>
        /// <value>
        /// The area of the diode.
        /// </value>
        [ParameterName("area"), ParameterInfo("Area factor", Units = "m^2")]
        [GreaterThanOrEquals(0), Finite]
        private double _area = 1;

        /// <summary>
        /// Gets or sets whether or not the diode is initially off (non-conducting).
        /// </summary>
        /// <value>
        ///   <c>true</c> if the diode is initially off; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("off"), ParameterInfo("Initially off")]
        public bool Off { get; set; }

        /// <summary>
        /// Gets or sets the initial condition.
        /// </summary>
        /// <value>
        /// The initial voltage.
        /// </value>
        [ParameterName("ic"), ParameterInfo("Initial device voltage", Units = "V")]
        [Finite]
        private GivenParameter<double> _initCond;

        /// <summary>
        /// Gets or sets the temperature in degrees Celsius.
        /// </summary>
        /// <value>
        /// The temperature in degrees Celsius.
        /// </value>
        [ParameterName("temp"), ParameterInfo("Instance temperature", Units = "\u00b0C")]
        [DerivedProperty, GreaterThan(-Constants.CelsiusKelvin), Finite]
        public GivenParameter<double> TemperatureCelsius
        {
            get => new(Temperature - Constants.CelsiusKelvin, Temperature.Given);
            set => Temperature = new GivenParameter<double>(value + Constants.CelsiusKelvin, value.Given);
        }

        /// <summary>
        /// Gets the temperature parameter in degrees Kelvin.
        /// </summary>
        /// <value>
        /// The temperature in degrees Kelvin.
        /// </value>
        [GreaterThan(0), Finite]
        private GivenParameter<double> _temperature = new(Constants.ReferenceTemperature, false);

        /// <summary>
        /// Gets or sets the number of diodes in parallel.
        /// </summary>
        /// <value>
        /// The number of diodes in parallel.
        /// </value>
        [ParameterName("m"), ParameterInfo("Parallel multiplier")]
        [GreaterThanOrEquals(0), Finite]
        private double _parallelMultiplier = 1.0;

        /// <summary>
        /// Gets or sets the number of diodes in series.
        /// </summary>
        /// <value>
        /// The number of diodes in series.
        /// </value>
        [ParameterName("n"), ParameterInfo("Series multiplier")]
        [GreaterThan(0), Finite]
        private double _seriesMultiplier = 1.0;
    }
}
