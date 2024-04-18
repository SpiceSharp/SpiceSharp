using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.JFETs
{
    /// <summary>
    /// Temperature behavior for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="Behavior"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="JFETs.Parameters"/>
    /// <seealso cref="ITemperatureBehavior"/>
    [BehaviorFor(typeof(JFET)), AddBehaviorIfNo(typeof(ITemperatureBehavior))]
    public class Temperature : Behavior,
        IParameterized<Parameters>,
        ITemperatureBehavior
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
        protected ModelParameters ModelParameters { get; private set; }

        /// <summary>
        /// Gets the model temperature behavior.
        /// </summary>
        protected ModelTemperature ModelTemperature { get; private set; }

        /// <summary>
        /// Gets the temperature-modified saturation current.
        /// </summary>
        /// <value>
        /// The temperature-modified saturation current.
        /// </value>
        public double TempSaturationCurrent { get; private set; }

        /// <summary>
        /// Gets the temperature-modified gate-source capacitance.
        /// </summary>
        /// <value>
        /// The temperature-modified gate-source capacitance.
        /// </value>
        public double TempCapGs { get; private set; }

        /// <summary>
        /// Gets the temperature-modified gate-drain capacitance.
        /// </summary>
        /// <value>
        /// The temperature-modified gate-drain capacitance.
        /// </value>
        public double TempCapGd { get; private set; }

        /// <summary>
        /// Gets the temperature-modified gate potential.
        /// </summary>
        /// <value>
        /// The temperature-modified gate potential.
        /// </value>
        public double TempGatePotential { get; private set; }

        /// <summary>
        /// Gets the temperature-modified depletion capacitance correction.
        /// </summary>
        /// <value>
        /// The temperature-modified depletion capacitance correction.
        /// </value>
        public double CorDepCap { get; private set; }

        /// <summary>
        /// Gets the implementation-specific factor 1.
        /// </summary>
        /// <value>
        /// The f1.
        /// </value>
        protected double F1 { get; private set; }

        /// <summary>
        /// Gets the temperature-modified critical voltage.
        /// </summary>
        /// <value>
        /// The temperature-modified critical voltage.
        /// </value>
        public double Vcrit { get; private set; }

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
            ModelTemperature = context.ModelBehaviors.GetValue<ModelTemperature>();
            Parameters = context.GetParameterSet<Parameters>();
            ModelParameters = context.ModelBehaviors.GetParameterSet<ModelParameters>();
            BiasingState = context.GetState<IBiasingSimulationState>();
        }

        /// <inheritdoc/>
        void ITemperatureBehavior.Temperature()
        {
            if (!Parameters.Temperature.Given)
                Parameters.Temperature = new GivenParameter<double>(_temperature.Temperature, false);
            double vt = Parameters.Temperature * Constants.KOverQ;
            double fact2 = Parameters.Temperature / Constants.ReferenceTemperature;
            double ratio1 = Parameters.Temperature / ModelParameters.NominalTemperature - 1;
            TempSaturationCurrent = ModelParameters.GateSaturationCurrent * Math.Exp(ratio1 * 1.11 / vt);
            TempCapGs = ModelParameters.CapGs * ModelTemperature.Cjfact;
            TempCapGd = ModelParameters.CapGd * ModelTemperature.Cjfact;
            double kt = Constants.Boltzmann * Parameters.Temperature;
            double egfet = 1.16 - (7.02e-4 * Parameters.Temperature * Parameters.Temperature) / (Parameters.Temperature + 1108);
            double arg = -egfet / (kt + kt) + 1.1150877 / (Constants.Boltzmann * 2 * Constants.ReferenceTemperature);
            double pbfact = -2 * vt * (1.5 * Math.Log(fact2) + Constants.Charge * arg);
            TempGatePotential = fact2 * ModelTemperature.Pbo + pbfact;
            double gmanew = (TempGatePotential - ModelTemperature.Pbo) / ModelTemperature.Pbo;
            double cjfact1 = 1 + .5 * (4e-4 * (Parameters.Temperature - Constants.ReferenceTemperature) - gmanew);
            TempCapGs *= cjfact1;
            TempCapGd *= cjfact1;

            CorDepCap = ModelParameters.DepletionCapCoefficient * TempGatePotential;
            F1 = TempGatePotential * (1 - Math.Exp((1 - .5) * ModelTemperature.Xfc)) / (1 - .5);
            Vcrit = vt * Math.Log(vt / (Constants.Root2 * TempSaturationCurrent));

            if (TempGatePotential.Equals(0.0))
                throw new SpiceSharpException("{0}: {1}".FormatString(nameof(TempGatePotential), Properties.Resources.Parameters_IsZero));
        }
    }
}
