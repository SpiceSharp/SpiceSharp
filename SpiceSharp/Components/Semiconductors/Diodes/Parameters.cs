using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.Diodes
{
    /// <summary>
    /// Base parameters for a <see cref="Diode" />
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public class Parameters : ParameterSet
    {
        private double _seriesMultiplier = 1.0;
        private double _parallelMultiplier = 1.0;
        private GivenParameter<double> _temperature = new GivenParameter<double>(Constants.ReferenceTemperature, false);
        private double _area = 1;

        /// <summary>
        /// Gets or sets the area.
        /// </summary>
        /// <value>
        /// The area of the diode.
        /// </value>
        [ParameterName("area"), ParameterInfo("Area factor", Units = "m^2")]
        [GreaterThanOrEquals(0)]
        public double Area
        {
            get => _area;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Area), 0);
                _area = value;
            }
        }

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
        public GivenParameter<double> InitCond { get; set; }

        /// <summary>
        /// Gets or sets the temperature in degrees Celsius.
        /// </summary>
        /// <value>
        /// The temperature in degrees Celsius.
        /// </value>
        [ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance temperature", Units = "\u00b0C")]
        [GreaterThan(Constants.CelsiusKelvin)]
        public GivenParameter<double> TemperatureCelsius
        {
            get => new GivenParameter<double>(Temperature - Constants.CelsiusKelvin, Temperature.Given);
            set => Temperature = new GivenParameter<double>(value + Constants.CelsiusKelvin, value.Given);
        }

        /// <summary>
        /// Gets the temperature parameter in degrees Kelvin.
        /// </summary>
        /// <value>
        /// The temperature in degrees Kelvin.
        /// </value>
        [GreaterThan(0)]
        public GivenParameter<double> Temperature
        {
            get => _temperature;
            set
            {
                Utility.GreaterThan(value, nameof(Temperature), 0);
                _temperature = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of diodes in parallel.
        /// </summary>
        /// <value>
        /// The number of diodes in parallel.
        /// </value>
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
        /// Gets or sets the number of diodes in series.
        /// </summary>
        /// <value>
        /// The number of diodes in series.
        /// </value>
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
