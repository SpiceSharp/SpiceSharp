using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviours
{
    public interface ICircuitObjectBehavior
    {
        void Setup(ICircuitObject component, Circuit ckt);
        void Execute(Circuit ckt);
    }
}
