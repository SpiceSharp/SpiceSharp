namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Represents a variable with memory
    /// </summary>
    public abstract class StateHistory
    {
        /// <summary>
        /// Gets or sets the value of the state at the current timepoint
        /// </summary>
        public abstract double Current { get; set; }

        /// <summary>
        /// Gets a point in history
        /// </summary>
        /// <param name="index">Steps to go back in history</param>
        /// <returns></returns>
        public abstract double this[int index] { get; }
    }
}
