namespace SpiceSharp.Validation
{
    /// <summary>
    /// An interface that describes a rule for validation.
    /// </summary>
    public interface IValidationRule : ICloneable
    {
        /// <summary>
        /// Sets up the validation rule.
        /// </summary>
        void Setup();

        /// <summary>
        /// Finish the check by validating the results.
        /// </summary>
        void Validate();
    }
}
