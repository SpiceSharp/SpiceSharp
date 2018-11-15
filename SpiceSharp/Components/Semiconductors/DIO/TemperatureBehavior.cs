using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Behaviors;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Diode" />.
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
        /// Gets the base configuration of the simulation.
        /// </summary>
        /// <value>
        /// The base configuration.
        /// </value>
        protected BaseConfiguration BaseConfiguration { get; private set; }

        /// <summary>
        /// Extra variables
        /// </summary>
        public double TempJunctionCap { get; protected set; }
        public double TempJunctionPot { get; protected set; }
        public double TempSaturationCurrent { get; protected set; }
        public double TempFactor1 { get; protected set; }
        public double TempDepletionCap { get; protected set; }
        public double TempVCritical { get; protected set; }
        public double TempBreakdownVoltage { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get base configuration
            BaseConfiguration = simulation.Configurations.Get<BaseConfiguration>();

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
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var xcbv = 0.0;

            // loop through all the instances
            if (!BaseParameters.Temperature.Given)
                BaseParameters.Temperature.RawValue = simulation.RealState.Temperature;
            var vt = Circuit.KOverQ * BaseParameters.Temperature;

            // this part gets really ugly - I won't even try to explain these equations
            var fact2 = BaseParameters.Temperature / Circuit.ReferenceTemperature;
            var egfet = 1.16 - 7.02e-4 * BaseParameters.Temperature * BaseParameters.Temperature / (BaseParameters.Temperature + 1108);
            var arg = -egfet / (2 * Circuit.Boltzmann * BaseParameters.Temperature) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature +
                                                                                                                Circuit.ReferenceTemperature));
            var pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Circuit.Charge * arg);
            var egfet1 = 1.16 - 7.02e-4 * ModelParameters.NominalTemperature * ModelParameters.NominalTemperature / (ModelParameters.NominalTemperature + 1108);
            var arg1 = -egfet1 / (Circuit.Boltzmann * 2 * ModelParameters.NominalTemperature) + 1.1150877 / (2 * Circuit.Boltzmann * Circuit.ReferenceTemperature);
            var fact1 = ModelParameters.NominalTemperature / Circuit.ReferenceTemperature;
            var pbfact1 = -2 * ModelTemperature.VtNominal * (1.5 * Math.Log(fact1) + Circuit.Charge * arg1);
            var pbo = (ModelParameters.JunctionPotential - pbfact1) / fact1;
            var gmaold = (ModelParameters.JunctionPotential - pbo) / pbo;
            TempJunctionCap = ModelParameters.JunctionCap / (1 + ModelParameters.GradingCoefficient * (400e-6 * (ModelParameters.NominalTemperature - Circuit.ReferenceTemperature) - gmaold));
            TempJunctionPot = pbfact + fact2 * pbo;
            var gmanew = (TempJunctionPot - pbo) / pbo;
            TempJunctionCap *= 1 + ModelParameters.GradingCoefficient * (400e-6 * (BaseParameters.Temperature - Circuit.ReferenceTemperature) - gmanew);

            TempSaturationCurrent = ModelParameters.SaturationCurrent * Math.Exp((BaseParameters.Temperature / ModelParameters.NominalTemperature - 1) * ModelParameters.ActivationEnergy /
                (ModelParameters.EmissionCoefficient * vt) + ModelParameters.SaturationCurrentExp / ModelParameters.EmissionCoefficient * Math.Log(BaseParameters.Temperature / ModelParameters.NominalTemperature));

            // the defintion of f1, just recompute after temperature adjusting all the variables used in it
            TempFactor1 = TempJunctionPot * (1 - Math.Exp((1 - ModelParameters.GradingCoefficient) * ModelTemperature.Xfc)) / (1 - ModelParameters.GradingCoefficient);

            // same for Depletion Capacitance
            TempDepletionCap = ModelParameters.DepletionCapCoefficient * TempJunctionPot;

            // and Vcrit
            var vte = ModelParameters.EmissionCoefficient * vt;
            TempVCritical = vte * Math.Log(vte / (Circuit.Root2 * TempSaturationCurrent));

            // and now to copute the breakdown voltage, again, using temperature adjusted basic parameters
            if (ModelParameters.BreakdownVoltage.Given)
            {
                double cbv = ModelParameters.BreakdownCurrent;
                double xbv;
                if (cbv < TempSaturationCurrent * ModelParameters.BreakdownVoltage / vt)
                {
                    cbv = TempSaturationCurrent * ModelParameters.BreakdownVoltage / vt;
                    CircuitWarning.Warning(this, "{0}: breakdown current increased to {1:g} to resolve incompatability with specified saturation current".FormatString(Name, cbv));
                    xbv = ModelParameters.BreakdownVoltage;
                }
                else
                {
                    var tol = BaseConfiguration.RelativeTolerance * cbv;
                    xbv = ModelParameters.BreakdownVoltage - vt * Math.Log(1 + cbv / TempSaturationCurrent);
                    int iter;
                    for (iter = 0; iter < 25; iter++)
                    {
                        xbv = ModelParameters.BreakdownVoltage - vt * Math.Log(cbv / TempSaturationCurrent + 1 - xbv / vt);
                        xcbv = TempSaturationCurrent * (Math.Exp((ModelParameters.BreakdownVoltage - xbv) / vt) - 1 + xbv / vt);
                        if (Math.Abs(xcbv - cbv) <= tol)
                            break;
                    }
                    if (iter >= 25)
                        CircuitWarning.Warning(this, "{0}: unable to match forward and reverse diode regions: bv = {1:g}, ibv = {2:g}".FormatString(Name, xbv, xcbv));
                }
                TempBreakdownVoltage = xbv;
            }
        }
    }
}
