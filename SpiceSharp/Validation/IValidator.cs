namespace SpiceSharp.Validation
{
    /// <summary>
    /// An interface that describes a class that can be validated.
    /// </summary>
    public interface IValidationEntity
    {
        /// <summary>
        /// Validates this instance.
        /// </summary>
        void Validate(IValidationContainer container);
    }
}
