using SpiceSharp.Circuits;

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
        /// <param name="nodes">The circuit</param>
        public virtual void Initialize(Nodes nodes)
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
