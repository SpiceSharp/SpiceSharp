using SpiceSharp.Sparse;

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
        public Vector<double> Values { get; set; }

        /// <summary>
        /// The next point in history
        /// </summary>
        public HistoryPoint Next { get; set; } = null;

        /// <summary>
        /// The previous point in history
        /// </summary>
        public HistoryPoint Previous { get; set; } = null;
    }
}
