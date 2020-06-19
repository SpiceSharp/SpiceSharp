using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.JFETs
{
    /// <summary>
    /// Base parameters for a <see cref="JFETModel" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public class ModelParameters : ParameterSet
    {
        private double _b = 1;
        private double _depletionCapCoefficient = 0.5;
        private double _gateSaturationCurrent = 1e-14;
        private double _gatePotential = 1;
        private double _capGd;
        private double _capGs;
        private double _sourceResistance;
        private double _drainResistance;
        private double _lModulation;
        private double _beta = 1e-4;
        private GivenParameter<double> _nominalTemperature = new GivenParameter<double>(300.15, false);
        private double _nominalTemperatureCelsius;

        /// <summary>
        /// Gets or sets the measurement temperature in degrees celsius.
        /// </summary>
        /// <value>
        /// The measurement temperature in degrees celsius.
        /// </value>
        [ParameterName("tnom"), ParameterInfo("Nominal temperature.", Units = "\u00b0C")]
        [GreaterThan(Constants.CelsiusKelvin)]
        public double NominalTemperatureCelsius
        {
            get => _nominalTemperatureCelsius;
            set
            {
                Utility.GreaterThan(value, nameof(NominalTemperatureCelsius), Constants.CelsiusKelvin);
                _nominalTemperatureCelsius = value;
            }
        }

        /// <summary>
        /// Gets the measurement temperature in Kelvin.
        /// </summary>
        /// <value>
        /// The measurement temperature in Kelvin.
        /// </value>
        [GreaterThan(0)]
        public GivenParameter<double> NominalTemperature
        {
            get => _nominalTemperature;
            set
            {
                Utility.GreaterThan(value, nameof(NominalTemperature), 0);
                _nominalTemperature = value;
            }
        }

        /// <summary>
        /// Gets or sets the threshold voltage.
        /// </summary>
        /// <value>
        /// The threshold voltage.
        /// </value>
        [ParameterName("vt0"), ParameterName("vto"), ParameterInfo("Threshold voltage", Units = "V")]
        public double Threshold { get; set; } = -2;

        /// <summary>
        /// Gets or sets the transconductance.
        /// </summary>
        /// <value>
        /// The transconductance.
        /// </value>
        [ParameterName("beta"), ParameterInfo("Transconductance parameter", Units = "\u03a9^-1")]
        [GreaterThanOrEquals(0)]
        public double Beta
        {
            get => _beta;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Beta), 0);
                _beta = value;
            }
        }

        /// <summary>
        /// Gets or sets the channel length modulation parameter.
        /// </summary>
        /// <value>
        /// The channel length modulation parameter.
        /// </value>
        [ParameterName("lambda"), ParameterInfo("Channel length modulation parameter", Units = "V^-1")]
        [GreaterThanOrEquals(0)]
        public double LModulation
        {
            get => _lModulation;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(LModulation), 0);
                _lModulation = value;
            }
        }

        /// <summary>
        /// Gets or sets the drain resistance.
        /// </summary>
        /// <value>
        /// The drain resistance.
        /// </value>
        [ParameterName("rd"), ParameterInfo("Drain ohmic resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0)]
        public double DrainResistance
        {
            get => _drainResistance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(DrainResistance), 0);
                _drainResistance = value;
            }
        }

        /// <summary>
        /// Gets or sets the source resistance.
        /// </summary>
        /// <value>
        /// The source resistance.
        /// </value>
        [ParameterName("rs"), ParameterInfo("Source ohmic resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0)]
        public double SourceResistance
        {
            get => _sourceResistance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(SourceResistance), 0);
                _sourceResistance = value;
            }
        }

        /// <summary>
        /// Gets or sets the gate-source junction capacitance.
        /// </summary>
        /// <value>
        /// The gate-source junction capacitance.
        /// </value>
        [ParameterName("cgs"), ParameterInfo("G-S junction capacitance", Units = "F")]
        [GreaterThanOrEquals(0)]
        public double CapGs
        {
            get => _capGs;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(CapGs), 0);
                _capGs = value;
            }
        }

        /// <summary>
        /// Gets or sets the gate-drain junction capacitance.
        /// </summary>
        /// <value>
        /// The gate-drain junction capacitance.
        /// </value>
        [ParameterName("cgd"), ParameterInfo("G-D junction capacitance", Units = "F")]
        [GreaterThanOrEquals(0)]
        public double CapGd
        {
            get => _capGd;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(CapGd), 0);
                _capGd = value;
            }
        }

        /// <summary>
        /// Gets or sets the gate junction potential.
        /// </summary>
        /// <value>
        /// The gate junction potential.
        /// </value>
        [ParameterName("pb"), ParameterInfo("Gate junction potential", Units = "V")]
        [GreaterThan(0)]
        public double GatePotential
        {
            get => _gatePotential;
            set
            {
                Utility.GreaterThan(value, nameof(GatePotential), 0);
                _gatePotential = value;
            }
        }

        /// <summary>
        /// Gets or sets the gate saturation current.
        /// </summary>
        /// <value>
        /// The gate saturation current.
        /// </value>
        [ParameterName("is"), ParameterInfo("Gate junction saturation current", Units = "A")]
        [GreaterThan(0)]
        public double GateSaturationCurrent
        {
            get => _gateSaturationCurrent;
            set
            {
                Utility.GreaterThan(value, nameof(GateSaturationCurrent), 0);
                _gateSaturationCurrent = value;
            }
        }

        /// <summary>
        /// Gets or sets the forward bias junction fitting parameter.
        /// </summary>
        /// <value>
        /// he forward bias junction fitting parameter.
        /// </value>
        [ParameterName("fc"), ParameterInfo("Forward bias junction fitting parameter")]
        [GreaterThan(0), UpperLimit(0.95)]
        public double DepletionCapCoefficient
        {
            get => _depletionCapCoefficient;
            set
            {
                Utility.GreaterThan(value, nameof(DepletionCapCoefficient), 0);
                value = Utility.UpperLimit(value, this, nameof(DepletionCapCoefficient), 0.95);
                _depletionCapCoefficient = value;
            }
        }

        /// <summary>
        /// Gets the doping tail parameter.
        /// </summary>
        /// <value>
        /// The doping tail parameter.
        /// </value>
        [ParameterName("b"), ParameterInfo("Doping tail parameter")]
        [GreaterThanOrEquals(0)]
        public double B
        {
            get => _b;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(B), 0);
                _b = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the JFET.
        /// </summary>
        /// <value>
        /// The type of the JFET.
        /// </value>
        public double JFETType { get; protected set; } = 1.0;

        /// <summary>
        /// Gets the drain conductance.
        /// </summary>
        /// <value>
        /// The drain conductance.
        /// </value>
        public double DrainConductance { get; private set; }

        /// <summary>
        /// Gets the source conductance.
        /// </summary>
        /// <value>
        /// The source conductance.
        /// </value>
        public double SourceConductance { get; private set; }

        /// <summary>
        /// Sets the model to be n-type.
        /// </summary>
        /// <param name="flag">if set to <c>true</c> n-type is set.</param>
        [ParameterName("njf"), ParameterInfo("N type JFET model")]
        public void SetNjf(bool flag)
        {
            if (flag)
                JFETType = 1.0;
        }

        /// <summary>
        /// Sets the model to be p-type.
        /// </summary>
        /// <param name="flag">if set to <c>true</c> p-type is set.</param>
        [ParameterName("pjf"), ParameterInfo("P type JFET model")]
        public void SetPjf(bool flag)
        {
            if (flag)
                JFETType = -1.0;
        }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <value>
        /// The name of the type.
        /// </value>
        [ParameterName("type"), ParameterInfo("N-type or P-type JFET model")]
        public string TypeName
        {
            get
            {
                if (JFETType > 0.0)
                    return "njf";
                return "pjf";
            }
        }

        /// <summary>
        /// Gets or sets the flicker noise coefficient.
        /// </summary>
        /// <value>
        /// The flicker noise coefficient.
        /// </value>
        [ParameterName("kf"), ParameterInfo("Flicker noise coefficient")]
        public double FnCoefficient { get; set; }

        /// <summary>
        /// Gets or sets the flicker noise exponent.
        /// </summary>
        /// <value>
        /// The flicker noise exponent.
        /// </value>
        [ParameterName("af"), ParameterInfo("Flicker noise exponent")]
        public double FnExponent { get; set; } = 1;

        /// <inheritdoc/>
        public void CalculateDefaults()
        {
            DrainConductance = DrainResistance > 0 ? 1 / DrainResistance : 0;
            SourceConductance = SourceResistance > 0 ? 1 / SourceResistance : 0;
        }
    }
}
