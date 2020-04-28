using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Mosfets.Level1
{
    /// <summary>
    /// Temperature behavior for a <see cref="Mosfet1"/>.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="ITemperatureBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    public class Temperature : Behavior, 
        ITemperatureBehavior,
        IParameterized<Parameters>
    {
        private readonly ITemperatureSimulationState _temperature;

        /// <inheritdoc/>
        public Parameters Parameters { get; } = new Parameters();

        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        protected readonly ModelParameters ModelParameters;

        /// <summary>
        /// Gets the model temperature behavior.
        /// </summary>
        protected readonly ModelTemperature ModelTemperature;

        /// <summary>
        /// Gets the small source conductance.
        /// </summary>
        /// <value>
        /// The source conductance.
        /// </value>
        [ParameterName("sourceconductance"), ParameterInfo("Conductance at the source")]
        public double SourceConductance { get; private set; }

        /// <summary>
        /// Gets the drain conductance.
        /// </summary>
        /// <value>
        /// The drain conductance.
        /// </value>
        [ParameterName("drainconductance"), ParameterInfo("Conductance at the drain")]
        public double DrainConductance { get; private set; }

        /// <summary>
        /// Gets the source resistance.
        /// </summary>
        /// <value>
        /// The source resistance.
        /// </value>
        [ParameterName("rs"), ParameterInfo("Source resistance")]
        public double SourceResistance
        {
            get
            {
                if (SourceConductance > 0.0)
                    return 1.0 / SourceConductance;
                return 0.0;
            }
        }

        /// <summary>
        /// Gets the drain resistance.
        /// </summary>
        /// <value>
        /// The drain resistance.
        /// </value>
        [ParameterName("rd"), ParameterInfo("Drain conductance")]
        public double DrainResistance
        {
            get
            {
                if (DrainConductance > 0.0)
                    return 1.0 / DrainConductance;
                return 0.0;
            }
        }

        /// <summary>
        /// Gets or sets the critical source voltage.
        /// </summary>
        /// <value>
        /// The critical source voltage.
        /// </value>
        [ParameterName("sourcevcrit"), ParameterInfo("Critical source voltage")]
        public double SourceVCritical { get; protected set; }

        /// <summary>
        /// Gets or sets the critical drain voltage.
        /// </summary>
        /// <value>
        /// The critical drain voltage.
        /// </value>
        [ParameterName("drainvcrit"), ParameterInfo("Critical drain voltage")]
        public double DrainVCritical { get; protected set; }

        /// <summary>
        /// Gets the temperature-modified surface mobility.
        /// </summary>
        /// <value>
        /// The temperature-modified surface mobility.
        /// </value>
        protected double TempSurfaceMobility { get; private set; }

        /// <summary>
        /// Gets the temperature-modified phi.
        /// </summary>
        /// <value>
        /// The temperature-modified phi.
        /// </value>
        protected double TempPhi { get; private set; }

        /// <summary>
        /// Gets the temperature-modified Vbi.
        /// </summary>
        /// <value>
        /// The temperature-modified Vbi.
        /// </value>
        protected double TempVoltageBi { get; private set; }

        /// <summary>
        /// Gets the temperature-modified bulk potential.
        /// </summary>
        /// <value>
        /// The temperature-modified bulk potential.
        /// </value>
        protected double TempBulkPotential { get; private set; }

        /// <summary>
        /// Gets the temperature-modified transconductance.
        /// </summary>
        /// <value>
        /// The temperature-modified transconductance.
        /// </value>
        protected double TempTransconductance { get; private set; }

        /// <summary>
        /// Gets the temperature-modified threshold voltage.
        /// </summary>
        /// <value>
        /// The temperature-modified thermal voltage.
        /// </value>
        protected double TempVt0 { get; private set; }

        /// <summary>
        /// Gets the thermal voltage.
        /// </summary>
        /// <value>
        /// The thermal voltage.
        /// </value>
        protected double Vt { get; private set; }

        /// <summary>
        /// Gets the temperature-modified drain saturation current.
        /// </summary>
        /// <value>
        /// The temperature-modified drain saturation current.
        /// </value>
        protected double DrainSatCurrent { get; private set; }

        /// <summary>
        /// Gets the temperature-modified source saturation current.
        /// </summary>
        /// <value>
        /// The temperature-modified source saturation current.
        /// </value>
        protected double SourceSatCurrent { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Temperature"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public Temperature(string name, ComponentBindingContext context) : base(name)
        {
            context.ThrowIfNull(nameof(context));
            _temperature = context.GetState<ITemperatureSimulationState>();
            ModelParameters = context.ModelBehaviors.GetParameterSet<ModelParameters>();
            ModelTemperature = context.ModelBehaviors.GetValue<ModelTemperature>();
            Parameters = context.GetParameterSet<Parameters>();
        }

        void ITemperatureBehavior.Temperature()
        {
            // Update the width and length if they are not given and if the model specifies them
            if (!Parameters.Width.Given && ModelParameters.Width.Given)
                Parameters.Width = new GivenParameter<double>(ModelParameters.Width.Value, false);
            if (!Parameters.Length.Given && ModelParameters.Length.Given)
                Parameters.Length = new GivenParameter<double>(ModelParameters.Length.Value, false);

            if (!Parameters.Temperature.Given)
                Parameters.Temperature = new GivenParameter<double>(_temperature.Temperature, false);
            Vt = Parameters.Temperature * Constants.KOverQ;
            var ratio = Parameters.Temperature / ModelParameters.NominalTemperature;
            var fact2 = Parameters.Temperature / Constants.ReferenceTemperature;
            var kt = Parameters.Temperature * Constants.Boltzmann;
            var egfet = 1.16 - 7.02e-4 * Parameters.Temperature * Parameters.Temperature / (Parameters.Temperature + 1108);
            var arg = -egfet / (kt + kt) + 1.1150877 / (Constants.Boltzmann * (Constants.ReferenceTemperature + Constants.ReferenceTemperature));
            var pbfact = -2 * Vt * (1.5 * Math.Log(fact2) + Constants.Charge * arg);

            if (!ModelParameters.DrainResistance.Equals(0.0))
                DrainConductance = 1 / ModelParameters.DrainResistance;
            else if (!ModelParameters.SheetResistance.Equals(0.0))
                DrainConductance = 1 / (ModelParameters.SheetResistance * Parameters.DrainSquares);
            else
                DrainConductance = 0;
            if (!ModelParameters.SourceResistance.Equals(0.0))
                SourceConductance = 1 / ModelParameters.SourceResistance;
            else if (!ModelParameters.SheetResistance.Equals(0.0))
                SourceConductance = 1 / (ModelParameters.SheetResistance * Parameters.SourceSquares);
            else
                SourceConductance = 0;

            if (Parameters.Length - 2 * ModelParameters.LateralDiffusion <= 0)
                SpiceSharpWarning.Warning(this, Properties.Resources.Mosfets_EffectiveChannelTooSmall.FormatString(Name));
            var ratio4 = ratio * Math.Sqrt(ratio);
            TempTransconductance = ModelParameters.Transconductance / ratio4;
            TempSurfaceMobility = ModelParameters.SurfaceMobility / ratio4;
            var phio = (ModelParameters.Phi - ModelTemperature.PbFactor1) / ModelTemperature.Factor1;
            TempPhi = fact2 * phio + pbfact;
            TempVoltageBi = ModelParameters.Vt0 - ModelParameters.MosfetType * (ModelParameters.Gamma * Math.Sqrt(ModelParameters.Phi)) + .5 * (ModelTemperature.EgFet1 - egfet) +
                ModelParameters.MosfetType * .5 * (TempPhi - ModelParameters.Phi);
            TempVt0 = TempVoltageBi + ModelParameters.MosfetType * ModelParameters.Gamma * Math.Sqrt(TempPhi);
            var tempSaturationCurrent = ModelParameters.JunctionSatCur * Math.Exp(-egfet / Vt + ModelTemperature.EgFet1 / ModelTemperature.VtNominal);
            var tempSaturationCurrentDensity = ModelParameters.JunctionSatCurDensity * Math.Exp(-egfet / Vt + ModelTemperature.EgFet1 / ModelTemperature.VtNominal);
            var pbo = (ModelParameters.BulkJunctionPotential - ModelTemperature.PbFactor1) / ModelTemperature.Factor1;
            TempBulkPotential = fact2 * pbo + pbfact;

            if (tempSaturationCurrentDensity <= 0 || Parameters.DrainArea <= 0 || Parameters.SourceArea <= 0)
            {
                SourceVCritical = DrainVCritical = Vt * Math.Log(Vt / (Constants.Root2 * tempSaturationCurrent));
            }
            else
            {
                DrainVCritical = Vt * Math.Log(Vt / (Constants.Root2 * tempSaturationCurrentDensity * Parameters.DrainArea));
                SourceVCritical = Vt * Math.Log(Vt / (Constants.Root2 * tempSaturationCurrentDensity * Parameters.SourceArea));
            }

            if (tempSaturationCurrentDensity.Equals(0.0) || Parameters.DrainArea <= 0 || Parameters.SourceArea <= 0)
            {
                DrainSatCurrent = tempSaturationCurrent;
                SourceSatCurrent = tempSaturationCurrent;
            }
            else
            {
                DrainSatCurrent = tempSaturationCurrentDensity * Parameters.DrainArea;
                SourceSatCurrent = tempSaturationCurrentDensity * Parameters.SourceArea;
            }
        }
    }
}
