using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Template for a behavior.
    /// </summary>
    public abstract class Behavior : IBehavior
    {
        /// <summary>
        /// Gets the identifier of the behavior.
        /// </summary>
        /// <remarks>
        /// This should be the same identifier as the entity that created it.
        /// </remarks>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Behavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected Behavior(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public virtual void Bind(BindingContext context)
        {
        }

        /// <summary>
        /// Destroy the behavior.
        /// </summary>
        public virtual void Unbind()
        {
        }
    }
}
