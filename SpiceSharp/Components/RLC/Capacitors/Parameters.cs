using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.Capacitors
{
    /// <summary>
    /// Base parameters for a <see cref="Capacitor"/>.
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    [GeneratedParameters]
    public class Parameters : ParameterSet
    {
        private GivenParameter<double> _temperature = new GivenParameter<double>(Constants.ReferenceTemperature, false);
        private double _parallelMultiplier = 1.0;
        private GivenParameter<double> _length = new GivenParameter<double>();
        private GivenParameter<double> _width = new GivenParameter<double>();
        private GivenParameter<double> _capacitance = new GivenParameter<double>();

        /// <summary>
        /// Gets the capacitance parameter.
        /// </summary>
        /// <value>
        /// The capacitance.
        /// </value>
        [ParameterName("capacitance"), ParameterInfo("Device capacitance", Units = "F", IsPrincipal = true)]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> Capacitance
        {
            get => _capacitance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Capacitance), 0);
                _capacitance = value;
            }
        }

        /// <summary>
        /// Gets the initial voltage parameter.
        /// </summary>
        /// <value>
        /// The initial voltage.
        /// </value>
        [ParameterName("ic"), ParameterInfo("Initial capacitor voltage", Units = "V", Interesting = false)]
        public double InitialCondition { get; set; }

        /// <summary>
        /// Gets the width of the capacitor.
        /// </summary>
        /// <value>
        /// The width of the capacitor.
        /// </value>
        [ParameterName("w"), ParameterInfo("Device width", Units = "m", Interesting = false)]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> Width
        {
            get => _width;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Width), 0);
                _width = value;
            }
        }

        /// <summary>
        /// Gets the length of the capacitor.
        /// </summary>
        /// <value>
        /// The length of the capacitor.
        /// </value>
        [ParameterName("l"), ParameterInfo("Device length", Units = "m", Interesting = false)]
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
        /// Gets or sets the parallel multiplier.
        /// </summary>
        /// <value>
        /// The parallel multiplier.
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
        /// Gets or sets the temperature in degrees Celsius.
        /// </summary>
        /// <value>
        /// The temperature celsius.
        /// </value>
        [ParameterName("temp"), ParameterInfo("Instance operating temperature", Units = "\u00b0C", Interesting = false)]
        [DerivedProperty(), GreaterThan(Constants.CelsiusKelvin)]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the temperature parameter (in degrees Kelvin).
        /// </summary>
        /// <value>
        /// The temperature in Kelvin.
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
    }
}
