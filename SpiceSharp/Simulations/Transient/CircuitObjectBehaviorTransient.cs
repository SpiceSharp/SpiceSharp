namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Transient behavior
    /// </summary>
    public abstract class CircuitObjectBehaviorTransient : CircuitObjectBehavior
    {
        /// <summary>
        /// Transient calculations
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public abstract void Transient(Circuit ckt);
    }
}
