namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Behavior for truncating the current timestep
    /// </summary>
    public abstract class CircuitObjectBehaviorTruncate : CircuitObjectBehavior
    {
        /// <summary>
        /// Truncate the current timestep
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timestep">Timestep</param>
        public abstract void Truncate(Circuit ckt, ref double timestep);
    }
}
