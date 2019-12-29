namespace SpiceSharp.Validation
{
    /// <summary>
    /// Describes a class that applies to a certain rule.
    /// </summary>
    public interface IRuleSubject
    {
        /// <summary>
        /// Applies the subject to any rules in the validation provider.
        /// </summary>
        /// <param name="rules">The provider.</param>
        void Apply(IRules rules);
    }
}
