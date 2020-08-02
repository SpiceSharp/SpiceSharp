using System;

namespace SpiceSharp.Components.Bipolars
{
    /// <summary>
    /// Event arguments for modifying charges and currents
    /// </summary>
    public class ExcessPhaseEventArgs : EventArgs
    {
        /// <summary>
        /// Collector current
        /// </summary>
        public double CollectorCurrent { get; set; }

        /// <summary>
        /// Charges on the base
        /// </summary>
        public double BaseCharge { get; set; }

        /// <summary>
        /// Excess phase current
        /// </summary>
        public double ExcessPhaseCurrent { get; set; }

        /// <summary>
        /// Excess phase conductance
        /// </summary>
        public double ExcessPhaseConduct { get; set; }
    }
}
