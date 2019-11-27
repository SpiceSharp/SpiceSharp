namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Exception thrown when the solution vector is not suitable for storing the solution of the solver equations.
    /// </summary>
    /// <seealso cref="AlgebraException" />
    public class InvalidSolutionVectorException : AlgebraException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidSolutionVectorException"/> class.
        /// </summary>
        public InvalidSolutionVectorException()
            : base("Solution vector does not match solver size")
        {
        }
    }
}
