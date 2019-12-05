namespace SpiceSharp.Validation
{
    /// <summary>
    /// An interface that describes a class that can be validated.
    /// </summary>
    public interface IRuleSubject
    {
        /// <summary>
        /// Applies information about the subject to the rules in the container.
        /// </summary>
        /// <param name="container">The container with all the rules that should be validated.</param>
        void ApplyTo(IRuleContainer container);
    }
}
