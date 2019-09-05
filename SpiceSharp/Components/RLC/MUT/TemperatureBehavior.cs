using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.MutualInductanceBehaviors
{
    /// <summary>
    /// Temperature-dependent calculations for a <see cref="MutualInductance"/>.
    /// </summary>
    public class TemperatureBehavior : Behavior, ITemperatureBehavior
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
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);
            var c = (MutualInductanceBindingContext)context;
            BaseParameters = context.Behaviors.Parameters.Get<BaseParameters>();
            BaseParameters1 = c.Inductor1Behaviors.Parameters.Get<InductorBehaviors.BaseParameters>();
            BaseParameters2 = c.Inductor2Behaviors.Parameters.Get<InductorBehaviors.BaseParameters>();
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            // Calculate coupling factor
            Factor = BaseParameters.Coupling * Math.Sqrt(BaseParameters1.Inductance * BaseParameters2.Inductance);
        }
    }
}
