using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviours
{
    public abstract class CircuitObjectBehaviour: ICircuitObjectBehaviour
    {
        protected ICircuitObject Component { get; private set; }

        protected T ComponentTyped<T>() where T: class, ICircuitObject
        {
            return Component as T;
        }

        public virtual void Setup(ICircuitObject component, Circuit ckt)
        {
            this.Component = component;
        }

        public abstract void Execute(Circuit ckt);
    }
}
