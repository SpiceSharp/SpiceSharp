using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Components.Noise;

namespace SpiceSharp.Behaviors.RES
{
    /// <summary>
    /// Noise behaviour for <see cref="Resistor"/>
    /// </summary>
    public class NoiseBehavior : Behaviors.NoiseBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private LoadBehavior load;

        /// <summary>
        /// Get resistor noise sources
        /// </summary>
        public ComponentNoise RESnoise { get; private set; } = new ComponentNoise(new NoiseThermal("thermal", 0, 1));

        /// <summary>
        /// Setup noise behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var res = component as Resistor;

            // Get behaviors
            load = GetBehavior<LoadBehavior>(component);

            RESnoise?.Setup(ckt, res.RESposNode, res.RESnegNode);
        }

        /// <summary>
        /// Execute noise behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Noise(Circuit ckt)
        {
            RESnoise.Generators[0].Set(load.RESconduct);
            RESnoise.Evaluate(ckt);
        }
    }
}
