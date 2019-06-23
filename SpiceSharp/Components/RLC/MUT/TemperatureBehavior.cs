using System;
using System.Collections.Generic;
using System.Text;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Behaviors;

namespace SpiceSharp.Components.MutualInductanceBehaviors
{
    public class TemperatureBehavior : ExportingBehavior, ITemperatureBehavior
    {
        /// <summary>
        /// Gets the coupling factor.
        /// </summary>
        /// <value>
        /// The factor.
        /// </value>
        protected double Factor { get; private set; }

        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        /// <value>
        /// The base parameters.
        /// </value>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the base parameters of inductor 1.
        /// </summary>
        /// <value>
        /// The base parameters.
        /// </value>
        protected InductorBehaviors.BaseParameters BaseParameters1 { get; private set; }

        /// <summary>
        /// Gets the base parameters of inductor 2.
        /// </summary>
        /// <value>
        /// The base parameters.
        /// </value>
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
        /// <exception cref="ArgumentNullException">provider</exception>
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
