using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
{
    /// <summary>
    /// Temperature behavior for a <see cref="Mosfet2"/>
    /// </summary>
    public class TemperatureBehavior : Behavior, ITemperatureBehavior,
        IParameterized<BaseParameters>
    {
        private readonly ITemperatureSimulationState _temperature;

        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        /// <value>
        /// The base parameters.
        /// </value>
        public BaseParameters Parameters { get; }

        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        /// <value>
        /// The model parameters.
        /// </value>
        protected ModelBaseParameters ModelParameters { get; }

        /// <summary>
        /// Gets the model temperature behavior.
        /// </summary>
        protected ModelTemperatureBehavior ModelTemperature { get; private set; }

        /// <summary>
        /// Gets the source conductance.
        /// </summary>
        [ParameterName("sourceconductance"), ParameterInfo("Conductance at the source")]
        public double SourceConductance { get; private set; }

        /// <summary>
        /// Gets the drain conductance.
        /// </summary>
        [ParameterName("drainconductance"), ParameterInfo("Conductance at the drain")]
        public double DrainConductance { get; private set; }

        /// <summary>
        /// Gets the source resistance.
        /// </summary>
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
        [ParameterName("sourcevcrit"), ParameterInfo("Critical source voltage")]
        public double SourceVCritical { get; protected set; }

        /// <summary>
        /// Gets or sets the critical drain voltage.
        /// </summary>
        [ParameterName("drainvcrit"), ParameterInfo("Critical drain voltage")]
        public double DrainVCritical { get; protected set; }

        /// <summary>
        /// Gets the temperature-modified surface mobility.
        /// </summary>
        protected double TempSurfaceMobility { get; private set; }

        /// <summary>
        /// Gets the temperature-modified phi.
        /// </summary>
        protected double TempPhi { get; private set; }

        /// <summary>
        /// Gets the temperature-modified Vbi.
        /// </summary>
        protected double TempVoltageBi { get; private set; }

        /// <summary>
        /// Gets the temperature-modified bulk potential.
        /// </summary>
        protected double TempBulkPotential { get; private set; }

        /// <summary>
        /// Gets the temperature-modified transconductance.
        /// </summary>
        protected double TempTransconductance { get; private set; }

        /// <summary>
        /// Gets the temperature-modified threshold voltage.
        /// </summary>
        protected double TempVt0 { get; private set; }

        /// <summary>
        /// Gets the thermal voltage.
        /// </summary>
        protected double Vt { get; private set; }

        /// <summary>
        /// Gets the temperature-modified drain saturation current.
        /// </summary>
        protected double DrainSatCurrent { get; private set; }

        /// <summary>
        /// Gets the temperature-modified source saturation current.
        /// </summary>
        protected double SourceSatCurrent { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TemperatureBehavior(string name, ComponentBindingContext context) : base(name)
        {
            context.ThrowIfNull(nameof(context));
            _temperature = context.GetState<ITemperatureSimulationState>();
            ModelTemperature = context.ModelBehaviors.GetValue<ModelTemperatureBehavior>();
            ModelParameters = ModelTemperature.GetParameterSet<ModelBaseParameters>();
            Parameters = context.GetParameterSet<BaseParameters>();
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            // Update the width and length if they are not given and if the model specifies them
            if (!Parameters.Width.Given && ModelParameters.Width.Given)
                Parameters.Width.RawValue = ModelParameters.Width.Value;
            if (!Parameters.Length.Given && ModelParameters.Length.Given)
                Parameters.Length.RawValue = ModelParameters.Length.Value;

            if (!Parameters.Temperature.Given)
                Parameters.Temperature.RawValue = _temperature.Temperature;
            Vt = Parameters.Temperature * Constants.KOverQ;
            var ratio = Parameters.Temperature / ModelParameters.NominalTemperature;
            var fact2 = Parameters.Temperature / Constants.ReferenceTemperature;
            var kt = Parameters.Temperature * Constants.Boltzmann;
            var egfet = 1.16 - 7.02e-4 * Parameters.Temperature * Parameters.Temperature / (Parameters.Temperature + 1108);
            var arg = -egfet / (kt + kt) + 1.1150877 / (Constants.Boltzmann * (Constants.ReferenceTemperature + Constants.ReferenceTemperature));
            var pbfact = -2 * Vt * (1.5 * Math.Log(fact2) + Constants.Charge * arg);

            if (ModelParameters.DrainResistance > 0)
                    DrainConductance = 1 / ModelParameters.DrainResistance;
            else if (ModelParameters.SheetResistance > 0)
                DrainConductance = 1 / (ModelParameters.SheetResistance * Parameters.DrainSquares);
            else
                DrainConductance = 0;
            if (ModelParameters.SourceResistance > 0)
                SourceConductance = 1 / ModelParameters.SourceResistance;
            else if (ModelParameters.SheetResistance > 0)
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

            if (tempSaturationCurrentDensity.Equals(0) || Parameters.DrainArea <= 0 || Parameters.SourceArea <= 0)
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
