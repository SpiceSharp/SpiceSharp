using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that implements a linear sweep.
    /// </summary>
    /// <seealso cref="Sweep{T}" />
    public class LinearSweep : Sweep<double>
    {
        /// <summary>
        /// Gets an enumeration of the points in the sweep.
        /// </summary>
        public override IEnumerable<double> Points
        {
            get
            {
                // Initialize
                Current = Initial;

                // Go through the list
                for (var i = 0; i < Count; i++)
                {
                    Current = Initial + (Final - Initial) * i / Count;
                    yield return Current;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearSweep"/> class.
        /// </summary>
        /// <param name="initial">The initial value.</param>
        /// <param name="final">The final value.</param>
        /// <param name="count">The number of points.</param>
        public LinearSweep(double initial, double final, int count)
            : base(initial, final, count)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearSweep"/> class.
        /// </summary>
        /// <param name="initial">The initial value.</param>
        /// <param name="final">The final value.</param>
        /// <param name="delta">The step size.</param>
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
