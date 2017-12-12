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
        /// Necessary behaviors
        /// </summary>
        private ResistorLoadBehavior load;

        /// <summary>
        /// Get resistor noise sources
        /// </summary>
        public ComponentNoise RESnoise { get; private set; } = new ComponentNoise(new NoiseThermal("thermal", 0, 1));

        /// <summary>
        /// Setup noise behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            var res = component as Resistor;
            load = GetBehavior<ResistorLoadBehavior>(component);
            RESnoise?.Setup(ckt, res.RESposNode, res.RESnegNode);
            return true;
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
