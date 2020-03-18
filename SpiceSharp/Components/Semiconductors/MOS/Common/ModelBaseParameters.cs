using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    /// <summary>
    /// Common model parameters for mosfets.
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public abstract class ModelBaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets the default width for transistors using this model.
        /// </summary>
        [ParameterName("w"), ParameterInfo("The default width for transistors using this model")]
        public GivenParameter<double> Width { get; set; }

        /// <summary>
        /// Gets the default length for transistors using this model.
        /// </summary>
        [ParameterName("l"), ParameterInfo("The default length for transistors using this model")]
        public GivenParameter<double> Length { get; set; }

        /// <summary>
        /// Gets or sets the nominal temperature in degrees celsius.
        /// </summary>
        [ParameterName("tnom"), DerivedProperty, ParameterInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the base threshold voltage.
        /// </summary>
        [ParameterName("vto"), ParameterName("vt0"), ParameterInfo("Threshold voltage")]
        public GivenParameter<double> Vt0 { get; set; }

        /// <summary>
        /// Gets the transconductance.
        /// </summary>
        [ParameterName("kp"), ParameterInfo("Transconductance parameter")]
        public GivenParameter<double> Transconductance { get; set; } = new GivenParameter<double>(2e-5, false);

        /// <summary>
        /// Gets the bulk threshold parameter.
        /// </summary>
        [ParameterName("gamma"), ParameterInfo("Bulk threshold parameter")]
        public GivenParameter<double> Gamma { get; set; }

        /// <summary>
        /// Gets the surface potential.
        /// </summary>
        [ParameterName("phi"), ParameterInfo("Surface potential")]
        public GivenParameter<double> Phi { get; set; } = new GivenParameter<double>(0.6, false);

        /// <summary>
        /// Gets the drain ohmic resistance.
        /// </summary>
        [ParameterName("rd"), ParameterInfo("Drain ohmic resistance")]
        public double DrainResistance { get; set; }

        /// <summary>
        /// Gets the source ohmic resistance.
        /// </summary>
        [ParameterName("rs"), ParameterInfo("Source ohmic resistance")]
        public double SourceResistance { get; set; }

        /// <summary>
        /// Gets the bulk-drain junction capacitance.
        /// </summary>
        [ParameterName("cbd"), ParameterInfo("B-D junction capacitance")]
        public GivenParameter<double> CapBd { get; set; }

        /// <summary>
        /// Gets the bulk-source junction capacitance
        /// </summary>
        [ParameterName("cbs"), ParameterInfo("B-S junction capacitance")]
        public GivenParameter<double> CapBs { get; set; }

        /// <summary>
        /// Gets the bulk junction saturation current.
        /// </summary>
        [ParameterName("is"), ParameterInfo("Bulk junction sat. current")]
        public double JunctionSatCur { get; set; } = 1e-14;

        /// <summary>
        /// Gets the bulk junction potential.
        /// </summary>
        [ParameterName("pb"), ParameterInfo("Bulk junction potential")]
        public double BulkJunctionPotential { get; set; } = 0.8;

        /// <summary>
        /// Gets the gate-source overlap capacitance.
        /// </summary>
        [ParameterName("cgso"), ParameterInfo("Gate-source overlap cap.")]
        public double GateSourceOverlapCapFactor { get; set; }

        /// <summary>
        /// Gets the gate-drain overlap capacitance.
        /// </summary>
        [ParameterName("cgdo"), ParameterInfo("Gate-drain overlap cap.")]
        public double GateDrainOverlapCapFactor { get; set; }

        /// <summary>
        /// Gets the gate-bulk overlap capacitance.
        /// </summary>
        [ParameterName("cgbo"), ParameterInfo("Gate-bulk overlap cap.")]
        public double GateBulkOverlapCapFactor { get; set; }

        /// <summary>
        /// Gets the bottom junction capacitance per area.
        /// </summary>
        [ParameterName("cj"), ParameterInfo("Bottom junction cap per area")]
        public GivenParameter<double> BulkCapFactor { get; set; }

        /// <summary>
        /// Gets the bulk junction bottom grading coefficient.
        /// </summary>
        [ParameterName("mj"), ParameterInfo("Bottom grading coefficient")]
        public double BulkJunctionBotGradingCoefficient { get; set; } = 0.5;

        /// <summary>
        /// Gets the sidewall capacitance.
        /// </summary>
        [ParameterName("cjsw"), ParameterInfo("Side junction cap per area")]
        public GivenParameter<double> SidewallCapFactor { get; set; }

        /// <summary>
        /// Gets the bulk junction side grading coefficient.
        /// </summary>
        [ParameterName("mjsw"), ParameterInfo("Side grading coefficient")]
        public double BulkJunctionSideGradingCoefficient { get; set; } = 0.33;

        /// <summary>
        /// Gets the bulk junction saturation current density.
        /// </summary>
        [ParameterName("js"), ParameterInfo("Bulk jct. sat. current density")]
        public double JunctionSatCurDensity { get; set; }

        /// <summary>
        /// Gets the oxide thickness.
        /// </summary>
        [ParameterName("tox"), ParameterInfo("Oxide thickness")]
        public GivenParameter<double> OxideThickness { get; set; }

        /// <summary>
        /// Gets the lateral diffusion.
        /// </summary>
        [ParameterName("ld"), ParameterInfo("Lateral diffusion")]
        public double LateralDiffusion { get; set; }

        /// <summary>
        /// Gets the sheet resistance.
        /// </summary>
        [ParameterName("rsh"), ParameterInfo("Sheet resistance")]
        public double SheetResistance { get; set; }

        /// <summary>
        /// Gets the surface mobility.
        /// </summary>
        [ParameterName("u0"), ParameterName("uo"), ParameterInfo("Surface mobility")]
        public double SurfaceMobility { get; set; } = 600;

        /// <summary>
        /// Gets the forward bias junction fitting parameter.
        /// </summary>
        [ParameterName("fc"), ParameterInfo("Forward bias jct. fit parm.")]
        public double ForwardCapDepletionCoefficient { get; set; } = 0.5;

        /// <summary>
        /// Gets the type of the gate.
        /// </summary>
        [ParameterName("tpg"), ParameterInfo("Gate type")]
        public GivenParameter<double> GateType { get; set; }

        /// <summary>
        /// Gets the substrate doping level.
        /// </summary>
        [ParameterName("nsub"), ParameterInfo("Substrate doping")]
        public GivenParameter<double> SubstrateDoping { get; set; }

        /// <summary>
        /// Gets the surface state density.
        /// </summary>
        [ParameterName("nss"), ParameterInfo("Surface state density")]
        public GivenParameter<double> SurfaceStateDensity { get; set; }

        /// <summary>
        /// Gets the nominal temperature in Kelvin.
        /// </summary>
        public GivenParameter<double> NominalTemperature { get; set; } = new GivenParameter<double>(Constants.ReferenceTemperature, false);

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
            if (OxideThickness.Given && OxideThickness <= 0.0)
                throw new BadParameterException(nameof(OxideThickness), OxideThickness, 
                    Properties.Resources.Mosfets_OxideThicknessTooSmall);

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
            var result = (ModelBaseParameters) base.Clone();

            // Copy the (private/protected) parameters
            result.MosfetType = MosfetType;
            result.OxideCapFactor = OxideCapFactor;

            return result;
        }
    }
}
