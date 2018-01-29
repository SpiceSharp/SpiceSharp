using System;
using System.Collections.Generic;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Simulations.Sweeps
{
    public class OctaveSweep : Sweep<double>
    {
        /// <summary>
        /// Get the points
        /// </summary>
        public override IEnumerable<double> Points
        {
            get
            {
                // Initialize
                Current = Initial;

                // Go through the points
                for (int i = 0; i < Count; i++)
                {
                    yield return Current;

                    // Go to the next point
                    Current = Current * freqDelta;
                }
            }
        }

        /// <summary>
        /// Multiplication factor
        /// </summary>
        double freqDelta;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initial">Initial value</param>
        /// <param name="final">Final value</param>
        /// <param name="steps">Steps per decade</param>
        public OctaveSweep(double initial, double final, int steps)
        {
            if (final * initial <= 0)
                throw new CircuitException("Invalid decade sweep from {0} to {1}".FormatString(initial, final));

            Initial = initial;
            Final = final;
            freqDelta = Math.Exp(Math.Log(2.0) / steps);
            Count = (int)Math.Floor(Math.Log(final / initial) / Math.Log(freqDelta) + 0.25) + 1;
        }
    }
}
