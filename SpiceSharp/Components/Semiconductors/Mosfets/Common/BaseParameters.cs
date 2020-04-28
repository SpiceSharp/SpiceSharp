using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    /// <summary>
    /// Common parameters for mosfet components.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public abstract class BaseParameters : ParameterSet
    {
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
        [ParameterName("temp"), DerivedProperty(), ParameterInfo("Instance operating temperature", Units = "\u00b0C")]
        [GreaterThan(Constants.CelsiusKelvin)]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the temperature in Kelvin.
        /// </summary>
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
        /// Gets the mosfet width.
        /// </summary>
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
        /// Gets the mosfet length.
        /// </summary>
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
        /// Gets the source layout area.
        /// </summary>
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
        /// Gets the drain layout area.
        /// </summary>
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
        /// Gets the source layout perimeter.
        /// </summary>
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
        /// Gets the drain layout perimeter.
        /// </summary>
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
        /// Gets the number of squares of the source.
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
        /// Gets the number of squares of the drain.
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
        /// Gets the initial bulk-source voltage.
        /// </summary>
        [ParameterName("icvbs"), ParameterInfo("Initial B-S voltage", Units = "V")]
        public double InitialVoltageBs { get; set; }

        /// <summary>
        /// Gets the initial drain-source voltage.
        /// </summary>
        [ParameterName("icvds"), ParameterInfo("Initial D-S voltage", Units = "V")]
        public double InitialVoltageDs { get; set; }

        /// <summary>
        /// Gets the initial gate-source voltage.
        /// </summary>
        [ParameterName("icvgs"), ParameterInfo("Initial G-S voltage", Units = "V")]
        public double InitialVoltageGs { get; set; }

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
                    InitialVoltageBs = ic[2];
                    goto case 2;
                case 2:
                    InitialVoltageGs = ic[1];
                    goto case 1;
                case 1:
                    InitialVoltageDs = ic[0];
                    break;
            }
        }
    }
}
