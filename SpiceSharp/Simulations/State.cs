namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Class representing a state
    /// </summary>
    public abstract class State
    {
        /// <summary>
        /// Initialize the state
        /// </summary>
        public bool IsSetup { get; private set; }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="nodes">The circuit</param>
        public virtual void Setup(VariableSet nodes)
        {
            IsSetup = true;
        }

        /// <summary>
        /// Destroy the state
        /// </summary>
        public virtual void Destroy()
        {
            IsSetup = false;
        }
    }
}
