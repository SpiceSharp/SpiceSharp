using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;

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
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public NoiseBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>();
        }
        
        /// <summary>
        /// Connect the noise
        /// </summary>
        public void Connect(params int[] pins)
        {
            RESposNode = pins[0];
            RESnegNode = pins[1];
        }

        /// <summary>
        /// Connect the noise
        /// </summary>
        public override void ConnectNoise()
        {
            RESnoise.Setup(RESposNode, RESnegNode);
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        /// <param name="sim">Noise simulation</param>
        public override void Noise(Noise sim)
        {
            RESnoise.Generators[0].Set(load.RESconduct);
            RESnoise.Evaluate(sim);
        }
    }
}
