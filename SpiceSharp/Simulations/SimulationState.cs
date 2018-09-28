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
        /// Setup the simulation state.
        /// </summary>
        /// <param name="nodes">The unknown variables for which the state is used.</param>
        public virtual void Setup(VariableSet nodes)
        {
            IsSetup = true;
        }

        /// <summary>
        /// Unsetup the state.
        /// </summary>
        public virtual void Destroy()
        {
            // TODO: Call this Unsetup
            IsSetup = false;
        }
    }
}
