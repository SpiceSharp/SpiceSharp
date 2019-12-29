using SpiceSharp.Validation;
using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An exception thrown when a simulation fails its validation.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    public class SimulationValidationFailed : SpiceSharpException
    {
        /// <summary>
        /// Gets the rules.
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        public IRuleProvider Rules { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimulationValidationFailed"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="rules">The rule provider.</param>
        public SimulationValidationFailed(ISimulation simulation, IRuleProvider rules) 
            : base(Properties.Resources.Simulations_ValidationFailed.FormatString(simulation?.Name, rules?.ViolationCount))
        {
            Rules = rules;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimulationValidationFailed"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="rules">The rule provider.</param>
        /// <param name="innerException">The inner exception.</param>
        public SimulationValidationFailed(ISimulation simulation, IRuleProvider rules, Exception innerException)
            : base(Properties.Resources.Simulations_ValidationFailed.FormatString(simulation?.Name, rules?.ViolationCount), innerException)
        {
            Rules = rules;
        }
    }
}
