using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Diode" />.
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
        /// Gets the base configuration of the simulation.
        /// </summary>
        protected BaseConfiguration BaseConfiguration { get; private set; }

        /// <summary>
        /// Gets the temperature-modified junction capacitance.
        /// </summary>
        public double TempJunctionCap { get; protected set; }

        /// <summary>
        /// Gets the temperature-modified junction built-in potential.
        /// </summary>
        public double TempJunctionPot { get; protected set; }

        /// <summary>
        /// Gets the temperature-modified saturation current.
        /// </summary>
        public double TempSaturationCurrent { get; protected set; }

        /// <summary>
        /// Gets the temperature-modified implementation-specific factor 1.
        /// </summary>
        public double TempFactor1 { get; protected set; }

        /// <summary>
        /// Gets the temperature-modified depletion capacitance.
        /// </summary>
        public double TempDepletionCap { get; protected set; }

        /// <summary>
        /// Gets the temperature-modified critical voltage.
        /// </summary>
        public double TempVCritical { get; protected set; }

        /// <summary>
        /// Gets the temperature-modified breakdown voltage.
        /// </summary>
        public double TempBreakdownVoltage { get; protected set; }

        /// <summary>
        /// Gets the thermal voltage.
        /// </summary>
        protected double Vt { get; private set; }

        /// <summary>
        /// Gets the temperature-modified and emission-modified thermal voltage.
        /// </summary>
        protected double Vte { get; private set; }

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
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            // Get base configuration
            BaseConfiguration = simulation.Configurations.Get<BaseConfiguration>();

            // Get parameters
            BaseParameters = context.GetParameterSet<BaseParameters>();
            ModelParameters = context.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            ModelTemperature = context.GetBehavior<ModelTemperatureBehavior>("model");

            State = ((BaseSimulation)simulation).RealState;
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            State.ThrowIfNotBound(this);
            var xcbv = 0.0;

            // loop through all the instances
            if (!BaseParameters.Temperature.Given)
                BaseParameters.Temperature.RawValue = State.Temperature;
            Vt = Constants.KOverQ * BaseParameters.Temperature;
            Vte = ModelParameters.EmissionCoefficient * Vt;

            // this part gets really ugly - I won't even try to explain these equations
            var fact2 = BaseParameters.Temperature / Constants.ReferenceTemperature;
            var egfet = 1.16 - 7.02e-4 * BaseParameters.Temperature * BaseParameters.Temperature / (BaseParameters.Temperature + 1108);
            var arg = -egfet / (2 * Constants.Boltzmann * BaseParameters.Temperature) + 1.1150877 / (Constants.Boltzmann * (Constants.ReferenceTemperature +
                                                                                                                Constants.ReferenceTemperature));
            var pbfact = -2 * Vt * (1.5 * Math.Log(fact2) + Constants.Charge * arg);
            var egfet1 = 1.16 - 7.02e-4 * ModelParameters.NominalTemperature * ModelParameters.NominalTemperature / (ModelParameters.NominalTemperature + 1108);
            var arg1 = -egfet1 / (Constants.Boltzmann * 2 * ModelParameters.NominalTemperature) + 1.1150877 / (2 * Constants.Boltzmann * Constants.ReferenceTemperature);
            var fact1 = ModelParameters.NominalTemperature / Constants.ReferenceTemperature;
            var pbfact1 = -2 * ModelTemperature.VtNominal * (1.5 * Math.Log(fact1) + Constants.Charge * arg1);
            var pbo = (ModelParameters.JunctionPotential - pbfact1) / fact1;
            var gmaold = (ModelParameters.JunctionPotential - pbo) / pbo;
            TempJunctionCap = ModelParameters.JunctionCap / (1 + ModelParameters.GradingCoefficient * (400e-6 * (ModelParameters.NominalTemperature - Constants.ReferenceTemperature) - gmaold));
            TempJunctionPot = pbfact + fact2 * pbo;
            var gmanew = (TempJunctionPot - pbo) / pbo;
            TempJunctionCap *= 1 + ModelParameters.GradingCoefficient * (400e-6 * (BaseParameters.Temperature - Constants.ReferenceTemperature) - gmanew);

            TempSaturationCurrent = ModelParameters.SaturationCurrent * Math.Exp((BaseParameters.Temperature / ModelParameters.NominalTemperature - 1) * ModelParameters.ActivationEnergy /
                (ModelParameters.EmissionCoefficient * Vt) + ModelParameters.SaturationCurrentExp / ModelParameters.EmissionCoefficient * Math.Log(BaseParameters.Temperature / ModelParameters.NominalTemperature));

            // the defintion of f1, just recompute after temperature adjusting all the variables used in it
            TempFactor1 = TempJunctionPot * (1 - Math.Exp((1 - ModelParameters.GradingCoefficient) * ModelTemperature.Xfc)) / (1 - ModelParameters.GradingCoefficient);

            // same for Depletion Capacitance
            TempDepletionCap = ModelParameters.DepletionCapCoefficient * TempJunctionPot;

            // and Vcrit
            var vte = ModelParameters.EmissionCoefficient * Vt;
            TempVCritical = vte * Math.Log(vte / (Constants.Root2 * TempSaturationCurrent));

            // and now to copute the breakdown voltage, again, using temperature adjusted basic parameters
            if (ModelParameters.BreakdownVoltage.Given)
            {
                double cbv = ModelParameters.BreakdownCurrent;
                double xbv;
                if (cbv < TempSaturationCurrent * ModelParameters.BreakdownVoltage / Vt)
                {
                    cbv = TempSaturationCurrent * ModelParameters.BreakdownVoltage / Vt;
                    CircuitWarning.Warning(this, "{0}: breakdown current increased to {1:g} to resolve incompatability with specified saturation current".FormatString(Name, cbv));
                    xbv = ModelParameters.BreakdownVoltage;
                }
                else
                {
                    var tol = BaseConfiguration.RelativeTolerance * cbv;
                    xbv = ModelParameters.BreakdownVoltage - Vt * Math.Log(1 + cbv / TempSaturationCurrent);
                    int iter;
                    for (iter = 0; iter < 25; iter++)
                    {
                        xbv = ModelParameters.BreakdownVoltage - Vt * Math.Log(cbv / TempSaturationCurrent + 1 - xbv / Vt);
                        xcbv = TempSaturationCurrent * (Math.Exp((ModelParameters.BreakdownVoltage - xbv) / Vt) - 1 + xbv / Vt);
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
