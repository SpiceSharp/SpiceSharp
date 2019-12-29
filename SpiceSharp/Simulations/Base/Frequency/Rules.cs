using SpiceSharp.Simulations.Biasing;
using System.Collections.Generic;

namespace SpiceSharp.Simulations.Frequency
{
    /// <summary>
    /// Necessary rules for frequency simulation.
    /// </summary>
    /// <seealso cref="Biasing.Rules" />
    public class Rules : Biasing.Rules,
        IParameterized<RuleParameters>
    {
        /// <summary>
        /// Gets the frequency parameters.
        /// </summary>
        /// <value>
        /// The frequency parameters.
        /// </value>
        public RuleParameters FrequencyParameters { get; }
        RuleParameters IParameterized<RuleParameters>.Parameters => FrequencyParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rules"/> class.
        /// </summary>
        /// <param name="variables">The variables.</param>
        /// <param name="frequencies">The frequencies that will be evaluated.</param>
        public Rules(IVariableSet variables, IEnumerable<double> frequencies)
            : base(variables)
        {
            FrequencyParameters = new RuleParameters(frequencies);
        }
    }
}
