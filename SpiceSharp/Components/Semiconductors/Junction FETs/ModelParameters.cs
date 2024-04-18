using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.JFETs
{
    /// <summary>
    /// Base parameters for a <see cref="JFETModel" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public partial class ModelParameters : ParameterSet<ModelParameters>
    {
        /// <summary>
        /// Gets or sets the measurement temperature in degrees celsius.
        /// </summary>
        /// <value>
        /// The measurement temperature in degrees celsius.
        /// </value>
        [ParameterName("tnom"), ParameterInfo("Nominal temperature.", Units = "\u00b0C")]
        [DerivedProperty, GreaterThan(-Constants.CelsiusKelvin), Finite]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the measurement temperature in Kelvin.
        /// </summary>
        /// <value>
        /// The measurement temperature in Kelvin.
        /// </value>
        [GreaterThan(0), Finite]
        private GivenParameter<double> _nominalTemperature = new(300.15, false);

        /// <summary>
        /// Gets or sets the threshold voltage.
        /// </summary>
        /// <value>
        /// The threshold voltage.
        /// </value>
        [ParameterName("vt0"), ParameterName("vto"), ParameterInfo("Threshold voltage", Units = "V")]
        [Finite]
        private double _threshold = -2;

        /// <summary>
        /// Gets or sets the transconductance.
        /// </summary>
        /// <value>
        /// The transconductance.
        /// </value>
        [ParameterName("beta"), ParameterInfo("Transconductance parameter", Units = "\u03a9^-1")]
        [GreaterThanOrEquals(0), Finite]
        private double _beta = 1e-4;

        /// <summary>
        /// Gets or sets the channel length modulation parameter.
        /// </summary>
        /// <value>
        /// The channel length modulation parameter.
        /// </value>
        [ParameterName("lambda"), ParameterInfo("Channel length modulation parameter", Units = "V^-1")]
        [GreaterThanOrEquals(0), Finite]
        private double _lModulation;

        /// <summary>
        /// Gets or sets the drain resistance.
        /// </summary>
        /// <value>
        /// The drain resistance.
        /// </value>
        [ParameterName("rd"), ParameterInfo("Drain ohmic resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0), Finite]
        private double _drainResistance;

        /// <summary>
        /// Gets or sets the source resistance.
        /// </summary>
        /// <value>
        /// The source resistance.
        /// </value>
        [ParameterName("rs"), ParameterInfo("Source ohmic resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0), Finite]
        private double _sourceResistance;

        /// <summary>
        /// Gets or sets the gate-source junction capacitance.
        /// </summary>
        /// <value>
        /// The gate-source junction capacitance.
        /// </value>
        [ParameterName("cgs"), ParameterInfo("G-S junction capacitance", Units = "F")]
        [GreaterThanOrEquals(0), Finite]
        private double _capGs;

        /// <summary>
        /// Gets or sets the gate-drain junction capacitance.
        /// </summary>
        /// <value>
        /// The gate-drain junction capacitance.
        /// </value>
        [ParameterName("cgd"), ParameterInfo("G-D junction capacitance", Units = "F")]
        [GreaterThanOrEquals(0), Finite]
        private double _capGd;

        /// <summary>
        /// Gets or sets the gate junction potential.
        /// </summary>
        /// <value>
        /// The gate junction potential.
        /// </value>
        [ParameterName("pb"), ParameterInfo("Gate junction potential", Units = "V")]
        [GreaterThan(0), Finite]
        private double _gatePotential = 1;

        /// <summary>
        /// Gets or sets the gate saturation current.
        /// </summary>
        /// <value>
        /// The gate saturation current.
        /// </value>
        [ParameterName("is"), ParameterInfo("Gate junction saturation current", Units = "A")]
        [GreaterThan(0), Finite]
        private double _gateSaturationCurrent = 1e-14;

        /// <summary>
        /// Gets or sets the forward bias junction fitting parameter.
        /// </summary>
        /// <value>
        /// he forward bias junction fitting parameter.
        /// </value>
        [ParameterName("fc"), ParameterInfo("Forward bias junction fitting parameter")]
        [GreaterThan(0), UpperLimit(0.95)]
        private double _depletionCapCoefficient = 0.5;

        /// <summary>
        /// Gets the doping tail parameter.
        /// </summary>
        /// <value>
        /// The doping tail parameter.
        /// </value>
        [ParameterName("b"), ParameterInfo("Doping tail parameter")]
        [GreaterThanOrEquals(0), Finite]
        private double _b = 1;

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
        [Finite]
        private double _fnCoefficient;

        /// <summary>
        /// Gets or sets the flicker noise exponent.
        /// </summary>
        /// <value>
        /// The flicker noise exponent.
        /// </value>
        [ParameterName("af"), ParameterInfo("Flicker noise exponent")]
        [Finite]
        private double _fnExponent = 1;

        /// <inheritdoc/>
        public void CalculateDefaults()
        {
            DrainConductance = DrainResistance > 0 ? 1 / DrainResistance : 0;
            SourceConductance = SourceResistance > 0 ? 1 / SourceResistance : 0;
        }
    }
}
