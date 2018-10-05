namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class represents a simulation state.
    /// </summary>
    public abstract class SimulationState
    {
        /// <summary>
        /// Gets a value indicating whether this state is set up.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is setup; otherwise, <c>false</c>.
        /// </value>
        public bool IsSetup { get; private set; }

        /// <summary>
        /// Sets up the simulation state.
        /// </summary>
        /// <param name="nodes">The unknown variables for which the state is used.</param>
        public virtual void Setup(VariableSet nodes)
        {
            IsSetup = true;
        }

        /// <summary>
        /// Destroys the state.
        /// </summary>
        public virtual void Unsetup()
        {
            IsSetup = false;
        }
    }
}
