using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviours
{
    public interface ICircuitObjectBehaviour
    {
        void Setup(ICircuitObject component, Circuit ckt);
        void Execute(Circuit ckt);
    }
}
