using SpiceSharp.Behaviours;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Represents a circuit object that can be connected in the circuit.
    /// </summary>
    public interface ICircuitComponentWithBehaviours : ICircuitComponent
    {
        ICircuitObjectBehavior[] CurrentBehaviours { get; set; }
    }
}
