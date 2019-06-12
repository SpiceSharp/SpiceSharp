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
        /// <value>
        /// The width.
        /// </value>
        [ParameterName("w"), ParameterInfo("The default width for transistors using this model")]
        public GivenParameter<double> Width { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the default length for transistors using this model.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        [ParameterName("l"), ParameterInfo("The default length for transistors using this model")]
        public GivenParameter<double> Length { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets or sets the nominal temperature in degrees celsius.
        /// </summary>
        /// <value>
        /// The nominal temperature.
        /// </value>
        [ParameterName("tnom"), DerivedProperty, ParameterInfo("Parameter measurement temperature")]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature.Value = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets the base threshold voltage.
        /// </summary>
        /// <value>
        /// The threshold voltage.
        /// </value>
        [ParameterName("vto"), ParameterName("vt0"), ParameterInfo("Threshold voltage")]
        public GivenParameter<double> Vt0 { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the transconductance.
        /// </summary>
        /// <value>
        /// The transconductance.
        /// </value>
        [ParameterName("kp"), ParameterInfo("Transconductance parameter")]
        public GivenParameter<double> Transconductance { get; } = new GivenParameter<double>(2e-5);

        /// <summary>
        /// Gets the bulk threshold parameter.
        /// </summary>
        /// <value>
        /// The bulk threshold parameter.
        /// </value>
        [ParameterName("gamma"), ParameterInfo("Bulk threshold parameter")]
        public GivenParameter<double> Gamma { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the surface potential.
        /// </summary>
        /// <value>
        /// The surface potential.
        /// </value>
        [ParameterName("phi"), ParameterInfo("Surface potential")]
        public GivenParameter<double> Phi { get; } = new GivenParameter<double>(0.6);

        /// <summary>
        /// Gets the drain ohmic resistance.
        /// </summary>
        /// <value>
        /// The drain resistance.
        /// </value>
        [ParameterName("rd"), ParameterInfo("Drain ohmic resistance")]
        public GivenParameter<double> DrainResistance { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the source ohmic resistance.
        /// </summary>
        /// <value>
        /// The source resistance.
        /// </value>
        [ParameterName("rs"), ParameterInfo("Source ohmic resistance")]
        public GivenParameter<double> SourceResistance { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the bulk-drain junction capacitance.
        /// </summary>
        /// <value>
        /// The bulk-drain junction capacitance.
        /// </value>
        [ParameterName("cbd"), ParameterInfo("B-D junction capacitance")]
        public GivenParameter<double> CapBd { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the bulk-source junction capacitance
        /// </summary>
        /// <value>
        /// The bulk-source junction capacitance.
        /// </value>
        [ParameterName("cbs"), ParameterInfo("B-S junction capacitance")]
        public GivenParameter<double> CapBs { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the bulk junction saturation current.
        /// </summary>
        /// <value>
        /// The junction saturation current.
        /// </value>
        [ParameterName("is"), ParameterInfo("Bulk junction sat. current")]
        public GivenParameter<double> JunctionSatCur { get; } = new GivenParameter<double>(1e-14);

        /// <summary>
        /// Gets the bulk junction potential.
        /// </summary>
        /// <value>
        /// The bulk junction potential.
        /// </value>
        [ParameterName("pb"), ParameterInfo("Bulk junction potential")]
        public GivenParameter<double> BulkJunctionPotential { get; } = new GivenParameter<double>(.8);

        /// <summary>
        /// Gets the gate-source overlap capacitance.
        /// </summary>
        /// <value>
        /// The gate-source overlap capacitance.
        /// </value>
        [ParameterName("cgso"), ParameterInfo("Gate-source overlap cap.")]
        public GivenParameter<double> GateSourceOverlapCapFactor { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the gate-drain overlap capacitance.
        /// </summary>
        /// <value>
        /// The gate-drain overlap capacitance.
        /// </value>
        [ParameterName("cgdo"), ParameterInfo("Gate-drain overlap cap.")]
        public GivenParameter<double> GateDrainOverlapCapFactor { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the gate-bulk overlap capacitance.
        /// </summary>
        /// <value>
        /// The gate-bulk overlap capacitance.
        /// </value>
        [ParameterName("cgbo"), ParameterInfo("Gate-bulk overlap cap.")]
        public GivenParameter<double> GateBulkOverlapCapFactor { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the bottom junction capacitance per area.
        /// </summary>
        /// <value>
        /// The bulk capacitance density.
        /// </value>
        [ParameterName("cj"), ParameterInfo("Bottom junction cap per area")]
        public GivenParameter<double> BulkCapFactor { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the bulk junction bottom grading coefficient.
        /// </summary>
        /// <value>
        /// The bulk junction bottom grading coefficient.
        /// </value>
        [ParameterName("mj"), ParameterInfo("Bottom grading coefficient")]
        public GivenParameter<double> BulkJunctionBotGradingCoefficient { get; } = new GivenParameter<double>(0.5);

        /// <summary>
        /// Gets the sidewall capacitance.
        /// </summary>
        /// <value>
        /// The sidewall capacitance.
        /// </value>
        [ParameterName("cjsw"), ParameterInfo("Side junction cap per area")]
        public GivenParameter<double> SidewallCapFactor { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the bulk junction side grading coefficient.
        /// </summary>
        /// <value>
        /// The bulk junction side grading coefficient.
        /// </value>
        [ParameterName("mjsw"), ParameterInfo("Side grading coefficient")]
        public GivenParameter<double> BulkJunctionSideGradingCoefficient { get; } = new GivenParameter<double>(0.33);

        /// <summary>
        /// Gets the bulk junction saturation current density.
        /// </summary>
        /// <value>
        /// The bulk junction saturation current density.
        /// </value>
        [ParameterName("js"), ParameterInfo("Bulk jct. sat. current density")]
        public GivenParameter<double> JunctionSatCurDensity { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the oxide thickness.
        /// </summary>
        /// <value>
        /// The oxide thickness.
        /// </value>
        [ParameterName("tox"), ParameterInfo("Oxide thickness")]
        public GivenParameter<double> OxideThickness { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the lateral diffusion.
        /// </summary>
        /// <value>
        /// The lateral diffusion.
        /// </value>
        [ParameterName("ld"), ParameterInfo("Lateral diffusion")]
        public GivenParameter<double> LateralDiffusion { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the sheet resistance.
        /// </summary>
        /// <value>
        /// The sheet resistance.
        /// </value>
        [ParameterName("rsh"), ParameterInfo("Sheet resistance")]
        public GivenParameter<double> SheetResistance { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the surface mobility.
        /// </summary>
        /// <value>
        /// The surface mobility.
        /// </value>
        [ParameterName("u0"), ParameterName("uo"), ParameterInfo("Surface mobility")]
        public GivenParameter<double> SurfaceMobility { get; } = new GivenParameter<double>(600);

        /// <summary>
        /// Gets the forward bias junction fitting parameter.
        /// </summary>
        /// <value>
        /// The forward bias junction fitting parameter.
        /// </value>
        [ParameterName("fc"), ParameterInfo("Forward bias jct. fit parm.")]
        public GivenParameter<double> ForwardCapDepletionCoefficient { get; } = new GivenParameter<double>(0.5);

        /// <summary>
        /// Gets the type of the gate.
        /// </summary>
        /// <value>
        /// The type of the gate.
        /// </value>
        [ParameterName("tpg"), ParameterInfo("Gate type")]
        public GivenParameter<double> GateType { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the substrate doping level.
        /// </summary>
        /// <value>
        /// The substrate doping level.
        /// </value>
        [ParameterName("nsub"), ParameterInfo("Substrate doping")]
        public GivenParameter<double> SubstrateDoping { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the surface state density.
        /// </summary>
        /// <value>
        /// The surface state density.
        /// </value>
        [ParameterName("nss"), ParameterInfo("Surface state density")]
        public GivenParameter<double> SurfaceStateDensity { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the nominal temperature in Kelvin.
        /// </summary>
        /// <value>
        /// The nominal temperature.
        /// </value>
        public GivenParameter<double> NominalTemperature { get; } = new GivenParameter<double>(Constants.ReferenceTemperature);

        /// <summary>
        /// Gets or sets the mosfet type.
        /// </summary>
        /// <value>
        ///     <c>1.0</c> if the model represents an NMOS; <c>-1.0</c> if it is PMOS.
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
        /// Gets the oxide capacitance density.
        /// </summary>
        /// <value>
        /// The oxide capacitance density.
        /// </value>
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
                throw new CircuitException("Invalid oxide thickness: {0}.".FormatString(OxideThickness));

            // Calculate the oxide capacitance
            OxideCapFactor = 3.9 * 8.854214871e-12 / OxideThickness;

            // Calculate the default transconductance
            if (!Transconductance.Given)
                Transconductance.RawValue = SurfaceMobility * 1e-4 * OxideCapFactor; // m^2/cm^2
        }

        /// <summary>
        /// Creates a deep clone of the parameter set.
        /// </summary>
        /// <returns>
        /// A deep clone of the parameter set.
        /// </returns>
        public override ParameterSet Clone()
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
