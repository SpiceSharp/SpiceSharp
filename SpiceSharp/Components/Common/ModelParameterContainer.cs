using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.CommonBehaviors
{
    /// <summary>
    /// A dummy behavior that can be used to store parameters.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.Behavior" />
    /// <seealso cref="SpiceSharp.Behaviors.ITemperatureBehavior" />
    public class ModelParameterContainer : Behavior, ITemperatureBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelParameterContainer"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public ModelParameterContainer(string name) : base(name)
        {
        }

        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        public void Temperature()
        {
        }
    }
}
