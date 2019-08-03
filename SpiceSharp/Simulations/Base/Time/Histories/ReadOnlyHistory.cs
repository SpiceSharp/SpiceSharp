namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// A wrapper class to make a history read-only.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    public class ReadOnlyHistory<T>
    {
        /// <summary>
        /// Gets the base history object.
        /// </summary>
        protected History<T> History { get; }

        /// <summary>
        /// Gets the current value.
        /// </summary>
        public T Current => History.Current;

        /// <summary>
        /// Gets a value in history.
        /// </summary>
        /// <param name="index">The number of points to go back in time. 0 means the current point.</param>
        /// <returns>
        /// The value at the specified timepoint.
        /// </returns>
        public T this[int index] => History[index];

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyHistory{T}"/> class.
        /// </summary>
        /// <param name="history">The base history.</param>
        public ReadOnlyHistory(History<T> history)
        {
            History = history.ThrowIfNull(nameof(history));
        }
    }
}
