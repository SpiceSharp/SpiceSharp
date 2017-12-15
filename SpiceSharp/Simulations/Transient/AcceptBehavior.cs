namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Behavior for accepting a timepoint
    /// </summary>
    public abstract class AcceptBehavior : Behavior
    {
        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public abstract void Accept(Circuit ckt);
    }
}
