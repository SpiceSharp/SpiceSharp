using System;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Event arguments for modifying charges and currents
    /// </summary>
    public class BJTExcessPhaseEventArgs : EventArgs
    {
        /// <summary>
        /// Collector current
        /// </summary>
        public double cc;

        /// <summary>
        /// Charges on the base
        /// </summary>
        public double qb;

        /// <summary>
        /// Excess phase parameters
        /// </summary>
        public double cex, gex;
    }
}
