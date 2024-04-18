using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Mosfets
{
    /// <summary>
    /// Common parameters for mosfet components.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public partial class Parameters : ParameterSet<Parameters>
    {
        /// <summary>
        /// Gets or sets the temperature in degrees celsius.
        /// </summary>
        /// <value>
        /// The temperature in degrees celsius.
        /// </value>
        [ParameterName("temp"), ParameterInfo("Instance operating temperature", Units = "\u00b0C")]
        [DerivedProperty, GreaterThan(-Constants.CelsiusKelvin), Finite]
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
        [GreaterThan(0), Finite]
        private GivenParameter<double> _temperature = new(Constants.ReferenceTemperature, false);

        /// <summary>
        /// Gets or sets the instance temperature difference.
        /// </summary>
        /// <value>
        /// The temperature difference.
        /// </value>
        [ParameterName("dtemp"), ParameterInfo("The instance temperature difference", Units = "\u00b0C")]
        [Finite]
        private double _deltaTemperature;

        /// <summary>
        /// Gets or sets the mosfet width.
        /// </summary>
        /// <value>
        /// The mosfet width.
        /// </value>
        [ParameterName("w"), ParameterInfo("Width", Units = "m")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _width = new(1e-4, false);

        /// <summary>
        /// Gets or sets the mosfet length.
        /// </summary>
        /// <value>
        /// The mosfet length.
        /// </value>
        [ParameterName("l"), ParameterInfo("Length", Units = "m")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _length = new(1e-4, false);

        /// <summary>
        /// Gets or sets the source layout area.
        /// </summary>
        /// <value>
        /// The source layout area.
        /// </value>
        [ParameterName("as"), ParameterInfo("Source area", Units = "m^2")]
        [GreaterThanOrEquals(0), Finite]
        private double _sourceArea;

        /// <summary>
        /// Gets or sets the drain layout area.
        /// </summary>
        /// <value>
        /// The drain layout area.
        /// </value>
        [ParameterName("ad"), ParameterInfo("Drain area", Units = "m^2")]
        [GreaterThanOrEquals(0), Finite]
        private double _drainArea;

        /// <summary>
        /// Gets or sets the source layout perimeter.
        /// </summary>
        /// <value>
        /// The source layout perimeter.
        /// </value>
        [ParameterName("ps"), ParameterInfo("Source perimeter", Units = "m")]
        [GreaterThanOrEquals(0), Finite]
        private double _sourcePerimeter;

        /// <summary>
        /// Gets or sets the drain layout perimeter.
        /// </summary>
        /// <value>
        /// The drain layout perimeter.
        /// </value>
        [ParameterName("pd"), ParameterInfo("Drain perimeter", Units = "m")]
        [GreaterThanOrEquals(0), Finite]
        private double _drainPerimeter;

        /// <summary>
        /// Gets or sets the number of squares of the source.
        /// Used in conjunction with the sheet resistance.
        /// </summary>
        [ParameterName("nrs"), ParameterInfo("Source squares")]
        [GreaterThanOrEquals(0), Finite]
        private double _sourceSquares = 1;

        /// <summary>
        /// Gets or sets the number of squares of the drain.
        /// Used in conjunction with the sheet resistance.
        /// </summary>
        [ParameterName("nrd"), ParameterInfo("Drain squares")]
        [GreaterThanOrEquals(0), Finite]
        private double _drainSquares = 1;

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
        [Finite]
        private GivenParameter<double> _initialVbs;

        /// <summary>
        /// Gets or sets the initial drain-source voltage.
        /// </summary>
        /// <value>
        /// The initial drain-source voltage.
        /// </value>
        [ParameterName("icvds"), ParameterInfo("Initial D-S voltage", Units = "V")]
        [Finite]
        private GivenParameter<double> _initialVds;

        /// <summary>
        /// Gets or sets the initial gate-source voltage.
        /// </summary>
        /// <value>
        /// The initial gate-source voltage.
        /// </value>
        [ParameterName("icvgs"), ParameterInfo("Initial G-S voltage", Units = "V")]
        [Finite]
        private GivenParameter<double> _initialVgs;

        /// <summary>
        /// Gets or sets the parallel multplier (the number of transistors in parallel).
        /// </summary>
        /// <value>
        /// The parallel multplier.
        /// </value>
        [ParameterName("m"), ParameterInfo("The parallel multiplier")]
        [GreaterThan(0), Finite]
        private double _parallelMultiplier = 1.0;

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
