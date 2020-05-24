using System;
using SpiceSharp.ParameterSets;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Mosfets.Level2
{
    /// <summary>
    /// Temperature behavior for a <see cref="Mosfet2"/>.
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
        /// The common temperature-dependent properties.
        /// </summary>
        protected readonly TemperatureProperties Properties = new TemperatureProperties();

        /// <summary>
        /// The model parameters.
        /// </summary>
        protected readonly ModelParameters ModelParameters;

        /// <summary>
        /// The model temperature behavior.
        /// </summary>
        protected readonly ModelTemperature ModelTemperature;

        /// <include file='../common/docs.xml' path='docs/members/SourceConductance/*'/>
        [ParameterName("sourceconductance"), ParameterInfo("Conductance at the source")]
        public double SourceConductance => Properties.SourceConductance.Equals(0.0) ? double.PositiveInfinity : Properties.SourceConductance;

        /// <include file='../common/docs.xml' path='docs/members/DrainConductance/*'/>
        [ParameterName("drainconductance"), ParameterInfo("Conductance at the drain")]
        public double DrainConductance => Properties.DrainConductance.Equals(0.0) ? double.PositiveInfinity : Properties.DrainConductance;

        /// <include file='../common/docs.xml' path='docs/members/SourceResistance/*'/>
        [ParameterName("rs"), ParameterInfo("Source resistance")]

        public double SourceResistance => Properties.SourceConductance.Equals(0.0) ? 0 : 1.0 / Properties.SourceConductance;

        /// <include file='../common/docs.xml' path='docs/members/DrainResistance/*'/>
        [ParameterName("rd"), ParameterInfo("Drain conductance")]
        public double DrainResistance => Properties.DrainConductance.Equals(0.0) ? 0 : 1.0 / Properties.DrainConductance;

        /// <include file='../common/docs.xml' path='docs/members/CriticalSourceVoltage/*'/>
        [ParameterName("sourcevcrit"), ParameterInfo("Critical source voltage")]
        public double SourceVCritical => Properties.SourceVCritical;

        /// <include file='../common/docs.xml' path='docs/members/CriticalDrainVoltage/*'/>
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
            ModelTemperature = context.ModelBehaviors.GetValue<ModelTemperature>();
            ModelParameters = ModelTemperature.GetParameterSet<ModelParameters>();
            Parameters = context.GetParameterSet<Parameters>();
        }

        void ITemperatureBehavior.Temperature()
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
