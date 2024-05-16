using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Diodes
{
    /// <summary>
    /// Temperature behavior for a <see cref="Diode" />.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="ITemperatureBehavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="Diodes.Parameters"/>
    [BehaviorFor(typeof(Diode)), AddBehaviorIfNo(typeof(ITemperatureBehavior))]
    public class Temperature : Behavior,
        ITemperatureBehavior,
        IParameterized<Parameters>
    {
        private readonly ITemperatureSimulationState _temperature;

        /// <summary>
        /// Gets the biasing parameters.
        /// </summary>
        /// <value>
        /// The biasing parameters.
        /// </value>
        protected BiasingParameters BiasingParameters { get; }

        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        /// <value>
        /// The model parameters.
        /// </value>
        protected ModelParameters ModelParameters { get; }

        /// <inheritdoc/>
        public Parameters Parameters { get; }

        /// <summary>
        /// Gets the model temperature behavior.
        /// </summary>
        protected ModelTemperature ModelTemperature { get; private set; }

        /// <summary>
        /// Gets the temperature-modified junction capacitance.
        /// </summary>
        /// <value>
        /// The temperature-modified junction capacitance.
        /// </value>
        public double TempJunctionCap { get; protected set; }

        /// <summary>
        /// Gets the temperature-modified junction built-in potential.
        /// </summary>
        /// <value>
        /// The temperature-modified junction built-in potential.
        /// </value>
        public double TempJunctionPot { get; protected set; }

        /// <summary>
        /// Gets the temperature-modified saturation current.
        /// </summary>
        /// <value>
        /// The temperature-modified saturation current.
        /// </value>
        public double TempSaturationCurrent { get; protected set; }

        /// <summary>
        /// Gets the temperature-modified implementation-specific factor 1.
        /// </summary>
        /// <value>
        /// The temperature-modified implementation-specific factor 1.
        /// </value>
        public double TempFactor1 { get; protected set; }

        /// <summary>
        /// Gets the temperature-modified depletion capacitance.
        /// </summary>
        /// <value>
        /// The temperature-modified depletion capacitance.
        /// </value>
        public double TempDepletionCap { get; protected set; }

        /// <summary>
        /// Gets the temperature-modified critical voltage.
        /// </summary>
        /// <value>
        /// The temperature-modified critical voltage.
        /// </value>
        public double TempVCritical { get; protected set; }

        /// <summary>
        /// Gets the temperature-modified breakdown voltage.
        /// </summary>
        /// <value>
        /// The temperature-modified breakdown voltage.
        /// </value>
        public double TempBreakdownVoltage { get; protected set; }

        /// <summary>
        /// Gets the thermal voltage.
        /// </summary>
        /// <value>
        /// The thermal voltage.
        /// </value>
        protected double Vt { get; private set; }

        /// <summary>
        /// Gets the temperature-modified and emission-modified thermal voltage.
        /// </summary>
        /// <value>
        /// The temperature-modified and emission-modified thermal voltage.
        /// </value>
        protected double Vte { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Temperature"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Temperature(IComponentBindingContext context)
            : base(context)
        {
            context.ThrowIfNull(nameof(context));
            _temperature = context.GetState<ITemperatureSimulationState>();
            ModelParameters = context.ModelBehaviors.GetParameterSet<ModelParameters>();
            ModelTemperature = context.ModelBehaviors.GetValue<ModelTemperature>();
            BiasingParameters = context.GetSimulationParameterSet<BiasingParameters>();
            Parameters = context.GetParameterSet<Parameters>();
        }

        /// <inheritdoc/>
        void ITemperatureBehavior.Temperature()
        {
            double xcbv = 0.0;

            // loop through all the instances
            if (!Parameters.Temperature.Given)
                Parameters.Temperature = new GivenParameter<double>(_temperature.Temperature, false);
            Vt = Constants.KOverQ * Parameters.Temperature;
            Vte = ModelParameters.EmissionCoefficient * Vt;

            // this part gets really ugly - I won't even try to explain these equations
            double fact2 = Parameters.Temperature / Constants.ReferenceTemperature;
            double egfet = 1.16 - 7.02e-4 * Parameters.Temperature * Parameters.Temperature / (Parameters.Temperature + 1108);
            double arg = -egfet / (2 * Constants.Boltzmann * Parameters.Temperature) + 1.1150877 / (Constants.Boltzmann * (Constants.ReferenceTemperature +
                                                                                                                Constants.ReferenceTemperature));
            double pbfact = -2 * Vt * (1.5 * Math.Log(fact2) + Constants.Charge * arg);
            double egfet1 = 1.16 - 7.02e-4 * ModelParameters.NominalTemperature * ModelParameters.NominalTemperature / (ModelParameters.NominalTemperature + 1108);
            double arg1 = -egfet1 / (Constants.Boltzmann * 2 * ModelParameters.NominalTemperature) + 1.1150877 / (2 * Constants.Boltzmann * Constants.ReferenceTemperature);
            double fact1 = ModelParameters.NominalTemperature / Constants.ReferenceTemperature;
            double pbfact1 = -2 * ModelTemperature.VtNominal * (1.5 * Math.Log(fact1) + Constants.Charge * arg1);
            double pbo = (ModelParameters.JunctionPotential - pbfact1) / fact1;
            double gmaold = (ModelParameters.JunctionPotential - pbo) / pbo;
            TempJunctionCap = ModelParameters.JunctionCap / (1 + ModelParameters.GradingCoefficient * (400e-6 * (ModelParameters.NominalTemperature - Constants.ReferenceTemperature) - gmaold));
            TempJunctionPot = pbfact + fact2 * pbo;
            double gmanew = (TempJunctionPot - pbo) / pbo;
            TempJunctionCap *= 1 + ModelParameters.GradingCoefficient * (400e-6 * (Parameters.Temperature - Constants.ReferenceTemperature) - gmanew);

            TempSaturationCurrent = ModelParameters.SaturationCurrent * Math.Exp((Parameters.Temperature / ModelParameters.NominalTemperature - 1) * ModelParameters.ActivationEnergy /
                (ModelParameters.EmissionCoefficient * Vt) + ModelParameters.SaturationCurrentExp / ModelParameters.EmissionCoefficient * Math.Log(Parameters.Temperature / ModelParameters.NominalTemperature));

            // the defintion of f1, just recompute after temperature adjusting all the variables used in it
            TempFactor1 = TempJunctionPot * (1 - Math.Exp((1 - ModelParameters.GradingCoefficient) * ModelTemperature.Xfc)) / (1 - ModelParameters.GradingCoefficient);

            // same for Depletion Capacitance
            TempDepletionCap = ModelParameters.DepletionCapCoefficient * TempJunctionPot;

            // and Vcrit
            double vte = ModelParameters.EmissionCoefficient * Vt;
            TempVCritical = vte * Math.Log(vte / (Constants.Root2 * TempSaturationCurrent));

            // and now to copute the breakdown voltage, again, using temperature adjusted basic parameters
            if (ModelParameters.BreakdownVoltage.Given)
            {
                double vbv = Math.Abs(ModelParameters.BreakdownVoltage);
                double cbv = Math.Abs(ModelParameters.BreakdownCurrent);
                double xbv;
                if (cbv < TempSaturationCurrent * vbv / Vt)
                {
                    cbv = TempSaturationCurrent * vbv / Vt;
                    SpiceSharpWarning.Warning(this,
                        Properties.Resources.Diodes_BreakdownCurrentIncreased.FormatString(Name, cbv));
                    xbv = vbv;
                }
                else
                {
                    double tol = BiasingParameters.RelativeTolerance * cbv;
                    xbv = vbv - Vt * Math.Log(1 + cbv / TempSaturationCurrent);
                    int iter;
                    for (iter = 0; iter < 25; iter++)
                    {
                        xbv = vbv - Vt * Math.Log(cbv / TempSaturationCurrent + 1 - xbv / Vt);
                        xcbv = TempSaturationCurrent * (Math.Exp((vbv - xbv) / Vt) - 1 + xbv / Vt);
                        if (Math.Abs(xcbv - cbv) <= tol)
                            break;
                    }
                    if (iter >= 25)
                        SpiceSharpWarning.Warning(this,
                            Properties.Resources.Diodes_ImpossibleFwdRevMatch.FormatString(Name, xbv, xcbv));
                }
                TempBreakdownVoltage = xbv;
            }
        }
    }
}
