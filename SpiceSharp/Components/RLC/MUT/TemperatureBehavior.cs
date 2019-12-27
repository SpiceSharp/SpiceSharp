using System;
using SpiceSharp.Behaviors;
using IInductanceBehavior = SpiceSharp.Components.InductorBehaviors.IInductanceBehavior;

namespace SpiceSharp.Components.MutualInductanceBehaviors
{
    /// <summary>
    /// Temperature-dependent calculations for a <see cref="MutualInductance"/>.
    /// </summary>
    public class TemperatureBehavior : Behavior, ITemperatureBehavior,
        IParameterized<BaseParameters>
    {
        /// <summary>
        /// Gets the coupling factor.
        /// </summary>
        protected double Factor { get; private set; }

        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        public BaseParameters Parameters { get;  }

        /// <summary>
        /// Gets the temperature behavior of inductor 1.
        /// </summary>
        protected IInductanceBehavior InductanceBehavior1 { get; private set; }

        /// <summary>
        /// Gets the temperature behavior of inductor 2.
        /// </summary>
        protected IInductanceBehavior InductanceBehavior2 { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TemperatureBehavior(string name, MutualInductanceBindingContext context) : base(name)
        {
            context.ThrowIfNull(nameof(context));

            Parameters = context.GetParameterSet<BaseParameters>();
            InductanceBehavior1 = context.Inductor1Behaviors.GetValue<IInductanceBehavior>();
            InductanceBehavior2 = context.Inductor2Behaviors.GetValue<IInductanceBehavior>();
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            // Calculate coupling factor
            Factor = Parameters.Coupling * Math.Sqrt(InductanceBehavior1.Inductance * InductanceBehavior2.Inductance);
        }
    }
}
