namespace SpiceSharp.Diagnostics.Validation
{
    /// <summary>
    /// Exception for when there is no independent source.
    /// </summary>
    /// <seealso cref="ValidationException" />
    public class NoIndependentSourceException : ValidationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoIndependentSourceException"/> class.
        /// </summary>
        public NoIndependentSourceException()
            : base(Properties.Resources.Validation_NoIndependentSource)
        {
        }
    }
}
