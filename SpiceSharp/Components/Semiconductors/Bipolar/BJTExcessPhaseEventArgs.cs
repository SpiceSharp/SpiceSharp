using System;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Event arguments for modifying charges and currents
    /// </summary>
    public class ExcessPhaseEventArgs : EventArgs
    {
        /// <summary>
        /// Collector current
        /// </summary>
        public double cc { get; set; }

        /// <summary>
        /// Charges on the base
        /// </summary>
        public double qb { get; set; }

        /// <summary>
        /// Excess phase current
        /// </summary>
        public double cex { get; set; }

        /// <summary>
        /// Excess phase conductance
        /// </summary>
        public double gex { get; set; }
    }
}
