using SpiceSharp.Circuits;
using SpiceSharp.Behaviours;
using SpiceSharp.Components.Noise;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// Noise behaviour of a resistor
    /// </summary>
    public class ResistorNoiseBehaviour : CircuitObjectBehaviourNoise
    {
        /// <summary>
        /// Get resistor noise sources
        /// </summary>
        public ComponentNoise RESnoise { get; private set; } = new ComponentNoise(new NoiseThermal("thermal", 0, 1));

        /// <summary>
        /// Setup noise behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(ICircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var res = ComponentTyped<Resistor>();
            RESnoise?.Setup(ckt, res.RESposNode, res.RESnegNode);
        }

        /// <summary>
        /// Execute noise behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            RESnoise.Evaluate(ckt, ComponentTyped<Resistor>().RESconduct);
        }
    }
}
