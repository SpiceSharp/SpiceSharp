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
        public BaseParameters Parameters { get; }

        /// <summary>
        /// Gets the capacitance.
        /// </summary>
        [ParameterName("capacitance"), ParameterInfo("The capacitance of the capacitor.")]
        public double Capacitance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TemperatureBehavior(string name, IComponentBindingContext context) : base(name) 
        {
            context.ThrowIfNull(nameof(context));

            // Get parameters
            Parameters = context.GetParameterSet<BaseParameters>();
            if (context.ModelBehaviors != null)
                _mbp = context.ModelBehaviors.GetParameterSet<ModelBaseParameters>();

            // Connections
            _temperature = context.GetState<ITemperatureSimulationState>();
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            if (!Parameters.Temperature.Given)
                Parameters.Temperature = new GivenParameter<double>(_temperature.Temperature, false);

            double capacitance;
            if (!Parameters.Capacitance.Given)
            {
                if (_mbp == null)
                    throw new ModelNotFoundException(Name);

                var width = Parameters.Width.Given
                    ? Parameters.Width.Value
                    : _mbp.DefaultWidth;
                capacitance = _mbp.JunctionCap *
                              (width - _mbp.Narrow) *
                              (Parameters.Length - _mbp.Narrow) +
                              _mbp.JunctionCapSidewall * 2 * (
                                  Parameters.Length - _mbp.Narrow +
                                  (width - _mbp.Narrow));
            }
            else
                capacitance = Parameters.Capacitance;

            double factor = 1.0;

            if (_mbp != null)
            {
                double temperatureDiff = Parameters.Temperature - _mbp.NominalTemperature;
                factor = 1.0 + _mbp.TemperatureCoefficient1 * temperatureDiff + _mbp.TemperatureCoefficient2 * temperatureDiff * temperatureDiff;
            }

            Capacitance = factor * capacitance * Parameters.ParallelMultiplier;
        }
    }
}
