using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Bipolars
{
    /// <summary>
    /// Temperature behavior for a <see cref="BipolarJunctionTransistor" />.
    /// </summary>
    /// <seealso cref="Behavior" />
    /// <seealso cref="ITemperatureBehavior" />
    /// <seealso cref="IParameterized{P}" />
    /// <seealso cref="Bipolars.Parameters"/>
    [BehaviorFor(typeof(BipolarJunctionTransistor)), AddBehaviorIfNo(typeof(ITemperatureBehavior))]
    public class Temperature : Behavior,
        ITemperatureBehavior,
        IParameterized<Parameters>
    {
        private readonly ITemperatureSimulationState _temperature;

        /// <inheritdoc/>
        public Parameters Parameters { get; }

        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        /// <value>
        /// The model parameters.
        /// </value>
        protected ModelParameters ModelParameters { get; }

        /// <summary>
        /// Gets the model temperature behavior.
        /// </summary>
        /// <value>
        /// The model temperature behavior.
        /// </value>
        protected ModelTemperature ModelTemperature { get; }

        /// <summary>
        /// Gets the temperature-modified saturation current.
        /// </summary>
        /// <value>
        /// The temperature-modified saturation current.
        /// </value>
        protected double TempSaturationCurrent { get; private set; }

        /// <summary>
        /// Gets the temperature-modified forward beta.
        /// </summary>
        /// <value>
        /// The temperature-modified forward beta.
        /// </value>
        protected double TempBetaForward { get; private set; }

        /// <summary>
        /// Gets the temperature-modified reverse beta.
        /// </summary>
        /// <value>
        /// The temperature-modified reverse beta.
        /// </value>
        protected double TempBetaReverse { get; private set; }

        /// <summary>
        /// Gets the temperature-modified base-emitter saturation current.
        /// </summary>
        /// <value>
        /// the temperature-modified base-emitter saturation current.
        /// </value>
        protected double TempBeLeakageCurrent { get; private set; }

        /// <summary>
        /// Gets the temperature-modified base-collector saturation current.
        /// </summary>
        /// <value>
        /// The temperature-modified base-collector saturation current.
        /// </value>
        protected double TempBcLeakageCurrent { get; private set; }

        /// <summary>
        /// Gets the temperature-modified base-emitter capacitance.
        /// </summary>
        /// <value>
        /// The temperature-modified base-emitter capacitance.
        /// </value>
        protected double TempBeCap { get; private set; }

        /// <summary>
        /// Gets the temperature-modified base-emitter built-in potential.
        /// </summary>
        /// <value>
        /// The temperature-modified base-emitter built-in potential.
        /// </value>
        protected double TempBePotential { get; private set; }

        /// <summary>
        /// Gets the temperature-modified base-collector capacitance.
        /// </summary>
        /// <value>
        /// The temperature-modified base-collector capacitance.
        /// </value>
        protected double TempBcCap { get; private set; }

        /// <summary>
        /// Gets the temperature-modified base-collector built-in potential.
        /// </summary>
        /// <value>
        /// The temperature-modified base-collector built-in potential.
        /// </value>
        protected double TempBcPotential { get; private set; }

        /// <summary>
        /// Gets the temperature-modified depletion capacitance.
        /// </summary>
        /// <value>
        /// The temperature-modified depletion capacitance.
        /// </value>
        protected double TempDepletionCap { get; private set; }

        /// <summary>
        /// Gets the temperature-modified implementation-specific factor 1.
        /// </summary>
        /// <value>
        /// The temperature-modified implementation-specific factor 1.
        /// </value>
        protected double TempFactor1 { get; private set; }

        /// <summary>
        /// Gets the temperature-modified implementation-specific factor 4.
        /// </summary>
        /// <value>
        /// The temperature-modified implementation-specific factor 4.
        /// </value>
        protected double TempFactor4 { get; private set; }

        /// <summary>
        /// Gets the temperature-modified implementation-specific factor 5.
        /// </summary>
        /// <value>
        /// The temperature-modified implementation-specific factor 5.
        /// </value>
        protected double TempFactor5 { get; private set; }

        /// <summary>
        /// Gets the temperature-modified critical voltage.
        /// </summary>
        /// <value>
        /// The temperature-modified critical voltage.
        /// </value>
        protected double TempVCritical { get; private set; }

        /// <summary>
        /// Gets the thermal voltage.
        /// </summary>
        /// <value>
        /// The thermal voltage.
        /// </value>
        protected double Vt { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        protected IBiasingSimulationState BiasingState { get; private set; }

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
            Parameters = context.GetParameterSet<Parameters>();
            BiasingState = context.GetState<IBiasingSimulationState>();
        }

        /// <inheritdoc/>
        void ITemperatureBehavior.Temperature()
        {
            if (!Parameters.Temperature.Given)
                Parameters.Temperature = new GivenParameter<double>(_temperature.Temperature, false);
            Vt = Parameters.Temperature * Constants.KOverQ;
            double fact2 = Parameters.Temperature / Constants.ReferenceTemperature;
            double egfet = 1.16 - 7.02e-4 * Parameters.Temperature * Parameters.Temperature / (Parameters.Temperature + 1108);
            double arg = -egfet / (2 * Constants.Boltzmann * Parameters.Temperature) + 1.1150877 / (Constants.Boltzmann * (Constants.ReferenceTemperature +
                                                                                                                Constants.ReferenceTemperature));
            double pbfact = -2 * Vt * (1.5 * Math.Log(fact2) + Constants.Charge * arg);

            double ratlog = Math.Log(Parameters.Temperature / ModelParameters.NominalTemperature);
            double ratio1 = Parameters.Temperature / ModelParameters.NominalTemperature - 1;
            double factlog = ratio1 * ModelParameters.EnergyGap / Vt + ModelParameters.TempExpIs * ratlog;
            double factor = Math.Exp(factlog);
            TempSaturationCurrent = ModelParameters.SatCur * factor;
            double bfactor = Math.Exp(ratlog * ModelParameters.BetaExponent);
            TempBetaForward = ModelParameters.BetaF * bfactor;
            TempBetaReverse = ModelParameters.BetaR * bfactor;
            TempBeLeakageCurrent = ModelParameters.LeakBeCurrent * Math.Exp(factlog / ModelParameters.LeakBeEmissionCoefficient) / bfactor;
            TempBcLeakageCurrent = ModelParameters.LeakBcCurrent * Math.Exp(factlog / ModelParameters.LeakBcEmissionCoefficient) / bfactor;

            double pbo = (ModelParameters.PotentialBe - pbfact) / ModelTemperature.Factor1;
            double gmaold = (ModelParameters.PotentialBe - pbo) / pbo;
            TempBeCap = ModelParameters.DepletionCapBe / (1 + ModelParameters.JunctionExpBe * (4e-4 * (ModelParameters.NominalTemperature - Constants.ReferenceTemperature) - gmaold));
            TempBePotential = fact2 * pbo + pbfact;
            double gmanew = (TempBePotential - pbo) / pbo;
            TempBeCap *= 1 + ModelParameters.JunctionExpBe * (4e-4 * (Parameters.Temperature - Constants.ReferenceTemperature) - gmanew);

            pbo = (ModelParameters.PotentialBc - pbfact) / ModelTemperature.Factor1;
            gmaold = (ModelParameters.PotentialBc - pbo) / pbo;
            TempBcCap = ModelParameters.DepletionCapBc / (1 + ModelParameters.JunctionExpBc * (4e-4 * (ModelParameters.NominalTemperature - Constants.ReferenceTemperature) - gmaold));
            TempBcPotential = fact2 * pbo + pbfact;
            gmanew = (TempBcPotential - pbo) / pbo;
            TempBcCap *= 1 + ModelParameters.JunctionExpBc * (4e-4 * (Parameters.Temperature - Constants.ReferenceTemperature) - gmanew);

            TempDepletionCap = ModelParameters.DepletionCapCoefficient * TempBePotential;
            TempFactor1 = TempBePotential * (1 - Math.Exp((1 - ModelParameters.JunctionExpBe) * ModelTemperature.Xfc)) / (1 - ModelParameters.JunctionExpBe);
            TempFactor4 = ModelParameters.DepletionCapCoefficient * TempBcPotential;
            TempFactor5 = TempBcPotential * (1 - Math.Exp((1 - ModelParameters.JunctionExpBc) * ModelTemperature.Xfc)) / (1 - ModelParameters.JunctionExpBc);
            TempVCritical = Vt * Math.Log(Vt / (Constants.Root2 * ModelParameters.SatCur));
        }
    }
}
