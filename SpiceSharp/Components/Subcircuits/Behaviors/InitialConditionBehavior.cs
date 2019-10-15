using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Initial condition behavior for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.SubcircuitBehaviors.SubcircuitBehavior{T}" />
    /// <seealso cref="SpiceSharp.Behaviors.IInitialConditionBehavior" />
    public class InitialConditionBehavior : SubcircuitBehavior<IInitialConditionBehavior>, IInitialConditionBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InitialConditionBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public InitialConditionBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Sets the initial conditions for the behavior.
        /// </summary>
        public void SetInitialCondition()
        {
            foreach (var bs in Behaviors)
            {
                for (var i = 0; i < bs.Count; i++)
                    bs[i].SetInitialCondition();
            }
        }
    }
}
