namespace SpiceSharp.Validation
{
    /// <summary>
    /// An interface that describes a class that can be validated.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <param name="container">The container with all the rules that should be validated.</param>
        void Validate(IRuleContainer container);
    }
}
