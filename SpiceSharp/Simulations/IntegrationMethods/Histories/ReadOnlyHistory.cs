namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Wrapper to make a history read-only
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReadOnlyHistory<T>
    {
        /// <summary>
        /// Gets the base history
        /// </summary>
        protected History<T> History { get; }

        /// <summary>
        /// Gets the current value
        /// </summary>
        public T Current
        {
            get => History.Current;
        }

        /// <summary>
        /// Gets a value in history
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public T this[int index]
        {
            get => History[index];
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="history">Base history</param>
        public ReadOnlyHistory(History<T> history)
        {
            History = history;
        }
    }
}
