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
        private GivenParameter<double> _substrateDoping = new GivenParameter<double>(1.45e10, false);
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
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(Width), value, 0));
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
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(Length), value, 0));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(Transconductance), value, 0));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(Gamma), value, 0));
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
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(Phi), value, 0));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(DrainResistance), value, 0));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(SourceResistance), value, 0));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(CapBd), value, 0));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(CapBs), value, 0));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(JunctionSatCur), value, 0));
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
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(BulkJunctionPotential), value, 0));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(GateSourceOverlapCapFactor), value, 0));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(GateDrainOverlapCapFactor), value, 0));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(GateBulkOverlapCapFactor), value, 0));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(BulkCapFactor), value, 0));
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
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(BulkJunctionBotGradingCoefficient), value, 0));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(SidewallCapFactor), value, 0));
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
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(BulkJunctionSideGradingCoefficient), value, 0));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(JunctionSatCurDensity), value, 0));
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
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(OxideThickness), value, 0));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(LateralDiffusion), value, 0));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(SheetResistance), value, 0));
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
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(SurfaceMobility), value, 0));
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
                if (value <= 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(ForwardCapDepletionCoefficient), value, 0));
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
        [GreaterThanOrEquals(1.45e10)]
        public GivenParameter<double> SubstrateDoping
        {
            get => _substrateDoping;
            set
            {
                if (value < 1.45e10)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(SubstrateDoping), value, 1.45e10));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(SurfaceStateDensity), value, 0));
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
                if (value < 0)
                    throw new ArgumentException(Properties.Resources.Parameters_TooSmall.FormatString(nameof(NominalTemperature), value, 0));
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
