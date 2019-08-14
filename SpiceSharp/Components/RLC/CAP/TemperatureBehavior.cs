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
        /// The positive node.
        /// </summary>
        protected int PosNode { get; private set; }

        /// <summary>
        /// The negative node.
        /// </summary>
        protected int NegNode { get; private set; }

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
        /// <param name="context">Data provider</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            // Get parameters
            BaseParameters = context.GetParameterSet<BaseParameters>();

            ModelBaseParameters modelParameters;
            if (context.TryGetParameterSet("model", out modelParameters))
                ModelParameters = modelParameters;

            if (context is ComponentBindingContext cc)
            {
                PosNode = cc.Pins[0];
                NegNode = cc.Pins[1];
            }

            State = ((BaseSimulation)simulation).RealState;
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            State = null;
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            if (!BaseParameters.Temperature.Given)
                BaseParameters.Temperature.RawValue = State.Temperature;

            double capacitance;
            if (!BaseParameters.Capacitance.Given)
            {
                if (ModelParameters == null)
                    throw new CircuitException("No model specified");

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

            Capacitance = factor * capacitance;
        }
    }
}
