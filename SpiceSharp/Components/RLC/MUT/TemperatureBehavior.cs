using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Behaviors;

namespace SpiceSharp.Components.MutualInductanceBehaviors
{
    /// <summary>
    /// Temperature-dependent calculations for a <see cref="MutualInductance"/>.
    /// </summary>
    public class TemperatureBehavior : ExportingBehavior, ITemperatureBehavior
    {
        /// <summary>
        /// Gets the coupling factor.
        /// </summary>
        protected double Factor { get; private set; }

        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the base parameters of inductor 1.
        /// </summary>
        protected InductorBehaviors.BaseParameters BaseParameters1 { get; private set; }

        /// <summary>
        /// Gets the base parameters of inductor 2.
        /// </summary>
        protected InductorBehaviors.BaseParameters BaseParameters2 { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        public TemperatureBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Setup the behavior for the specified simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The provider.</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            provider.ThrowIfNull(nameof(provider));

            // Get parameters
            BaseParameters = provider.GetParameterSet<BaseParameters>();
            BaseParameters1 = provider.GetParameterSet<InductorBehaviors.BaseParameters>("inductor1");
            BaseParameters2 = provider.GetParameterSet<InductorBehaviors.BaseParameters>("inductor2");
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        /// <param name="simulation">The base simulation.</param>
        public void Temperature(BaseSimulation simulation)
        {
            // Calculate coupling factor
            Factor = BaseParameters.Coupling * Math.Sqrt(BaseParameters1.Inductance * BaseParameters2.Inductance);
        }
    }
}
