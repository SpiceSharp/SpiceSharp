using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using System;

namespace SpiceSharp.Components.Common
{
    /// <summary>
    /// A behavior with the sole purpose of providing a parameter set.
    /// </summary>
    /// <typeparam name="P">The parameter set type.</typeparam>
    /// <seealso cref="Behavior" />
    /// <seealso cref="IParameterized{P}" />
    [BehaviorFor(typeof(ResistorModel), typeof(ITemperatureBehavior), new[] { typeof(Resistors.ModelParameters) })]
    [BehaviorFor(typeof(CapacitorModel), typeof(ITemperatureBehavior), new[] { typeof(Capacitors.ModelParameters) })]
    public class ParameterBehavior<P> : Behavior,
        IParameterized<P> where P : IParameterSet
    {
        /// <inheritdoc/>
        public P Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterBehavior{P}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public ParameterBehavior(IBindingContext context)
            : base(context)
        {
            Parameters = context.GetParameterSet<P>();
        }
    }
}
