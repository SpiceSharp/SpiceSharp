using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Time behavior for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.SubcircuitBehaviors.SubcircuitBehavior{T}" />
    /// <seealso cref="SpiceSharp.Behaviors.ITimeBehavior" />
    public class TimeBehavior : SubcircuitBehavior<ITimeBehavior>, ITimeBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public TimeBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Initialize the state values from the current DC solution.
        /// </summary>
        /// <remarks>
        /// In this method, the initial value is calculated based on the operating point solution,
        /// and the result is stored in each respective <see cref="SpiceSharp.IntegrationMethods.StateDerivative" /> or <see cref="SpiceSharp.IntegrationMethods.StateHistory" />.
        /// </remarks>
        public void InitializeStates()
        {
            foreach (var bs in Behaviors)
            {
                for (var i = 0; i < bs.Count; i++)
                    bs[i].InitializeStates();
            }
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        public void Load()
        {
            foreach (var bs in Behaviors)
            {
                for (var i = 0; i < bs.Count; i++)
                    bs[i].Load();
            }
        }
    }
}
