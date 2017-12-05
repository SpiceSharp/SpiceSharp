using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Noise;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Noise behaviour for <see cref="Resistor"/>
    /// </summary>
    public class ResistorNoiseBehavior : CircuitObjectBehaviorNoise
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
        public override void Noise(Circuit ckt)
        {
            RESnoise.Generators[0].Set(ComponentTyped<Resistor>().RESconduct);
            RESnoise.Evaluate(ckt);
        }
    }
}
