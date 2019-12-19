using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A description of an integration method.
    /// </summary>
    /// <seealso cref="IParameterSet" />
    public interface IIntegrationMethodDescription : IParameterSet
    {
        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        double StartTime { get; set; }

        /// <summary>
        /// Gets or sets the stop time.
        /// </summary>
        /// <value>
        /// The stop time.
        /// </value>
        double StopTime { get; set; }

        /// <summary>
        /// Gets or sets the minimum step.
        /// </summary>
        /// <value>
        /// The minimum step.
        /// </value>
        double MinStep { get; }

        /// <summary>
        /// Gets or sets the maximum step.
        /// </summary>
        /// <value>
        /// The maximum step.
        /// </value>
        double MaxStep { get; set; }

        /// <summary>
        /// Gets or sets the initial step.
        /// </summary>
        /// <value>
        /// The initial step.
        /// </value>
        double InitialStep { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use ic].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use ic]; otherwise, <c>false</c>.
        /// </value>
        bool UseIc { get; set; }

        /// <summary>
        /// Gets or sets the tran maximum iterations.
        /// </summary>
        /// <value>
        /// The tran maximum iterations.
        /// </value>
        int TranMaxIterations { get; set; }

        /// <summary>
        /// Gets the initial conditions.
        /// </summary>
        /// <value>
        /// The initial conditions.
        /// </value>
        Dictionary<string, double> InitialConditions { get; }

        /// <summary>
        /// Creates an instance of the integration method for an associated <see cref="IBiasingSimulationState"/>.
        /// </summary>
        /// <param name="simulation">The simulation that provides the biasing state.</param>
        /// <returns>
        /// The integration method.
        /// </returns>
        IIntegrationMethod Create(IStateful<IBiasingSimulationState> simulation);
    }
}
