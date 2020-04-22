using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    /// <summary>
    /// Common model parameters for mosfets.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public abstract class ModelBaseParameters : ParameterSet
    {
        private GivenParameter<double> _nominalTemperature = new GivenParameter<double>(Constants.ReferenceTemperature, false);
        private GivenParameter<double> _surfaceStateDensity;
        private GivenParameter<double> _substrateDoping = new GivenParameter<double>(2e10, false); // Value isn't used...
        private double _forwardCapDepletionCoefficient = 0.5;
        private double _surfaceMobility = 600;
        private double _sheetResistance;
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
        /// Gets the default width for transistors using this model.
        /// </summary>
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
        /// Gets the default length for transistors using this model.
        /// </summary>
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
        [ParameterName("tnom"), DerivedProperty, ParameterInfo("Parameter measurement temperature", Units = "\u00b0C")]
        [GreaterThan(Constants.CelsiusKelvin)]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the base threshold voltage.
        /// </summary>
        [ParameterName("vto"), ParameterName("vt0"), ParameterInfo("Threshold voltage", Units = "V")]
        public GivenParameter<double> Vt0 { get; set; }

        /// <summary>
        /// Gets the transconductance.
        /// </summary>
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
        /// Gets the bulk threshold parameter.
        /// </summary>
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
        /// Gets the surface potential.
        /// </summary>
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
        /// Gets the drain ohmic resistance.
        /// </summary>
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
        /// Gets the source ohmic resistance.
        /// </summary>
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
        /// Gets the bulk-drain junction capacitance.
        /// </summary>
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
        /// Gets the bulk-source junction capacitance
        /// </summary>
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
        /// Gets the bulk junction saturation current.
        /// </summary>
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
        /// Gets the bulk junction potential.
        /// </summary>
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
        /// Gets the gate-source overlap capacitance.
        /// </summary>
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
        /// Gets the gate-drain overlap capacitance.
        /// </summary>
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
        /// Gets the gate-bulk overlap capacitance.
        /// </summary>
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
        /// Gets the bottom junction capacitance per area.
        /// </summary>
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
        /// Gets the bulk junction bottom grading coefficient.
        /// </summary>
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
        /// Gets the sidewall capacitance.
        /// </summary>
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
        /// Gets the bulk junction side grading coefficient.
        /// </summary>
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
        /// Gets the bulk junction saturation current density.
        /// </summary>
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
        /// Gets the oxide thickness.
        /// </summary>
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
        /// Gets the lateral diffusion.
        /// </summary>
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
        /// Gets the sheet resistance.
        /// </summary>
        [ParameterName("rsh"), ParameterInfo("Sheet resistance", Units = "\u03a9")]
        [GreaterThanOrEquals(0)]
        public double SheetResistance
        {
            get => _sheetResistance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(SheetResistance), 0);
                _sheetResistance = value;
            }
        }

        /// <summary>
        /// Gets the surface mobility.
        /// </summary>
        [ParameterName("u0"), ParameterName("uo"), ParameterInfo("Surface mobility", Units = "V/cm")]
        [GreaterThan(0)]
        public double SurfaceMobility
        {
            get => _surfaceMobility;
            set
            {
                Utility.GreaterThan(value, nameof(SurfaceMobility), 0);
                _surfaceMobility = value;
            }
        }

        /// <summary>
        /// Gets the forward bias junction fitting parameter.
        /// </summary>
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
        /// Gets the type of the gate.
        /// </summary>
        [ParameterName("tpg"), ParameterInfo("Gate type")]
        public GivenParameter<double> GateType { get; set; }

        /// <summary>
        /// Gets the substrate doping level.
        /// </summary>
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
        /// Gets the surface state density.
        /// </summary>
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
        /// Gets the nominal temperature in Kelvin.
        /// </summary>
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
        /// Gets the oxide capacitance density.
        /// </summary>
        public double OxideCapFactor { get; protected set; }

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        public override void CalculateDefaults()
        {
            // Calculate the oxide capacitance
            OxideCapFactor = 3.9 * 8.854214871e-12 / OxideThickness;

            // Calculate the default transconductance
            if (!Transconductance.Given)
                Transconductance = new GivenParameter<double>(SurfaceMobility * 1e-4 * OxideCapFactor, false); // m^2/cm^2
        }

        /// <summary>
        /// Creates a deep clone of the parameter set.
        /// </summary>
        /// <returns>
        /// A deep clone of the parameter set.
        /// </returns>
        protected override ICloneable Clone()
        {
            // We have a properties that are only privately settable, so we need to update them manually when cloning.
            var result = (ModelBaseParameters)base.Clone();

            // Copy the (private/protected) parameters
            result.MosfetType = MosfetType;
            result.OxideCapFactor = OxideCapFactor;

            return result;
        }
    }
}
