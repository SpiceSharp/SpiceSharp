using System;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Event arguments
    /// </summary>
    /// <seealso cref="EventArgs" />
    public class BehaviorsNotFoundEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the name of the entity that has no behaviors.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the behaviors associated with the name.
        /// </summary>
        /// <value>
        /// The behaviors.
        /// </value>
        public IBehaviorContainer Behaviors { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorsNotFoundEventArgs"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public BehaviorsNotFoundEventArgs(string name)
        {
            Name = name.ThrowIfNull(nameof(name));
        }
    }
}
