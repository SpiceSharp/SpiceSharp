using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations.Sweeps
{
    /// <summary>
    /// A linear sweep
    /// </summary>
    public class LinearSweep : Sweep<double>
    {
        /// <summary>
        /// Gets all points in the sweep
        /// </summary>
        public override IEnumerable<double> Points
        {
            get
            {
                // Initialize
                Current = Initial;

                // Go through the list
                for (int i = 0; i < Count; i++)
                {
                    Current = Initial + (Final - Initial) * i / Count;
                    yield return Current;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initial">Initial value</param>
        /// <param name="final">Final value</param>
        /// <param name="count">Number of points</param>
        public LinearSweep(double initial, double final, int count)
            : base(initial, final, count)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initial">Initial value</param>
        /// <param name="final">Final value</param>
        /// <param name="delta">Step</param>
        public LinearSweep(double initial, double final, double delta)
            : base(initial, final, 1)
        {
            // Calculate the number of steps
            Count = (int)Math.Floor((final - initial) / delta + 0.25);

            // Update the final value
            Final = initial + delta * Count;
        }
    }
}
