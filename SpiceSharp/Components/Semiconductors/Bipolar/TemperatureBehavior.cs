using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="BipolarJunctionTransistor"/>
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
        /// Gets the temperature-modified saturation current.
        /// </summary>
        protected double TempSaturationCurrent { get; private set; }

        /// <summary>
        /// Gets the temperature-modified forward beta.
        /// </summary>
        protected double TempBetaForward { get; private set; }

        /// <summary>
        /// Gets the temperature-modified reverse beta.
        /// </summary>
        protected double TempBetaReverse { get; private set; }

        /// <summary>
        /// Gets the temperature-modified base-emitter saturation current.
        /// </summary>
        protected double TempBeLeakageCurrent { get; private set; }

        /// <summary>
        /// Gets the temperature-modified base-collector saturation current.
        /// </summary>
        protected double TempBcLeakageCurrent { get; private set; }

        /// <summary>
        /// Gets the temperature-modified base-emitter capacitance.
        /// </summary>
        protected double TempBeCap { get; private set; }

        /// <summary>
        /// Gets the temperature-modified base-emitter built-in potential.
        /// </summary>
        protected double TempBePotential { get; private set; }

        /// <summary>
        /// Gets the temperature-modified base-collector capacitance.
        /// </summary>
        protected double TempBcCap { get; private set; }

        /// <summary>
        /// Gets the temperature-modified base-collector built-in potential.
        /// </summary>
        protected double TempBcPotential { get; private set; }

        /// <summary>
        /// Gets the temperature-modified depletion capacitance.
        /// </summary>
        protected double TempDepletionCap { get; private set; }

        /// <summary>
        /// Gets the temperature-modified implementation-specific factor 1.
        /// </summary>
        protected double TempFactor1 { get; private set; }

        /// <summary>
        /// Gets the temperature-modified implementation-specific factor 4.
        /// </summary>
        protected double TempFactor4 { get; private set; }

        /// <summary>
        /// Gets the temperature-modified implementation-specific factor 5.
        /// </summary>
        protected double TempFactor5 { get; private set; }

        /// <summary>
        /// Gets the temperature-modified critical voltage.
        /// </summary>
        protected double TempVCritical { get; private set; }

        /// <summary>
        /// Gets the thermal voltage.
        /// </summary>
        protected double Vt { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        protected BaseSimulationState State { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">Data provider</param>
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
        /// Do temperature-dependent calculations
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            State.ThrowIfNotBound(this);
            if (!BaseParameters.Temperature.Given)
                BaseParameters.Temperature.RawValue = State.Temperature;
            Vt = BaseParameters.Temperature * Constants.KOverQ;
            var fact2 = BaseParameters.Temperature / Constants.ReferenceTemperature;
            var egfet = 1.16 - 7.02e-4 * BaseParameters.Temperature * BaseParameters.Temperature / (BaseParameters.Temperature + 1108);
            var arg = -egfet / (2 * Constants.Boltzmann * BaseParameters.Temperature) + 1.1150877 / (Constants.Boltzmann * (Constants.ReferenceTemperature +
                                                                                                                Constants.ReferenceTemperature));
            var pbfact = -2 * Vt * (1.5 * Math.Log(fact2) + Constants.Charge * arg);

            var ratlog = Math.Log(BaseParameters.Temperature / ModelParameters.NominalTemperature);
            var ratio1 = BaseParameters.Temperature / ModelParameters.NominalTemperature - 1;
            var factlog = ratio1 * ModelParameters.EnergyGap / Vt + ModelParameters.TempExpIs * ratlog;
            var factor = Math.Exp(factlog);
            TempSaturationCurrent = ModelParameters.SatCur * factor;
            var bfactor = Math.Exp(ratlog * ModelParameters.BetaExponent);
            TempBetaForward = ModelParameters.BetaF * bfactor;
            TempBetaReverse = ModelParameters.BetaR * bfactor;
            TempBeLeakageCurrent = ModelParameters.LeakBeCurrent * Math.Exp(factlog / ModelParameters.LeakBeEmissionCoefficient) / bfactor;
            TempBcLeakageCurrent = ModelParameters.LeakBcCurrent * Math.Exp(factlog / ModelParameters.LeakBcEmissionCoefficient) / bfactor;

            var pbo = (ModelParameters.PotentialBe - pbfact) / ModelTemperature.Factor1;
            var gmaold = (ModelParameters.PotentialBe - pbo) / pbo;
            TempBeCap = ModelParameters.DepletionCapBe / (1 + ModelParameters.JunctionExpBe * (4e-4 * (ModelParameters.NominalTemperature - Constants.ReferenceTemperature) - gmaold));
            TempBePotential = fact2 * pbo + pbfact;
            var gmanew = (TempBePotential - pbo) / pbo;
            TempBeCap *= 1 + ModelParameters.JunctionExpBe * (4e-4 * (BaseParameters.Temperature - Constants.ReferenceTemperature) - gmanew);

            pbo = (ModelParameters.PotentialBc - pbfact) / ModelTemperature.Factor1;
            gmaold = (ModelParameters.PotentialBc - pbo) / pbo;
            TempBcCap = ModelParameters.DepletionCapBc / (1 + ModelParameters.JunctionExpBc * (4e-4 * (ModelParameters.NominalTemperature - Constants.ReferenceTemperature) - gmaold));
            TempBcPotential = fact2 * pbo + pbfact;
            gmanew = (TempBcPotential - pbo) / pbo;
            TempBcCap *= 1 + ModelParameters.JunctionExpBc * (4e-4 * (BaseParameters.Temperature - Constants.ReferenceTemperature) - gmanew);

            TempDepletionCap = ModelParameters.DepletionCapCoefficient * TempBePotential;
            TempFactor1 = TempBePotential * (1 - Math.Exp((1 - ModelParameters.JunctionExpBe) * ModelTemperature.Xfc)) / (1 - ModelParameters.JunctionExpBe);
            TempFactor4 = ModelParameters.DepletionCapCoefficient * TempBcPotential;
            TempFactor5 = TempBcPotential * (1 - Math.Exp((1 - ModelParameters.JunctionExpBc) * ModelTemperature.Xfc)) / (1 - ModelParameters.JunctionExpBc);
            TempVCritical = Vt * Math.Log(Vt / (Constants.Root2 * ModelParameters.SatCur));
        }
    }
}
