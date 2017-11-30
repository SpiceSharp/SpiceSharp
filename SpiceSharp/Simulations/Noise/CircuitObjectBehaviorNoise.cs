namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Behavior for noise analysis
    /// </summary>
    public abstract class CircuitObjectBehaviorNoise: CircuitObjectBehavior
    {
        /// <summary>
        /// Noise calculations
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public abstract void Noise(Circuit ckt);
    }
}
