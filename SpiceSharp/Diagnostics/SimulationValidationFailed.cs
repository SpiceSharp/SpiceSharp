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
        /// Initializes a new instance of the <see cref="SimulationValidationFailed"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The provider.</param>
        public SimulationValidationFailed(ISimulation simulation, IRuleProvider provider) 
            : base(Properties.Resources.Simulations_ValidationFailed.FormatString(simulation.Name, provider.ViolationCount))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimulationValidationFailed"/> class.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="innerException">The inner exception.</param>
        public SimulationValidationFailed(ISimulation simulation, IRuleProvider provider, Exception innerException)
            : base(Properties.Resources.Simulations_ValidationFailed.FormatString(simulation.Name, provider.ViolationCount), innerException)
        {
        }
    }
}
