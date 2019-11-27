using System;

namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Exception thrown when a solver has not been factored yet.
    /// </summary>
    /// <seealso cref="AlgebraException" />
    public class SolverNotFactoredException : AlgebraException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SolverNotFactoredException"/> class.
        /// </summary>
        public SolverNotFactoredException()
            : base("Solver has not been factored yet")
        {
        }
    }
}
