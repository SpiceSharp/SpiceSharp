using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.DiodeBehaviors
{
    /// <summary>
    /// Temperature-dependent behavior for a diode.
    /// </summary>
    public class DiodeTemperature : Behavior, ITemperatureBehavior
    {
        private readonly ITemperatureSimulationState _temperatureState;

        /// <summary>
        /// Gets the diode parameters.
        /// </summary>
        protected DiodeParameters Parameters { get; }

        /// <summary>
        /// Gets the denominator in the exponent.
        /// </summary>
        protected double Vte { get; private set; }

        /// <summary>
        /// Creates a new temperature-dependent behavior
        /// </summary>
        /// <param name="context"></param>
        public DiodeTemperature(IBindingContext context)
            : base(context)
        {
            Parameters = context.GetParameterSet<DiodeParameters>();
            _temperatureState = context.GetState<ITemperatureSimulationState>();
        }

        /// <summary>
        /// Calculates temperature-dependent properties.
        /// </summary>
        public void Temperature()
        {
            Vte = Parameters.Eta * Constants.KOverQ * _temperatureState.Temperature;
        }
    }
}
