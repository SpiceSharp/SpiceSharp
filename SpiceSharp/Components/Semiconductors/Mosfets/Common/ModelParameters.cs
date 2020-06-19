using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.Mosfets
{
    /// <summary>
    /// Common model parameters for mosfets.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public abstract class ModelParameters : ParameterSet
    {
        private GivenParameter<double> _sheetResistance;
        private GivenParameter<double> _nominalTemperature = new GivenParameter<double>(Constants.ReferenceTemperature, false);
        private GivenParameter<double> _surfaceStateDensity;
        private GivenParameter<double> _substrateDoping = new GivenParameter<double>(2e10, false); // Value isn't used...
        private double _forwardCapDepletionCoefficient = 0.5;
        private GivenParameter<double> _surfaceMobility = new GivenParameter<double>(600, false);
        private double _lateralDiffusion;
        private GivenParameter<double> _oxideThickness = new GivenParameter<double>(1e-7, false);
        private double _junctionSatCurDensity;
        private double _bulkJunctionSideGradingCoefficient = 0.33;
        private GivenParameter<double> _sidewallCapFactor;
        private double _bulkJunctionBotGradingCoefficient = 0.5;
        private GivenParameter<double> _bulkCapFactor;
        private double _gateBulkOverlapCapFactor;
        private double _gateDrainOverlapCapFactor;
        private double _gateSourceOverlapCapFactor;
        private double _bulkJunctionPotential = 0.8;
        private double _junctionSatCur = 1e-14;
        private GivenParameter<double> _capBs;
        private GivenParameter<double> _capBd;
        private double _sourceResistance;
        private double _drainResistance;
        private GivenParameter<double> _phi = new GivenParameter<double>(0.6, false);
        private GivenParameter<double> _gamma;
        private GivenParameter<double> _transconductance = new GivenParameter<double>(2e-5, false);
        private GivenParameter<double> _length = 1e-4;
        private GivenParameter<double> _width = 1e-4;

        /// <summary>
        /// Gets or sets the default width for transistors using this model.
        /// </summary>
        /// <value>
        /// The default width for transistors.
        /// </value>
        [ParameterName("w"), ParameterInfo("The default width for transistors using this model", Units = "m")]
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
        /// Gets or sets the default length for transistors using this model.
        /// </summary>
        /// <value>
        /// The default length for transistors.
        /// </value>
        [ParameterName("l"), ParameterInfo("The default length for transistors using this model", Units = "m")]
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
        /// Gets or sets the nominal temperature in degrees celsius.
        /// </summary>
        /// <value>
        /// The nominal temperature in degrees Celsius.
        /// </value>
        [ParameterName("tnom"), DerivedProperty, ParameterInfo("Parameter measurement temperature", Units = "\u00b0C")]
        [GreaterThan(Constants.CelsiusKelvin)]
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
        public GivenParameter<double> Vt0 { get; set; }

        /// <summary>
        /// Gets or sets the transconductance.
        /// </summary>
        /// <value>
        /// The transconductance.
        /// </value>
        [ParameterName("kp"), ParameterInfo("Transconductance parameter", Units = "A/V^2")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> Transconductance
        {
            get => _transconductance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Transconductance), 0);
                _transconductance = value;
            }
        }

        /// <summary>
        /// Gets or sets the bulk threshold parameter.
        /// </summary>
        /// <value>
        /// The bulk threshold parameter.
        /// </value>
        [ParameterName("gamma"), ParameterInfo("Bulk threshold parameter", Units = "V^0.5")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> Gamma
        {
            get => _gamma;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Gamma), 0);
                _gamma = value;
            }
        }

        /// <summary>
        /// Gets or sets the surface potential.
        /// </summary>
        /// <value>
        /// The surface potential.
        /// </value>
        [ParameterName("phi"), ParameterInfo("Surface potential", Units = "V")]
        [GreaterThan(0)]
        public GivenParameter<double> Phi
        {
            get => _phi;
            set
            {
                Utility.GreaterThan(value, nameof(Phi), 0);
                _phi = value;
            }
        }

        /// <summary>
        /// Gets or sets the drain ohmic resistance.
        /// </summary>
        /// <value>
        /// The drain ohmic resistance.
        /// </value>
        [ParameterName("rd"), ParameterInfo("Drain ohmic resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> DrainResistance
        {
            get => _drainResistance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(DrainResistance), 0);
                _drainResistance = value;
            }
        }

        /// <summary>
        /// Gets or sets the source ohmic resistance.
        /// </summary>
        /// <value>
        /// The source ohmic resistance.
        /// </value>
        [ParameterName("rs"), ParameterInfo("Source ohmic resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> SourceResistance
        {
            get => _sourceResistance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(SourceResistance), 0);
                _sourceResistance = value;
            }
        }

        /// <summary>
        /// Gets or sets the bulk-drain junction capacitance.
        /// </summary>
        /// <value>
        /// The bulk-drain junction capacitance.
        /// </value>
        [ParameterName("cbd"), ParameterInfo("B-D junction capacitance", Units = "F")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> CapBd
        {
            get => _capBd;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(CapBd), 0);
                _capBd = value;
            }
        }

        /// <summary>
        /// Gets or sets the bulk-source junction capacitance
        /// </summary>
        /// <value>
        /// The bulk-source junction capacitance
        /// </value>
        [ParameterName("cbs"), ParameterInfo("B-S junction capacitance", Units = "F")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> CapBs
        {
            get => _capBs;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(CapBs), 0);
                _capBs = value;
            }
        }

        /// <summary>
        /// Gets or sets the bulk junction saturation current.
        /// </summary>
        /// <value>
        /// The bulk junction saturation current.
        /// </value>
        [ParameterName("is"), ParameterInfo("Bulk junction saturation current", Units = "A")]
        [GreaterThanOrEquals(0)]
        public double JunctionSatCur
        {
            get => _junctionSatCur;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(JunctionSatCur), 0);
                _junctionSatCur = value;
            }
        }

        /// <summary>
        /// Gets or sets the bulk junction potential.
        /// </summary>
        /// <value>
        /// The bulk junction potential.
        /// </value>
        [ParameterName("pb"), ParameterInfo("Bulk junction potential", Units = "V")]
        [GreaterThan(0)]
        public double BulkJunctionPotential
        {
            get => _bulkJunctionPotential;
            set
            {
                Utility.GreaterThan(value, nameof(BulkJunctionPotential), 0);
                _bulkJunctionPotential = value;
            }
        }

        /// <summary>
        /// Gets or sets the gate-source overlap capacitance.
        /// </summary>
        /// <value>
        /// The gate-source overlap capacitance.
        /// </value>
        [ParameterName("cgso"), ParameterInfo("Gate-source overlap cap.", Units = "F/m")]
        [GreaterThanOrEquals(0)]
        public double GateSourceOverlapCapFactor
        {
            get => _gateSourceOverlapCapFactor;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(GateSourceOverlapCapFactor), 0);
                _gateSourceOverlapCapFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the gate-drain overlap capacitance.
        /// </summary>
        /// <value>
        /// The gate-drain overlap capacitance.
        /// </value>
        [ParameterName("cgdo"), ParameterInfo("Gate-drain overlap cap.", Units = "F/m")]
        [GreaterThanOrEquals(0)]
        public double GateDrainOverlapCapFactor
        {
            get => _gateDrainOverlapCapFactor;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(GateDrainOverlapCapFactor), 0);
                _gateDrainOverlapCapFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the gate-bulk overlap capacitance.
        /// </summary>
        /// <value>
        /// The gate-bulk overlap capacitance.
        /// </value>
        [ParameterName("cgbo"), ParameterInfo("Gate-bulk overlap cap.", Units = "F/m")]
        [GreaterThanOrEquals(0)]
        public double GateBulkOverlapCapFactor
        {
            get => _gateBulkOverlapCapFactor;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(GateBulkOverlapCapFactor), 0);
                _gateBulkOverlapCapFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the bottom junction capacitance per area.
        /// </summary>
        /// <value>
        /// The bottom junction capacitance per area.
        /// </value>
        [ParameterName("cj"), ParameterInfo("Bottom junction cap. per area", Units = "F/m^2")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> BulkCapFactor
        {
            get => _bulkCapFactor;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(BulkCapFactor), 0);
                _bulkCapFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the bulk junction bottom grading coefficient.
        /// </summary>
        /// <value>
        /// The bulk junction bottom grading coefficient.
        /// </value>
        [ParameterName("mj"), ParameterInfo("Bottom grading coefficient")]
        [GreaterThan(0)]
        public double BulkJunctionBotGradingCoefficient
        {
            get => _bulkJunctionBotGradingCoefficient;
            set
            {
                Utility.GreaterThan(value, nameof(BulkJunctionBotGradingCoefficient), 0);
                _bulkJunctionBotGradingCoefficient = value;
            }
        }

        /// <summary>
        /// Gets or sets the sidewall capacitance.
        /// </summary>
        /// <value>
        /// The sidewall capacitance.
        /// </value>
        [ParameterName("cjsw"), ParameterInfo("Side junction cap. per area", Units = "F/m^2")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> SidewallCapFactor
        {
            get => _sidewallCapFactor;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(SidewallCapFactor), 0);
                _sidewallCapFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the bulk junction side grading coefficient.
        /// </summary>
        /// <value>
        /// The bulk junction side grading coefficient.
        /// </value>
        [ParameterName("mjsw"), ParameterInfo("Side grading coefficient")]
        [GreaterThan(0)]
        public double BulkJunctionSideGradingCoefficient
        {
            get => _bulkJunctionSideGradingCoefficient;
            set
            {
                Utility.GreaterThan(value, nameof(BulkJunctionSideGradingCoefficient), 0);
                _bulkJunctionSideGradingCoefficient = value;
            }
        }

        /// <summary>
        /// Gets or sets the bulk junction saturation current density.
        /// </summary>
        /// <value>
        /// The bulk junction saturation current density.
        /// </value>
        [ParameterName("js"), ParameterInfo("Bulk jct. sat. current density", Units = "A/m^2")]
        [GreaterThanOrEquals(0)]
        public double JunctionSatCurDensity
        {
            get => _junctionSatCurDensity;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(JunctionSatCurDensity), 0);
                _junctionSatCurDensity = value;
            }
        }

        /// <summary>
        /// Gets or sets the oxide thickness.
        /// </summary>
        /// <value>
        /// The oxide thickness.
        /// </value>
        [ParameterName("tox"), ParameterInfo("Oxide thickness", Units = "m")]
        [GreaterThan(0)]
        public GivenParameter<double> OxideThickness
        {
            get => _oxideThickness;
            set
            {
                Utility.GreaterThan(value, nameof(OxideThickness), 0);
                _oxideThickness = value;
            }
        }

        /// <summary>
        /// Gets or sets the lateral diffusion.
        /// </summary>
        /// <value>
        /// The lateral diffusion.
        /// </value>
        [ParameterName("ld"), ParameterInfo("Lateral diffusion", Units = "m")]
        [GreaterThanOrEquals(0)]
        public double LateralDiffusion
        {
            get => _lateralDiffusion;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(LateralDiffusion), 0);
                _lateralDiffusion = value;
            }
        }

        /// <summary>
        /// Gets or sets the sheet resistance.
        /// </summary>
        /// <value>
        /// The sheet resistance.
        /// </value>
        [ParameterName("rsh"), ParameterInfo("Sheet resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> SheetResistance
        {
            get => _sheetResistance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(SheetResistance), 0);
                _sheetResistance = value;
            }
        }

        /// <summary>
        /// Gets or sets the surface mobility.
        /// </summary>
        /// <value>
        /// The surface mobility.
        /// </value>
        [ParameterName("u0"), ParameterName("uo"), ParameterInfo("Surface mobility", Units = "V/cm")]
        [GreaterThan(0)]
        public GivenParameter<double> SurfaceMobility
        {
            get => _surfaceMobility;
            set
            {
                Utility.GreaterThan(value, nameof(SurfaceMobility), 0);
                _surfaceMobility = value;
            }
        }

        /// <summary>
        /// Gets or sets the forward bias junction fitting parameter.
        /// </summary>
        /// <value>
        /// The forward bias junction fitting parameter.
        /// </value>
        [ParameterName("fc"), ParameterInfo("Forward bias jct. fit parm.")]
        [GreaterThan(0)]
        public double ForwardCapDepletionCoefficient
        {
            get => _forwardCapDepletionCoefficient;
            set
            {
                Utility.GreaterThan(value, nameof(ForwardCapDepletionCoefficient), 0);
                _forwardCapDepletionCoefficient = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the gate.
        /// </summary>
        /// <value>
        /// The type of the gate.
        /// </value>
        [ParameterName("tpg"), ParameterInfo("Gate type")]
        public GivenParameter<double> GateType { get; set; } = new GivenParameter<double>(1, false);

        /// <summary>
        /// Gets or sets the substrate doping level.
        /// </summary>
        /// <value>
        /// The substrate doping level.
        /// </value>
        [ParameterName("nsub"), ParameterInfo("Substrate doping")]
        [GreaterThan(1.45e10)]
        public GivenParameter<double> SubstrateDoping
        {
            get => _substrateDoping;
            set
            {
                Utility.GreaterThan(value, nameof(SubstrateDoping), 1.45e10);
                _substrateDoping = value;
            }
        }

        /// <summary>
        /// Gets or sets the surface state density.
        /// </summary>
        /// <value>
        /// The surface state density.
        /// </value>
        [ParameterName("nss"), ParameterInfo("Surface state density")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> SurfaceStateDensity
        {
            get => _surfaceStateDensity;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(SurfaceStateDensity), 0);
                _surfaceStateDensity = value;
            }
        }

        /// <summary>
        /// Gets or sets the nominal temperature in Kelvin.
        /// </summary>
        /// <value>
        /// The nominal temperature in Kelvin.
        /// </value>
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> NominalTemperature
        {
            get => _nominalTemperature;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(NominalTemperature), 0);
                _nominalTemperature = value;
            }
        }

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
        public double FlickerNoiseCoefficient { get; set; }

        /// <summary>
        /// Gets or sets the flicker-noise exponent parameter.
        /// </summary>
        /// <value>
        /// The flicker noise exponent.
        /// </value>
        [ParameterName("af"), ParameterInfo("Flicker noise exponent")]
        public double FlickerNoiseExponent { get; set; } = 1;

        /// <summary>
        /// Creates a deep clone of the parameter set.
        /// </summary>
        /// <returns>
        /// A deep clone of the parameter set.
        /// </returns>
        protected override ICloneable Clone()
        {
            // We have a properties that are only privately settable, so we need to update them manually when cloning.
            var result = (ModelParameters)base.Clone();

            // Copy the (private/protected) parameters
            result.MosfetType = MosfetType;

            return result;
        }
    }
}
