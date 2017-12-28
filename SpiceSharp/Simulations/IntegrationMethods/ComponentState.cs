namespace SpiceSharp.Components
{
    /// <summary>
    /// A class that represents the current state of a component
    /// </summary>
    public abstract class ComponentState
    {
        /// <summary>
        /// Accept the current state as a solution
        /// Used for variables that need to keep a history
        /// </summary>
        public virtual void AcceptState()
        {
            // Do nothing
        }
    }
}
