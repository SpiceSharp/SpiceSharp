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
            pins.ThrowIfNull(nameof(pins));
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
            provider.ThrowIfNull(nameof(provider));

            // Get parameters
            BaseParameters = provider.GetParameterSet<BaseParameters>();

            ModelBaseParameters modelParameters;
            if (provider.TryGetParameterSet<ModelBaseParameters>("model", out modelParameters))
            {
                ModelParameters = modelParameters;
            }
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public void Temperature(BaseSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            if (!BaseParameters.Temperature.Given)
                BaseParameters.Temperature.RawValue = simulation.RealState.Temperature;

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
