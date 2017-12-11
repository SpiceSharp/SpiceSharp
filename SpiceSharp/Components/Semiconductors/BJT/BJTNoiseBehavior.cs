using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Noise behaviour for <see cref="BJT"/>
    /// </summary>
    public class BJTNoiseBehavior : CircuitObjectBehaviorNoise
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BJTModelNoiseBehavior modelnoise;
        private BJTModelTemperatureBehavior modeltemp;

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
            new Noise.NoiseThermal("rc", 0, 4),
            new Noise.NoiseThermal("rb", 1, 5),
            new Noise.NoiseThermal("re", 2, 6),
            new Noise.NoiseShot("ic", 4, 6),
            new Noise.NoiseShot("ib", 5, 6),
            new Noise.NoiseGain("1overf", 5, 6)
            );

        /// <summary>
        /// Private variables
        /// </summary>
        private int BJTstate;

        /// <summary>
        /// Setup the BJT behaviour for noise analysis
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            var bjt = component as BJT;
            var model = bjt.Model as BJTModel;

            // Get behaviors
            modeltemp = model.GetBehavior(typeof(CircuitObjectBehaviorTemperature)) as BJTModelTemperatureBehavior;
            modelnoise = model.GetBehavior(typeof(CircuitObjectBehaviorNoise)) as BJTModelNoiseBehavior;
            var load = bjt.GetBehavior(typeof(CircuitObjectBehaviorLoad)) as BJTLoadBehavior;
            BJTstate = load.BJTstate;

            BJTnoise.Setup(ckt,
                bjt.BJTcolNode,
                bjt.BJTbaseNode,
                bjt.BJTemitNode,
                bjt.BJTsubstNode,
                bjt.BJTcolPrimeNode,
                bjt.BJTbasePrimeNode,
                bjt.BJTemitPrimeNode);
            return true;
        }

        /// <summary>
        /// Perform noise simulation
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Noise(Circuit ckt)
        {
            var bjt = ComponentTyped<BJT>();
            var model = bjt.Model as BJTModel;
            var state = ckt.State;
            var noise = state.Noise;

            // Set noise parameters
            BJTnoise.Generators[BJTRCNOIZ].Set(modeltemp.BJTcollectorConduct * BJTarea);
            BJTnoise.Generators[BJTRBNOIZ].Set(state.States[0][BJTstate + BJTLoadBehavior.BJTgx]);
            BJTnoise.Generators[BJT_RE_NOISE].Set(modeltemp.BJTemitterConduct * BJTarea);
            BJTnoise.Generators[BJTICNOIZ].Set(state.States[0][BJTstate + BJTLoadBehavior.BJTcc]);
            BJTnoise.Generators[BJTIBNOIZ].Set(state.States[0][BJTstate + BJTLoadBehavior.BJTcb]);
            BJTnoise.Generators[BJTFLNOIZ].Set(modelnoise.BJTfNcoef * Math.Exp(modelnoise.BJTfNexp * Math.Log(Math.Max(Math.Abs(state.States[0][BJTstate + BJTLoadBehavior.BJTcb]), 1e-38))) / noise.Freq);

            // Evaluate all noise sources
            BJTnoise.Evaluate(ckt);
        }
    }
}
