using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Inductor"/>.
    /// </summary>
    public class TemperatureBehavior : Behavior, ITemperatureBehavior
    {
        /// <summary>
        /// Gets the inductance of the inductor.
        /// </summary>
        [ParameterName("l"), ParameterName("inductance"), ParameterInfo("The inductance")]
        public double Inductance { get; private set; }

        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TemperatureBehavior(string name, ComponentBindingContext context)
            : base(name)
        {
            context.ThrowIfNull(nameof(context));
            BaseParameters = context.Behaviors.Parameters.GetValue<BaseParameters>();
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        void ITemperatureBehavior.Temperature()
        {
            Inductance = BaseParameters.Inductance / BaseParameters.ParallelMultiplier;
        }
    }
}
