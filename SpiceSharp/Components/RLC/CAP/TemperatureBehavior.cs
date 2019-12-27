using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Capacitor" />.
    /// </summary>
    public class TemperatureBehavior : Behavior, ITemperatureBehavior,
        IParameterized<BaseParameters>
    {
        private readonly ModelBaseParameters _mbp = null;
        private readonly ITemperatureSimulationState _temperature;

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public BaseParameters BaseParameters { get; }

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        BaseParameters IParameterized<BaseParameters>.Parameters => BaseParameters;

        /// <summary>
        /// Gets the capacitance.
        /// </summary>
        [ParameterName("capacitance"), ParameterInfo("The capacitance of the capacitor.")]
        public double Capacitance { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        protected IBiasingSimulationState BiasingState { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TemperatureBehavior(string name, ComponentBindingContext context) : base(name) 
        {
            context.ThrowIfNull(nameof(context));

            // Get parameters
            BaseParameters = context.GetParameterSet<BaseParameters>();
            if (context.ModelBehaviors != null)
                _mbp = context.ModelBehaviors.GetParameterSet<ModelBaseParameters>();

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
                if (_mbp == null)
                    throw new ModelNotFoundException(Name);

                var width = BaseParameters.Width.Given
                    ? BaseParameters.Width.Value
                    : _mbp.DefaultWidth.Value;
                capacitance = _mbp.JunctionCap *
                              (width - _mbp.Narrow) *
                              (BaseParameters.Length - _mbp.Narrow) +
                              _mbp.JunctionCapSidewall * 2 * (
                                  BaseParameters.Length - _mbp.Narrow +
                                  (width - _mbp.Narrow));
            }
            else
                capacitance = BaseParameters.Capacitance;

            double factor = 1.0;

            if (_mbp != null)
            {
                double temperatureDiff = BaseParameters.Temperature - _mbp.NominalTemperature;
                factor = 1.0 + _mbp.TemperatureCoefficient1 * temperatureDiff + _mbp.TemperatureCoefficient2 * temperatureDiff * temperatureDiff;
            }

            Capacitance = factor * capacitance * BaseParameters.ParallelMultiplier;
        }
    }
}
