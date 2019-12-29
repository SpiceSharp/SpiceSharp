using SpiceSharp.Simulations;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An <see cref="IRule"/> that allows specifying an unconditionally conductive path.
    /// </summary>
    /// <seealso cref="IRule" />
    public interface IConductiveRule : IRule
    {
        /// <summary>
        /// Specifies variables as being unconditionally connected by a conductive path.
        /// </summary>
        /// <param name="subject">The subject that applies the conductive paths.</param>
        /// <param name="variables">The variables that are connected.</param>
        void Apply(IRuleSubject subject, params Variable[] variables);
    }
}
