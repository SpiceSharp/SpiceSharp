using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Simulations
{
    public abstract partial class TimeSimulation
    {
        /// <summary>
        /// Describes the state of a <see cref="TimeSimulation"/>.
        /// </summary>
        /// <seealso cref="ITimeSimulationState" />
        protected class TimeSimulationState : ITimeSimulationState
        {
            /// <summary>
            /// Gets the integration method.
            /// </summary>
            /// <value>
            /// The integration method.
            /// </value>
            public IntegrationMethod Method { get; set; }

            /// <summary>
            /// Set up the simulation state for the simulation.
            /// </summary>
            /// <param name="simulation">The simulation.</param>
            public void Setup(ISimulation simulation)
            {
                Method.Setup(simulation);
            }

            /// <summary>
            /// Destroys the simulation state.
            /// </summary>
            public void Unsetup()
            {
                Method.Unsetup();
            }
        }
    }
}
