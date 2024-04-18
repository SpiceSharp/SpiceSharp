using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Mosfets
{
    /// <summary>
    /// Common model parameters for mosfets.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public abstract partial class ModelParameters : ParameterSet, ICloneable<ModelParameters>
    {
        /// <summary>
        /// Gets or sets the default width for transistors using this model.
        /// </summary>
        /// <value>
        /// The default width for transistors.
        /// </value>
        [ParameterName("w"), ParameterInfo("The default width for transistors using this model", Units = "m")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _width = 1e-4;

        /// <summary>
        /// Gets or sets the default length for transistors using this model.
        /// </summary>
        /// <value>
        /// The default length for transistors.
        /// </value>
        [ParameterName("l"), ParameterInfo("The default length for transistors using this model", Units = "m")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _length = 1e-4;

        /// <summary>
        /// Gets or sets the nominal temperature in degrees celsius.
        /// </summary>
        /// <value>
        /// The nominal temperature in degrees Celsius.
        /// </value>
        [ParameterName("tnom"), ParameterInfo("Parameter measurement temperature", Units = "\u00b0C")]
        [DerivedProperty, GreaterThan(-Constants.CelsiusKelvin), Finite]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets or sets the base threshold voltage.
        /// </summary>
        /// <value>
        /// The base threshold voltage.
        /// </value>
        [ParameterName("vto"), ParameterName("vt0"), ParameterInfo("Threshold voltage", Units = "V")]
        [Finite]
        private GivenParameter<double> _vt0;

        /// <summary>
        /// Gets or sets the transconductance.
        /// </summary>
        /// <value>
        /// The transconductance.
        /// </value>
        [ParameterName("kp"), ParameterInfo("Transconductance parameter", Units = "A/V^2")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _transconductance = new(2e-5, false);

        /// <summary>
        /// Gets or sets the bulk threshold parameter.
        /// </summary>
        /// <value>
        /// The bulk threshold parameter.
        /// </value>
        [ParameterName("gamma"), ParameterInfo("Bulk threshold parameter", Units = "V^0.5")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _gamma;

        /// <summary>
        /// Gets or sets the surface potential.
        /// </summary>
        /// <value>
        /// The surface potential.
        /// </value>
        [ParameterName("phi"), ParameterInfo("Surface potential", Units = "V")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _phi = new(0.6, false);

        /// <summary>
        /// Gets or sets the drain ohmic resistance.
        /// </summary>
        /// <value>
        /// The drain ohmic resistance.
        /// </value>
        [ParameterName("rd"), ParameterInfo("Drain ohmic resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _drainResistance;

        /// <summary>
        /// Gets or sets the source ohmic resistance.
        /// </summary>
        /// <value>
        /// The source ohmic resistance.
        /// </value>
        [ParameterName("rs"), ParameterInfo("Source ohmic resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _sourceResistance;

        /// <summary>
        /// Gets or sets the bulk-drain junction capacitance.
        /// </summary>
        /// <value>
        /// The bulk-drain junction capacitance.
        /// </value>
        [ParameterName("cbd"), ParameterInfo("B-D junction capacitance", Units = "F")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _capBd;

        /// <summary>
        /// Gets or sets the bulk-source junction capacitance
        /// </summary>
        /// <value>
        /// The bulk-source junction capacitance
        /// </value>
        [ParameterName("cbs"), ParameterInfo("B-S junction capacitance", Units = "F")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _capBs;

        /// <summary>
        /// Gets or sets the bulk junction saturation current.
        /// </summary>
        /// <value>
        /// The bulk junction saturation current.
        /// </value>
        [ParameterName("is"), ParameterInfo("Bulk junction saturation current", Units = "A")]
        [GreaterThanOrEquals(0), Finite]
        private double _junctionSatCur = 1e-14;

        /// <summary>
        /// Gets or sets the bulk junction potential.
        /// </summary>
        /// <value>
        /// The bulk junction potential.
        /// </value>
        [ParameterName("pb"), ParameterInfo("Bulk junction potential", Units = "V")]
        [GreaterThan(0), Finite]
        private double _bulkJunctionPotential = 0.8;

        /// <summary>
        /// Gets or sets the gate-source overlap capacitance.
        /// </summary>
        /// <value>
        /// The gate-source overlap capacitance.
        /// </value>
        [ParameterName("cgso"), ParameterInfo("Gate-source overlap cap.", Units = "F/m")]
        [GreaterThanOrEquals(0), Finite]
        private double _gateSourceOverlapCapFactor;

        /// <summary>
        /// Gets or sets the gate-drain overlap capacitance.
        /// </summary>
        /// <value>
        /// The gate-drain overlap capacitance.
        /// </value>
        [ParameterName("cgdo"), ParameterInfo("Gate-drain overlap cap.", Units = "F/m")]
        [GreaterThanOrEquals(0), Finite]
        private double _gateDrainOverlapCapFactor;

        /// <summary>
        /// Gets or sets the gate-bulk overlap capacitance.
        /// </summary>
        /// <value>
        /// The gate-bulk overlap capacitance.
        /// </value>
        [ParameterName("cgbo"), ParameterInfo("Gate-bulk overlap cap.", Units = "F/m")]
        [GreaterThanOrEquals(0), Finite]
        private double _gateBulkOverlapCapFactor;

        /// <summary>
        /// Gets or sets the bottom junction capacitance per area.
        /// </summary>
        /// <value>
        /// The bottom junction capacitance per area.
        /// </value>
        [ParameterName("cj"), ParameterInfo("Bottom junction cap. per area", Units = "F/m^2")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _bulkCapFactor;

        /// <summary>
        /// Gets or sets the bulk junction bottom grading coefficient.
        /// </summary>
        /// <value>
        /// The bulk junction bottom grading coefficient.
        /// </value>
        [ParameterName("mj"), ParameterInfo("Bottom grading coefficient")]
        [GreaterThan(0), Finite]
        private double _bulkJunctionBotGradingCoefficient = 0.5;

        /// <summary>
        /// Gets or sets the sidewall capacitance.
        /// </summary>
        /// <value>
        /// The sidewall capacitance.
        /// </value>
        [ParameterName("cjsw"), ParameterInfo("Side junction cap. per area", Units = "F/m^2")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _sidewallCapFactor;

        /// <summary>
        /// Gets or sets the bulk junction side grading coefficient.
        /// </summary>
        /// <value>
        /// The bulk junction side grading coefficient.
        /// </value>
        [ParameterName("mjsw"), ParameterInfo("Side grading coefficient")]
        [GreaterThan(0), Finite]
        private double _bulkJunctionSideGradingCoefficient = 0.33;

        /// <summary>
        /// Gets or sets the bulk junction saturation current density.
        /// </summary>
        /// <value>
        /// The bulk junction saturation current density.
        /// </value>
        [ParameterName("js"), ParameterInfo("Bulk jct. sat. current density", Units = "A/m^2")]
        [GreaterThanOrEquals(0), Finite]
        private double _junctionSatCurDensity;

        /// <summary>
        /// Gets or sets the oxide thickness.
        /// </summary>
        /// <value>
        /// The oxide thickness.
        /// </value>
        [ParameterName("tox"), ParameterInfo("Oxide thickness", Units = "m")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _oxideThickness = new(1e-7, false);

        /// <summary>
        /// Gets or sets the lateral diffusion.
        /// </summary>
        /// <value>
        /// The lateral diffusion.
        /// </value>
        [ParameterName("ld"), ParameterInfo("Lateral diffusion", Units = "m")]
        [GreaterThanOrEquals(0), Finite]
        private double _lateralDiffusion;

        /// <summary>
        /// Gets or sets the sheet resistance.
        /// </summary>
        /// <value>
        /// The sheet resistance.
        /// </value>
        [ParameterName("rsh"), ParameterInfo("Sheet resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _sheetResistance;

        /// <summary>
        /// Gets or sets the surface mobility.
        /// </summary>
        /// <value>
        /// The surface mobility.
        /// </value>
        [ParameterName("u0"), ParameterName("uo"), ParameterInfo("Surface mobility", Units = "V/cm")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _surfaceMobility = new(600, false);

        /// <summary>
        /// Gets or sets the forward bias junction fitting parameter.
        /// </summary>
        /// <value>
        /// The forward bias junction fitting parameter.
        /// </value>
        [ParameterName("fc"), ParameterInfo("Forward bias jct. fit parm.")]
        [GreaterThan(0), Finite]
        private double _forwardCapDepletionCoefficient = 0.5;

        /// <summary>
        /// Gets or sets the type of the gate.
        /// </summary>
        /// <value>
        /// The type of the gate.
        /// </value>
        [ParameterName("tpg"), ParameterInfo("Gate type")]
        [Finite]
        private GivenParameter<double> _gateType = new(1, false);

        /// <summary>
        /// Gets or sets the substrate doping level.
        /// </summary>
        /// <value>
        /// The substrate doping level.
        /// </value>
        [ParameterName("nsub"), ParameterInfo("Substrate doping")]
        [GreaterThan(1.45e10), Finite]
        private GivenParameter<double> _substrateDoping = new(2e10, false); // Value isn't used...

        /// <summary>
        /// Gets or sets the surface state density.
        /// </summary>
        /// <value>
        /// The surface state density.
        /// </value>
        [ParameterName("nss"), ParameterInfo("Surface state density")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _surfaceStateDensity;

        /// <summary>
        /// Gets or sets the nominal temperature in Kelvin.
        /// </summary>
        /// <value>
        /// The nominal temperature in Kelvin.
        /// </value>
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _nominalTemperature = new(Constants.ReferenceTemperature, false);

        /// <summary>
        /// Gets or sets the mosfet type.
        /// </summary>
        /// <value>
        /// The mosfet type.
        /// </value>
        public double MosfetType { get; protected set; } = 1.0;

        /// <summary>
        /// Sets the model to represent an NMOS.
        /// </summary>
        /// <param name="value">if set to <c>true</c>, the model represents an NMOS.</param>
        [ParameterName("nmos"), ParameterInfo("N type Mosfet model")]
        public void SetNmos(bool value)
        {
            if (value)
                MosfetType = 1.0;
        }

        /// <summary>
        /// Sets the model to represent a PMOS.
        /// </summary>
        /// <param name="value">if set to <c>true</c>, the model represents a PMOS.</param>
        [ParameterName("pmos"), ParameterInfo("P type Mosfet model")]
        public void SetPmos(bool value)
        {
            if (value)
                MosfetType = -1.0;
        }

        /// <inheritdoc/>
        public virtual ModelParameters Clone()
            => (ModelParameters)Clone();

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <value>
        /// The name of the type.
        /// </value>
        [ParameterName("type"), ParameterInfo("N-channel or P-channel MOS")]
        public string TypeName
        {
            get
            {
                if (MosfetType > 0)
                    return "nmos";
                return "pmos";
            }
        }

        /// <summary>
        /// Gets or sets the flicker-noise coefficient parameter.
        /// </summary>
        /// <value>
        /// The flicker noise coefficient.
        /// </value>
        [ParameterName("kf"), ParameterInfo("Flicker noise coefficient")]
        [Finite]
        private double _flickerNoiseCoefficient;

        /// <summary>
        /// Gets or sets the flicker-noise exponent parameter.
        /// </summary>
        /// <value>
        /// The flicker noise exponent.
        /// </value>
        [ParameterName("af"), ParameterInfo("Flicker noise exponent")]
        [Finite]
        private double _flickerNoiseExponent = 1;
    }
}
