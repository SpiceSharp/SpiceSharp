using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class implements a sweep with a number of points per decade.
    /// </summary>
    /// <seealso cref="Sweep{T}" />
    public class DecadeSweep : Sweep<double>
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

                // Go through the points
                for (var i = 0; i < Count; i++)
                {
                    yield return Current;

                    // Go to the next point
                    Current = Current * _freqDelta;
                }
            }
        }

        /// <summary>
        /// Multiplication factor
        /// </summary>
        private readonly double _freqDelta;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecadeSweep"/> class.
        /// </summary>
        /// <param name="initial">The initial value.</param>
        /// <param name="final">The final value.</param>
        /// <param name="steps">The number of steps per decade.</param>
        public DecadeSweep(double initial, double final, int steps)
        {
            if (final * initial <= 0)
                throw new CircuitException("Invalid decade sweep from {0} to {1}".FormatString(initial, final));

            Initial = initial;
            Final = final;
            _freqDelta = Math.Exp(Math.Log(10.0) / steps);
            Count = (int)Math.Floor(Math.Log(final / initial) / Math.Log(_freqDelta) + 0.25) + 1;
        }
    }
}
