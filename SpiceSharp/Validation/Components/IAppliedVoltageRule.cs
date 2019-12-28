using SpiceSharp.Simulations;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An <see cref="IRule"/> that allows specifying an applied voltage.
    /// </summary>
    /// <seealso cref="IRule" />
    public interface IAppliedVoltageRule : IRule
    {
        /// <summary>
        /// Applies a fixed-voltage relation between two variables.
        /// </summary>
        /// <param name="subject">The subject that applies to the rule.</param>
        /// <param name="first">The first variable.</param>
        /// <param name="second">The second variable.</param>
        void Apply(IRuleSubject subject, Variable first, Variable second);
    }
}
