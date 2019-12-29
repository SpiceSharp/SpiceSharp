using System.Collections.Generic;

namespace SpiceSharp.Simulations.Frequency
{
    /// <summary>
    /// Rule parameters for a <see cref="Rules"/>.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class RuleParameters : ParameterSet
    {
        /// <summary>
        /// Gets the frequencies.
        /// </summary>
        /// <value>
        /// The frequencies.
        /// </value>
        public IEnumerable<double> Frequencies { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleParameters"/> class.
        /// </summary>
        /// <param name="frequencies">The frequencies.</param>
        public RuleParameters(IEnumerable<double> frequencies)
        {
            Frequencies = frequencies.ThrowIfNull(nameof(frequencies));
        }
    }
}
