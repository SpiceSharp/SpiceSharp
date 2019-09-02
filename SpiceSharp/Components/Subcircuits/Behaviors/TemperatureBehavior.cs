using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A temperature behavior for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.Behavior" />
    /// <seealso cref="SpiceSharp.Behaviors.ITemperatureBehavior" />
    public class TemperatureBehavior : SubcircuitBehavior<ITemperatureBehavior>, ITemperatureBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemperatureBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public TemperatureBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        public void Temperature()
        {
            for (var i = 0; i < Behaviors.Count; i++)
                Behaviors[i].Temperature();
        }
    }
}
