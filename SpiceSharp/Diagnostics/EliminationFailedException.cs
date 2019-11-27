namespace SpiceSharp.Algebra
{
    /// <summary>
    /// Exception thrown when elimination has failed.
    /// </summary>
    /// <seealso cref="AlgebraException" />
    public class EliminationFailedException : AlgebraException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EliminationFailedException"/> class.
        /// </summary>
        public EliminationFailedException()
            : base("Elimination failed")
        {
        }
    }
}
