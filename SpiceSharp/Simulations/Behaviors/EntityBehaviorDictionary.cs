namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A dictionary of <see cref="Behavior" />. Only on instance of each type is allowed.
    /// </summary>
    /// <seealso cref="TypeDictionary{Behavior}" />
    public class EntityBehaviorDictionary : TypeDictionary<IBehavior>
    {
        /// <summary>
        /// Gets the source identifier.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityBehaviorDictionary"/> class.
        /// </summary>
        /// <param name="source">The entity identifier that will provide the behaviors.</param>
        public EntityBehaviorDictionary(string source)
        {
            Source = source;
        }
    }
}
