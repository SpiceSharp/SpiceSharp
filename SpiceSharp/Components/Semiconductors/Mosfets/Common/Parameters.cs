using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.Mosfets
{
    /// <summary>
    /// Common parameters for mosfet components.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public class Parameters : ParameterSet
    {
        private double _parallelMultiplier = 1.0;
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
        /// <value>
        /// The temperature in degrees celsius.
        /// </value>
        [ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance operating temperature", Units = "\u00b0C")]
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

        /// <summary>
        /// Gets or sets the instance temperature difference.
        /// </summary>
        /// <value>
        /// The temperature difference.
        /// </value>
        [ParameterName("dtemp"), ParameterInfo("The instance temperature difference", Units = "\u00b0C")]
        public double DeltaTemperature { get; set; }

        /// <summary>
        /// Gets or sets the mosfet width.
        /// </summary>
        /// <value>
        /// The mosfet width.
        /// </value>
        [ParameterName("w"), ParameterInfo("Width", Units = "m")]
        [GreaterThan(0)]
        public GivenParameter<double> Width
        {
            get => _width;
            set
            {
                Utility.GreaterThan(value, nameof(Width), 0);
                _width = value;
            }
        }

        /// <summary>
        /// Gets or sets the mosfet length.
        /// </summary>
        /// <value>
        /// The mosfet length.
        /// </value>
        [ParameterName("l"), ParameterInfo("Length", Units = "m")]
        [GreaterThan(0)]
        public GivenParameter<double> Length
        {
            get => _length;
            set
            {
                Utility.GreaterThan(value, nameof(Length), 0);
                _length = value;
            }
        }

        /// <summary>
        /// Gets or sets the source layout area.
        /// </summary>
        /// <value>
        /// The source layout area.
        /// </value>
        [ParameterName("as"), ParameterInfo("Source area", Units = "m^2")]
        [GreaterThanOrEquals(0)]
        public double SourceArea
        {
            get => _sourceArea;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(SourceArea), 0);
                _sourceArea = value;
            }
        }

        /// <summary>
        /// Gets or sets the drain layout area.
        /// </summary>
        /// <value>
        /// The drain layout area.
        /// </value>
        [ParameterName("ad"), ParameterInfo("Drain area", Units = "m^2")]
        [GreaterThanOrEquals(0)]
        public double DrainArea
        {
            get => _drainArea;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(DrainArea), 0);
                _drainArea = value;
            }
        }

        /// <summary>
        /// Gets or sets the source layout perimeter.
        /// </summary>
        /// <value>
        /// The source layout perimeter.
        /// </value>
        [ParameterName("ps"), ParameterInfo("Source perimeter", Units = "m")]
        [GreaterThanOrEquals(0)]
        public double SourcePerimeter
        {
            get => _sourcePerimeter;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(SourcePerimeter), 0);
                _sourcePerimeter = value;
            }
        }

        /// <summary>
        /// Gets or sets the drain layout perimeter.
        /// </summary>
        /// <value>
        /// The drain layout perimeter.
        /// </value>
        [ParameterName("pd"), ParameterInfo("Drain perimeter", Units = "m")]
        [GreaterThanOrEquals(0)]
        public double DrainPerimeter
        {
            get => _drainPerimeter;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(DrainPerimeter), 0);
                _drainPerimeter = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of squares of the source.
        /// Used in conjunction with the sheet resistance.
        /// </summary>
        [ParameterName("nrs"), ParameterInfo("Source squares")]
        [GreaterThanOrEquals(0)]
        public double SourceSquares
        {
            get => _sourceSquares;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(SourceSquares), 0);
                _sourceSquares = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of squares of the drain.
        /// Used in conjunction with the sheet resistance.
        /// </summary>
        [ParameterName("nrd"), ParameterInfo("Drain squares")]
        [GreaterThanOrEquals(0)]
        public double DrainSquares
        {
            get => _drainSquares;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(DrainSquares), 0);
                _drainSquares = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the device is on or off.
        /// </summary>
        [ParameterName("off"), ParameterInfo("Device initially off")]
        public bool Off { get; set; }

        /// <summary>
        /// Gets or sets the initial bulk-source voltage.
        /// </summary>
        /// <value>
        /// The initial bulk-source voltage.
        /// </value>
        [ParameterName("icvbs"), ParameterInfo("Initial B-S voltage", Units = "V")]
        public double InitialVbs { get; set; }

        /// <summary>
        /// Gets or sets the initial drain-source voltage.
        /// </summary>
        /// <value>
        /// The initial drain-source voltage.
        /// </value>
        [ParameterName("icvds"), ParameterInfo("Initial D-S voltage", Units = "V")]
        public double InitialVds { get; set; }

        /// <summary>
        /// Gets or sets the initial gate-source voltage.
        /// </summary>
        /// <value>
        /// The initial gate-source voltage.
        /// </value>
        [ParameterName("icvgs"), ParameterInfo("Initial G-S voltage", Units = "V")]
        public double InitialVgs { get; set; }

        /// <summary>
        /// Gets or sets the parallel multplier (the number of transistors in parallel).
        /// </summary>
        /// <value>
        /// The parallel multplier.
        /// </value>
        [GreaterThan(0)]
        public double ParallelMultiplier
        {
            get => _parallelMultiplier;
            set
            {
                Utility.GreaterThan(value, nameof(ParallelMultiplier), 0);
                _parallelMultiplier = value;
            }
        }

        /// <summary>
        /// Set the initial conditions of the device.
        /// </summary>
        [ParameterName("ic"), ParameterInfo("Vector of D-S, G-S, B-S voltages")]
        public void SetIc(double[] ic)
        {
            ic.ThrowIfNotLength(nameof(ic), 1, 3);
            switch (ic.Length)
            {
                case 3:
                    InitialVbs = ic[2];
                    goto case 2;
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
