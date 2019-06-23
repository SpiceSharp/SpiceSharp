using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Behaviors;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class TemperatureBehavior : ExportingBehavior, ITemperatureBehavior
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        /// <value>
        /// The base parameters.
        /// </value>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        /// <value>
        /// The model parameters.
        /// </value>
        protected ModelBaseParameters ModelParameters { get; private set; }

        /// <summary>
        /// Gets the model temperature behavior.
        /// </summary>
        /// <value>
        /// The model temperature behavior.
        /// </value>
        protected ModelTemperatureBehavior ModelTemperature { get; private set; }

        /// <summary>
        /// Shared parameters
        /// </summary>
        protected double TempSaturationCurrent { get; private set; }
        protected double TempBetaForward { get; private set; }
        protected double TempBetaReverse { get; private set; }
        protected double TempBeLeakageCurrent { get; private set; }
        protected double TempBcLeakageCurrent { get; private set; }
        protected double TempBeCap { get; private set; }
        protected double TempBePotential { get; private set; }
        protected double TempBcCap { get; private set; }
        protected double TempBcPotential { get; private set; }
        protected double TempDepletionCap { get; private set; }
        protected double TempFactor1 { get; private set; }
        protected double TempFactor4 { get; private set; }
        protected double TempFactor5 { get; private set; }
        protected double TempVCritical { get; private set; }
        protected double Vt { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            provider.ThrowIfNull(nameof(provider));

            // Get parameters
            BaseParameters = provider.GetParameterSet<BaseParameters>();
            ModelParameters = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            ModelTemperature = provider.GetBehavior<ModelTemperatureBehavior>("model");
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public void Temperature(BaseSimulation simulation)
        {
			simulation.ThrowIfNull(nameof(simulation));

            if (!BaseParameters.Temperature.Given)
                BaseParameters.Temperature.RawValue = simulation.RealState.Temperature;
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
