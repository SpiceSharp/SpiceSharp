using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Components.Noise;

namespace SpiceSharp.Behaviors.RES
{
    /// <summary>
    /// Noise behavior for <see cref="Resistor"/>
    /// </summary>
    public class NoiseBehavior : Behaviors.NoiseBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        LoadBehavior load;

        /// <summary>
        /// Nodes
        /// </summary>
        public int RESposNode { get; protected set; }
        public int RESnegNode { get; protected set; }

        /// <summary>
        /// Get resistor noise sources
        /// </summary>
        public ComponentNoise RESnoise { get; private set; } = new ComponentNoise(new NoiseThermal("thermal", 0, 1));

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="parameters">Parameters</param>
        /// <param name="pool">Behavior pool</param>
        public override void Setup(ParametersCollection parameters, BehaviorPool pool)
        {
            load = pool.GetBehavior<LoadBehavior>();
        }

        /// <summary>
        /// Setup noise behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(Entity component, Circuit ckt)
        {
            var res = component as Resistor;

            // Get behaviors
            load = GetBehavior<LoadBehavior>(component);
        }

        /// <summary>
        /// Connect the noise
        /// </summary>
        /// <param name="nodes"></param>
        public void Connect(params int[] pins)
        {
            RESnoise?.Setup(pins[0], pins[1]);
        }

        /// <summary>
        /// Execute noise behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Noise(Circuit ckt)
        {
            RESnoise.Generators[0].Set(load.RESconduct);
            RESnoise.Evaluate(ckt);
        }
    }
}
