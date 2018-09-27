namespace SpiceSharp.Components
{
    /// <summary>
    /// Behaviors for component behaviors that can be connected in the circuit.
    /// </summary>
    public interface IConnectedBehavior
    {
        /// <summary>
        /// Connect the behavior in the circuit
        /// </summary>
        /// <param name="pins">Pin indices in order</param>
        void Connect(params int[] pins);
    }
}
