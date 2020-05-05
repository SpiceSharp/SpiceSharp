using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.Common
{
    /// <summary>
    /// A behavior with the sole purpose of providing parameters.
    /// </summary>
    /// <typeparam name="P">The parameter set type.</typeparam>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IParameterized{P}" />
    public class ParameterBehavior<P> : Behavior,
        IParameterized<P> where P : IParameterSet
    {
        /// <inheritdoc/>
        public P Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterBehavior{P}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public ParameterBehavior(string name, BindingContext context)
            : base(name)
        {
            Parameters = context.GetParameterSet<P>();
        }
    }
}
