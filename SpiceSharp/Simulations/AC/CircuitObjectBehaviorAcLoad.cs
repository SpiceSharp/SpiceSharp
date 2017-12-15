namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// AC behaviour for circuit objects
    /// </summary>
    public abstract class CircuitObjectBehaviorAcLoad : Behavior
    {
        /// <summary>
        /// Load the Y-matrix and Rhs-vector for AC analysis
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public abstract void Load(Circuit ckt);
    }
}
