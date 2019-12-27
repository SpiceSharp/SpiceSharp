using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Capacitor" />.
    /// </summary>
    public class TemperatureBehavior : Behavior, ITemperatureBehavior
    {
        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        protected ModelBaseParameters ModelParameters { get; private set; }

        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the capacitance.
        /// </summary>
        [ParameterName("capacitance"), ParameterInfo("The capacitance of the capacitor.")]
        public double Capacitance { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        protected IBiasingSimulationState BiasingState { get; private set; }

        private readonly ITemperatureSimulationState _temperature;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TemperatureBehavior(string name, ComponentBindingContext context) : base(name) 
        {
            context.ThrowIfNull(nameof(context));

            // Get parameters
            BaseParameters = context.Behaviors.Parameters.GetValue<BaseParameters>();
            if (context.ModelBehaviors != null)
                ModelParameters = context.ModelBehaviors.Parameters.GetValue<ModelBaseParameters>();

            // Connections
            _temperature = context.GetState<ITemperatureSimulationState>();
            BiasingState = context.GetState<IBiasingSimulationState>();
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            if (!BaseParameters.Temperature.Given)
                BaseParameters.Temperature.RawValue = _temperature.Temperature;

            double capacitance;
            if (!BaseParameters.Capacitance.Given)
            {
                if (ModelParameters == null)
                    throw new ModelNotFoundException(Name);

                var width = BaseParameters.Width.Given
                    ? BaseParameters.Width.Value
                    : ModelParameters.DefaultWidth.Value;
                capacitance = ModelParameters.JunctionCap *
                              (width - ModelParameters.Narrow) *
                              (BaseParameters.Length - ModelParameters.Narrow) +
                              ModelParameters.JunctionCapSidewall * 2 * (
                                  BaseParameters.Length - ModelParameters.Narrow +
                                  (width - ModelParameters.Narrow));
            }
            else
                capacitance = BaseParameters.Capacitance;

            double factor = 1.0;

            if (ModelParameters != null)
            {
                double temperatureDiff = BaseParameters.Temperature - ModelParameters.NominalTemperature;
                factor = 1.0 + ModelParameters.TemperatureCoefficient1 * temperatureDiff + ModelParameters.TemperatureCoefficient2 * temperatureDiff * temperatureDiff;
            }

            Capacitance = factor * capacitance * BaseParameters.ParallelMultiplier;
        }
    }
}
