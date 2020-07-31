using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;
using System;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A base template for an <see cref="IBehavior"/>.
    /// </summary>
    /// <remarks>
    /// This class also allows the behavior to define its own named parameters and properties,
    /// as well as link to existing <see cref="IParameterSet"/> classes.
    /// </remarks>
    /// <seealso cref="ParameterSetCollection"/>
    /// <seealso cref="IBehavior"/>
    public abstract class Behavior : ParameterSetCollection,
        IBehavior
    {
        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Behavior"/> class.
        /// </summary>
        /// <param name="name">The name of the behavior.</param>
        /// <remarks>
        /// The name of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        protected Behavior(string name)
        {
            Name = name.ThrowIfNull(nameof(name));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Behavior"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        protected Behavior(IBindingContext context)
        {
            Name = context.ThrowIfNull(nameof(context)).Behaviors.Name;
        }
    }
}
