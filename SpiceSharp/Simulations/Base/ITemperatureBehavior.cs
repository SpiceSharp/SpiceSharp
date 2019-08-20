namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// An interface that describes temperature-dependent behaviors.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.IBehavior" />
    public interface ITemperatureBehavior : IBehavior
    {
        /// <summary>
        /// Perform temperature-dependent calculations.
        /// </summary>
        void Temperature();
    }
}
