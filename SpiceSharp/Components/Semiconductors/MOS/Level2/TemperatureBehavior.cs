using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
{
    /// <summary>
    /// Temperature behavior for a <see cref="Mosfet2"/>
    /// </summary>
    public class TemperatureBehavior : Behavior, ITemperatureBehavior
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        protected ModelBaseParameters ModelParameters { get; private set; }

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
        /// Gets the state.
        /// </summary>
        protected BaseSimulationState State { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Bind the behavior. for the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            // Get parameters
            BaseParameters = context.GetParameterSet<BaseParameters>();
            ModelParameters = context.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            ModelTemperature = context.GetBehavior<ModelTemperatureBehavior>("model");

            State = ((BaseSimulation)simulation).RealState;
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();

            State = null;
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            // Update the width and length if they are not given and if the model specifies them
            if (!BaseParameters.Width.Given && ModelParameters.Width.Given)
                BaseParameters.Width.RawValue = ModelParameters.Width.Value;
            if (!BaseParameters.Length.Given && ModelParameters.Length.Given)
                BaseParameters.Length.RawValue = ModelParameters.Length.Value;

            if (!BaseParameters.Temperature.Given)
                BaseParameters.Temperature.RawValue = State.ThrowIfNotBound(this).Temperature;
            Vt = BaseParameters.Temperature * Constants.KOverQ;
            var ratio = BaseParameters.Temperature / ModelParameters.NominalTemperature;
            var fact2 = BaseParameters.Temperature / Constants.ReferenceTemperature;
            var kt = BaseParameters.Temperature * Constants.Boltzmann;
            var egfet = 1.16 - 7.02e-4 * BaseParameters.Temperature * BaseParameters.Temperature / (BaseParameters.Temperature + 1108);
            var arg = -egfet / (kt + kt) + 1.1150877 / (Constants.Boltzmann * (Constants.ReferenceTemperature + Constants.ReferenceTemperature));
            var pbfact = -2 * Vt * (1.5 * Math.Log(fact2) + Constants.Charge * arg);

            if (ModelParameters.DrainResistance.Given)
            {
                if (!ModelParameters.DrainResistance.Value.Equals(0.0))
                    DrainConductance = 1 / ModelParameters.DrainResistance;
                else
                    DrainConductance = 0;
            }
            else if (ModelParameters.SheetResistance.Given)
            {
                if (!ModelParameters.SheetResistance.Value.Equals(0.0))
                    DrainConductance = 1 / (ModelParameters.SheetResistance * BaseParameters.DrainSquares);
                else
                    DrainConductance = 0;
            }
            else
                DrainConductance = 0;
            if (ModelParameters.SourceResistance.Given)
            {
                if (!ModelParameters.SourceResistance.Value.Equals(0.0))
                    SourceConductance = 1 / ModelParameters.SourceResistance;
                else
                    SourceConductance = 0;
            }
            else if (ModelParameters.SheetResistance.Given)
            {
                if (!ModelParameters.SheetResistance.Value.Equals(0.0))
                    SourceConductance = 1 / (ModelParameters.SheetResistance * BaseParameters.SourceSquares);
                else
                    SourceConductance = 0;
            }
            else
                SourceConductance = 0;

            if (BaseParameters.Length - 2 * ModelParameters.LateralDiffusion <= 0)
                CircuitWarning.Warning(this, "{0}: effective channel length less than zero".FormatString(Name));
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

            if (tempSaturationCurrentDensity <= 0 || BaseParameters.DrainArea.Value <= 0 || BaseParameters.SourceArea.Value <= 0)
            {
                SourceVCritical = DrainVCritical = Vt * Math.Log(Vt / (Constants.Root2 * tempSaturationCurrent));
            }
            else
            {
                DrainVCritical = Vt * Math.Log(Vt / (Constants.Root2 * tempSaturationCurrentDensity * BaseParameters.DrainArea));
                SourceVCritical = Vt * Math.Log(Vt / (Constants.Root2 * tempSaturationCurrentDensity * BaseParameters.SourceArea));
            }

            if (tempSaturationCurrentDensity.Equals(0) || BaseParameters.DrainArea.Value <= 0 || BaseParameters.SourceArea.Value <= 0)
            {
                DrainSatCurrent = tempSaturationCurrent;
                SourceSatCurrent = tempSaturationCurrent;
            }
            else
            {
                DrainSatCurrent = tempSaturationCurrentDensity * BaseParameters.DrainArea;
                SourceSatCurrent = tempSaturationCurrentDensity * BaseParameters.SourceArea;
            }
        }
    }
}
