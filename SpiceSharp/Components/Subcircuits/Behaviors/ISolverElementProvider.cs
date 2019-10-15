namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// An interface describing a solver that has local elements for running in parallel.
    /// </summary>
    public interface ISolverElementProvider
    {
        /// <summary>
        /// Resets all local elements.
        /// </summary>
        public void Reset();

        /// <summary>
        /// Applies all changes to the local elements.
        /// </summary>
        public void ApplyElements();
    }
}
