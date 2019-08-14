namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// A template for a state that needs some kind of history.
    /// </summary>
    public abstract class StateHistory
    {
        /// <summary>
        /// Gets or sets the value of the state at the current timepoint.
        /// </summary>
        public abstract double Current { get; set; }

        /// <summary>
        /// Gets a point in history.
        /// </summary>
        /// <param name="index">Steps to go back in history. 0 means the current value.</param>
        /// <returns>
        /// The value at the specified timepoint.
        /// </returns>
        public abstract double this[int index] { get; }
    }
}
