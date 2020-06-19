using SpiceSharp.ParameterSets;
using System.Collections.Generic;

namespace SpiceSharp.Simulations.Frequency
{
    /// <summary>
    /// Necessary rules for frequency simulation.
    /// </summary>
    /// <seealso cref="Biasing.Rules" />
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="RuleParameters"/>
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
        /// <param name="state">The biasing simulation state.</param>
        /// <param name="frequencies">The frequencies that will be evaluated.</param>
        public Rules(ISolverSimulationState<double> state, IEnumerable<double> frequencies)
            : base(state, state.Comparer)
        {
            FrequencyParameters = new RuleParameters(frequencies);
        }
    }
}
