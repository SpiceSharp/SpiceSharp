namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Class for points in history
    /// </summary>
    public class HistoryPoint
    {
        /// <summary>
        /// The values
        /// </summary>
        public double[] Values;

        /// <summary>
        /// The next point in history
        /// </summary>
        public HistoryPoint Next;

        /// <summary>
        /// The previous point in history
        /// </summary>
        public HistoryPoint Previous;
    }
}
