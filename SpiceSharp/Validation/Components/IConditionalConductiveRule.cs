using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An <see cref="IRule"/> that allows specifying a conditionally conductive path.
    /// </summary>
    /// <seealso cref="IRule" />
    public interface IConditionalConductiveRule : IConductiveRule
    {
        /// <summary>
        /// Specifies variables as being conditionally connected by a conductive path.
        /// </summary>
        /// <param name="subject">The subject that applies the connection.</param>
        /// <param name="condition">The condition that needs to be true before this path is connected.</param>
        /// <param name="variables">The variables that are connected if the condition is true.</param>
        void Apply(IRuleSubject subject, Func<bool> condition, params Variable[] variables);
    }
}
