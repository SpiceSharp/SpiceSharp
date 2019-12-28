using SpiceSharp.Simulations;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An <see cref="IRule"/> that allows specifying a conductive path.
    /// </summary>
    /// <seealso cref="IRule" />
    public interface IConductiveRule : IRule
    {
        /// <summary>
        /// Applies the specified variables as being connected by a conductive path.
        /// </summary>
        /// <param name="subject">The rule subject.</param>
        /// <param name="variables">The variables.</param>
        void Apply(IRuleSubject subject, params Variable[] variables);
    }
}
