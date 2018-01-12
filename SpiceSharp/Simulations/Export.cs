using System;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Class for getting an export
    /// </summary>
    public class Export
    {
        /// <summary>
        /// Function for extracting
        /// </summary>
        public Func<double> Function { get; set; }

        /// <summary>
        /// Private parameters
        /// </summary>
        Func<BehaviorPool, Func<double>> extract;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="extract">Extraction method</param>
        public Export(Func<BehaviorPool, Func<double>> extract)
        {
            this.extract = extract;
        }

        /// <summary>
        /// Extract the export method from the behaviorpool
        /// </summary>
        /// <param name="pool">Pool</param>
        public void ExtractExport(BehaviorPool pool)
        {
            Function = extract(pool);
        }
    }
}
