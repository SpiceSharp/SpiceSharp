using System;
using System.Reflection;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A dictionary of <see cref="Behavior" />. Only on instance of each type is allowed.
    /// </summary>
    /// <seealso cref="TypeDictionary{Behavior}" />
    public class EntityBehaviorDictionary : TypeDictionary<Behavior>
    {
        // TODO: Take another look at this class... I'm not sure why the constructor uses Func<Behavior>.

        /// <summary>
        /// Gets the source identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public Identifier Source { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBehaviorDictionary"/> class.
        /// </summary>
        /// <param name="source">The entity identifier that will provide the behaviors.</param>
        public EntityBehaviorDictionary(Identifier source)
            : base(typeof(Func<Behavior>))
        {
            Source = source;
        }

        /// <summary>
        /// Register a behavior.
        /// </summary>
        /// <param name="behavior">Behavior</param>
        /// <exception cref="ArgumentNullException">behavior</exception>
        /// <exception cref="SpiceSharp.CircuitException">Invalid behavior</exception>
        public void Register(Behavior behavior)
        {
            if (behavior == null)
                throw new ArgumentNullException(nameof(behavior));

            // Get types
            var mytype = behavior.GetType();
            var basetype = mytype.GetTypeInfo().BaseType ?? throw new CircuitException("Invalid behavior");

            // Register types
            Dictionary[mytype] = behavior;
            Dictionary[basetype] = behavior;
        }
    }
}
