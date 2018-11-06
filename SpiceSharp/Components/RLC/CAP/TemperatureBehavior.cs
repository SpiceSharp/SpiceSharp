using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Behaviors;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Capacitor" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Simulations.Behaviors.ExportingBehavior" />
    /// <seealso cref="SpiceSharp.Behaviors.ITemperatureBehavior" />
    /// <seealso cref="SpiceSharp.Components.IConnectedBehavior" />
    public class TemperatureBehavior : ExportingBehavior, ITemperatureBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Gets the model parameters.
        /// </summary>
        /// <value>
        /// The model parameters.
        /// </value>
        protected ModelBaseParameters ModelParameters { get; private set; }

        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        /// <value>
        /// The base parameters.
        /// </value>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the capacitance.
        /// </summary>
        /// <value>
        /// The capacitance.
        /// </value>
        [ParameterName("capacitance"), ParameterInfo("The capacitance of the capacitor.")]
        public double Capacitance { get; private set; }

        /// <summary>
        /// Nodes for child classes
        /// </summary>
        protected int PosNode { get; private set; }
        protected int NegNode { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(string name) : base(name) { }

        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            PosNode = pins[0];
            NegNode = pins[1];
        }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            base.Setup(simulation, provider);
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            BaseParameters = provider.GetParameterSet<BaseParameters>();
            if (!BaseParameters.Capacitance.Given)
                ModelParameters = provider.GetParameterSet<ModelBaseParameters>("model");
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public void Temperature(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            if (!BaseParameters.Capacitance.Given)
            {
                if (ModelParameters == null)
                    throw new CircuitException("No model specified");

                var width = BaseParameters.Width.Given
                    ? BaseParameters.Width.Value
                    : ModelParameters.DefaultWidth.Value;
                Capacitance = ModelParameters.JunctionCap *
                              (width - ModelParameters.Narrow) *
                              (BaseParameters.Length - ModelParameters.Narrow) +
                              ModelParameters.JunctionCapSidewall * 2 * (
                                  BaseParameters.Length - ModelParameters.Narrow +
                                  (width - ModelParameters.Narrow));
            }
            else
                Capacitance = BaseParameters.Capacitance;
        }
    }
}
