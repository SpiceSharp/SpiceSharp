using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Class that implements a sweep.
    /// </summary>
    public class SweepInstance
    {
        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <value>
        /// The current value.
        /// </value>
        public double CurrentValue { get; private set; }

        /// <summary>
        /// Gets the current step index.
        /// </summary>
        /// <value>
        /// The current step.
        /// </value>
        public int CurrentStep { get; private set; }

        /// <summary>
        /// Gets the maximum number of steps.
        /// </summary>
        /// <value>
        /// The limit.
        /// </value>
        public int Limit { get; }

        /// <summary>
        /// Gets the initial value.
        /// </summary>
        /// <value>
        /// The initial.
        /// </value>
        public double Initial { get; }

        /// <summary>
        /// Gets the final value.
        /// </summary>
        /// <value>
        /// The final.
        /// </value>
        public double Final { get; }

        /// <summary>
        /// Gets the parameter identifier that is swept.
        /// </summary>
        /// <value>
        /// The parameter.
        /// </value>
        public Identifier Parameter { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SweepInstance"/> class.
        /// </summary>
        /// <param name="parameter">The parameter identifier.</param>
        /// <param name="start">The initial value.</param>
        /// <param name="stop">The final value.</param>
        /// <param name="step">The step value.</param>
        /// <exception cref="SpiceSharp.CircuitException">Invalid sweep: {0} to {1} cannot be reached in steps of {2}".FormatString(start, stop, step)</exception>
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
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            CurrentValue = Initial;
            CurrentStep = 0;
        }

        /// <summary>
        /// Go to the next step in the sweep.
        /// </summary>
        public void Increment()
        {
            CurrentStep++;
            CurrentValue = Initial + (Final - Initial) * CurrentStep / Limit;
        }
    }
}
