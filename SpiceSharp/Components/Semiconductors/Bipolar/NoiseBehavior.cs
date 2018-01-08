using System;
using SpiceSharp.Components;
using SpiceSharp.Components.Noise;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;

namespace SpiceSharp.Behaviors.Bipolar
{
    /// <summary>
    /// Noise behavior for <see cref="Bipolar"/>
    /// </summary>
    public class NoiseBehavior : Behaviors.NoiseBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private LoadBehavior load;
        private ModelNoiseBehavior modelnoise;
        private ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("area"), SpiceInfo("Area factor")]
        public Parameter BJTarea { get; } = new Parameter(1);

        /// <summary>
        /// Noise sources by their index
        /// </summary>
        private const int BJTRCNOIZ = 0;
        private const int BJTRBNOIZ = 1;
        private const int BJT_RE_NOISE = 2;
        private const int BJTICNOIZ = 3;
        private const int BJTIBNOIZ = 4;
        private const int BJTFLNOIZ = 5;

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
        /// Setup the BJT behavior for noise analysis
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(Entity component, Circuit ckt)
        {
            var bjt = component as Components.BJT;

            // Get behaviors
            load = GetBehavior<LoadBehavior>(component);
            modeltemp = GetBehavior<ModelTemperatureBehavior>(bjt.Model);
            modelnoise = GetBehavior<ModelNoiseBehavior>(bjt.Model);

            BJTnoise.Setup(
                bjt.BJTcolNode,
                bjt.BJTbaseNode,
                bjt.BJTemitNode,
                bjt.BJTsubstNode,
                bjt.BJTcolPrimeNode,
                bjt.BJTbasePrimeNode,
                bjt.BJTemitPrimeNode);
        }

        /// <summary>
        /// Perform noise simulation
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Noise(Circuit ckt)
        {
            var state = ckt.State;
            var noise = state.Noise;

            // Set noise parameters
            BJTnoise.Generators[BJTRCNOIZ].Set(modeltemp.BJTcollectorConduct * BJTarea);
            BJTnoise.Generators[BJTRBNOIZ].Set(state.States[0][load.BJTstate + LoadBehavior.BJTgx]);
            BJTnoise.Generators[BJT_RE_NOISE].Set(modeltemp.BJTemitterConduct * BJTarea);
            BJTnoise.Generators[BJTICNOIZ].Set(state.States[0][load.BJTstate + LoadBehavior.BJTcc]);
            BJTnoise.Generators[BJTIBNOIZ].Set(state.States[0][load.BJTstate + LoadBehavior.BJTcb]);
            BJTnoise.Generators[BJTFLNOIZ].Set(modelnoise.BJTfNcoef * Math.Exp(modelnoise.BJTfNexp * Math.Log(Math.Max(Math.Abs(state.States[0][load.BJTstate + LoadBehavior.BJTcb]), 1e-38))) / noise.Freq);

            // Evaluate all noise sources
            BJTnoise.Evaluate(ckt);
        }
    }
}
