using System;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Sweep instance
    /// </summary>
    public class SweepInstance
    {
        /// <summary>
        /// Gets the current value
        /// </summary>
        public double CurrentValue { get; private set; }

        /// <summary>
        /// Gets the current step index
        /// </summary>
        public int CurrentStep { get; private set; }

        /// <summary>
        /// The number of steps
        /// </summary>
        public int Limit { get; }

        /// <summary>
        /// Gets the initial value
        /// </summary>
        public double Initial { get; }

        /// <summary>
        /// Gets the final value
        /// </summary>
        public double Final { get; }

        /// <summary>
        /// Gets the parameter that is swept
        /// </summary>
        public Identifier Parameter { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="n">Steps</param>
        /// <param name="start">Initial value</param>
        /// <param name="stop">Final value</param>
        public SweepInstance(Identifier parameter, double start, double stop, double step)
        {
            Parameter = parameter;
            CurrentStep = 0;
            Initial = start;
            CurrentValue = start;

            // Calculate the limit
            if (Math.Sign(step) * (stop - start) <= 0)
                throw new CircuitException("Invalid sweep: {0} to {1} cannot be reached in steps of {2}".FormatString(start, stop, step));
            Limit = (int)Math.Floor((stop - start) / step + 0.25);

            // Calculate the final value
            Final = start + Limit * step;
        }

        /// <summary>
        /// Reset
        /// </summary>
        public void Reset()
        {
            CurrentValue = Initial;
            CurrentStep = 0;
        }

        /// <summary>
        /// Increment the sweep
        /// </summary>
        public void Increment()
        {
            CurrentStep++;
            CurrentValue = Initial + (Final - Initial) * CurrentStep / Limit;
        }
    }
}
