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
        /// Fixes the voltage difference between two node variables.
        /// </summary>
        /// <param name="subject">The subject that applies to the rule.</param>
        /// <param name="first">The first variable.</param>
        /// <param name="second">The second variable.</param>
        void Fix(IRuleSubject subject, IVariable first, IVariable second);
    }
}
