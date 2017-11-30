namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// General behaviour for a circuit object
    /// </summary>
    public abstract class CircuitObjectBehaviorLoad : CircuitObjectBehavior
    {
        /// <summary>
        /// Test convergence
        /// </summary>
        /// <returns></returns>
        public virtual bool IsConvergent(Circuit ckt)
        {
            return true;
        }
    }
}
