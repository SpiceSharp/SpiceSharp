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
        public bool Initialized { get; private set; }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="circuit">The circuit</param>
        public virtual void Initialize(Circuit circuit)
        {
            Initialized = true;
        }

        /// <summary>
        /// Destroy the state
        /// </summary>
        public virtual void Destroy()
        {
            Initialized = false;
        }
    }
}
