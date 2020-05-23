using SpiceSharp.ParameterSets;
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
        public Parameters Parameters { get; }

        /// <summary>
        /// Gets the common temperature-dependent properties.
        /// </summary>
        /// <value>
        /// The common temperature-dependent properties.
        /// </value>
        public TemperatureProperties Properties { get; } = new TemperatureProperties();

        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        protected readonly ModelParameters ModelParameters;

        /// <summary>
        /// Gets the model temperature behavior.
        /// </summary>
        protected readonly ModelTemperature ModelTemperature;

        [ParameterName("sourceconductance"), ParameterInfo("Conductance at the source")]
        public double SourceConductance => Properties.SourceConductance.Equals(0.0) ? double.PositiveInfinity : Properties.SourceConductance;
        [ParameterName("drainconductance"), ParameterInfo("Conductance at the drain")]
        public double DrainConductance => Properties.DrainConductance.Equals(0.0) ? double.PositiveInfinity : Properties.DrainConductance;
        [ParameterName("rs"), ParameterInfo("Source resistance")]
        public double SourceResistance => Properties.SourceConductance.Equals(0.0) ? 0 : 1.0 / Properties.SourceConductance;
        [ParameterName("rd"), ParameterInfo("Drain conductance")]
        public double DrainResistance => Properties.DrainConductance.Equals(0.0) ? 0 : 1.0 / Properties.DrainConductance;
        [ParameterName("sourcevcrit"), ParameterInfo("Critical source voltage")]
        public double SourceVCritical => Properties.SourceVCritical;
        [ParameterName("drainvcrit"), ParameterInfo("Critical drain voltage")]
        public double DrainVCritical => Properties.DrainVCritical;

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

        /// <inheritdoc/>
        void ITemperatureBehavior.Temperature() => CalculateTemperature();

        /// <summary>
        /// Do temperature-dependent calculations.
        /// </summary>
        protected virtual void CalculateTemperature()
        {
            // Update the width and length if they are not given and if the model specifies them
            if (!Parameters.Width.Given && ModelParameters.Width.Given)
                Parameters.Width = new GivenParameter<double>(ModelParameters.Width.Value, false);
            if (!Parameters.Length.Given && ModelParameters.Length.Given)
                Parameters.Length = new GivenParameter<double>(ModelParameters.Length.Value, false);
            if (!Parameters.Temperature.Given)
                Parameters.Temperature = new GivenParameter<double>(_temperature.Temperature, false);

            // Update common properties
            Properties.Update(Name, Parameters, ModelParameters, ModelTemperature.Properties);
        }
    }
}
