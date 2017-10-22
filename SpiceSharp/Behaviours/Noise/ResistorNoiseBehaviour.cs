using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Components.Noise;

namespace SpiceSharp.Behaviours.Noise
{
    internal class ResistorNoiseBehaviour : CircuitObjectBehavior
    {
        public ComponentNoise RESnoise { get; private set; } = new ComponentNoise(new NoiseThermal("thermal", 0, 1));

        public override void Setup(ICircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            RESnoise?.Setup(ckt, ComponentTyped<Resistor>().RESposNode, ComponentTyped<Resistor>().RESnegNode);
        }

        public override void Execute(Circuit ckt)
        {
            RESnoise.Evaluate(ckt, ComponentTyped<Resistor>().RESconduct);
        }
    }
}
