using System;
using SpiceSharp.Components;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Components.Bipolar;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.Bipolar
{
    /// <summary>
    /// Noise behavior for <see cref="Bipolar"/>
    /// </summary>
    public class NoiseBehavior : Behaviors.NoiseBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        LoadBehavior load;
        ModelNoiseParameters mnp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Noise sources by their index
        /// </summary>
        const int BJTRCNOIZ = 0;
        const int BJTRBNOIZ = 1;
        const int BJT_RE_NOISE = 2;
        const int BJTICNOIZ = 3;
        const int BJTIBNOIZ = 4;
        const int BJTFLNOIZ = 5;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise BJTnoise { get; } = new ComponentNoise(
            new NoiseThermal("rc", 0, 4),
            new NoiseThermal("rb", 1, 5),
            new NoiseThermal("re", 2, 6),
            new NoiseShot("ic", 4, 6),
            new NoiseShot("ib", 5, 6),
            new NoiseGain("1overf", 5, 6)
            );

        /// <summary>
        /// Nodes
        /// </summary>
        int BJTcolNode, BJTbaseNode, BJTemitNode, BJTsubstNode, BJTcolPrimeNode, BJTbasePrimeNode, BJTemitPrimeNode;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public NoiseBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();
            mnp = provider.GetParameters<ModelNoiseParameters>(1);

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>();
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            BJTcolNode = pins[0];
            BJTbaseNode = pins[1];
            BJTemitNode = pins[2];
            BJTsubstNode = pins[3];
        }

        /// <summary>
        /// Connect noise sources
        /// </summary>
        public override void ConnectNoise()
        {
            // Get extra nodes
            BJTcolPrimeNode = load.BJTcolPrimeNode;
            BJTbasePrimeNode = load.BJTbasePrimeNode;
            BJTemitPrimeNode = load.BJTemitPrimeNode;

            // Connect noise
            BJTnoise.Setup(BJTcolNode, BJTbaseNode, BJTemitNode, BJTsubstNode,
                BJTcolPrimeNode, BJTbasePrimeNode, BJTemitPrimeNode);
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        /// <param name="sim">Noise simulation</param>
        public override void Noise(Noise sim)
        {
            var state = sim.State;
            var noise = state.Noise;

            // Set noise parameters
            BJTnoise.Generators[BJTRCNOIZ].Set(modeltemp.BJTcollectorConduct * bp.BJTarea);
            BJTnoise.Generators[BJTRBNOIZ].Set(load.BJTgx);
            BJTnoise.Generators[BJT_RE_NOISE].Set(modeltemp.BJTemitterConduct * bp.BJTarea);
            BJTnoise.Generators[BJTICNOIZ].Set(load.BJTcc);
            BJTnoise.Generators[BJTIBNOIZ].Set(load.BJTcb);
            BJTnoise.Generators[BJTFLNOIZ].Set(mnp.BJTfNcoef * Math.Exp(mnp.BJTfNexp * Math.Log(Math.Max(Math.Abs(load.BJTcb), 1e-38))) / noise.Freq);

            // Evaluate all noise sources
            BJTnoise.Evaluate(sim);
        }
    }
}
