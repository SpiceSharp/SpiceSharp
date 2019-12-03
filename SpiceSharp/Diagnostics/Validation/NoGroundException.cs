namespace SpiceSharp.Diagnostics.Validation
{
    /// <summary>
    /// Exception for when no ground node has been referenced.
    /// </summary>
    /// <seealso cref="ValidationException" />
    public class NoGroundException : ValidationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoGroundException"/> class.
        /// </summary>
        public NoGroundException()
            : base(Properties.Resources.Validation_NoGround)
        {
        }
    }
}
