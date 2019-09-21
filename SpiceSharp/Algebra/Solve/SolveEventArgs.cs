using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Event arguments used by an <see cref="ISolver{T}"/> when calling events.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class SolveEventArgs<T> : EventArgs where T : IFormattable
    {
        /// <summary>
        /// Gets the solution.
        /// </summary>
        /// <value>
        /// The solution.
        /// </value>
        public IVector<T> Solution { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolveEventArgs{T}"/> class.
        /// </summary>
        /// <param name="solution">The solution.</param>
        public SolveEventArgs(IVector<T> solution)
        {
            Solution = solution.ThrowIfNull(nameof(solution));
        }
    }
}
