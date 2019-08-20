namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// This interface describes behavior for initial conditions.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.IBehavior" />
    public interface IInitialConditionBehavior : IBehavior
    {
        /// <summary>
        /// Sets the initial conditions for the behavior.
        /// </summary>
        void SetInitialCondition();
    }
}
