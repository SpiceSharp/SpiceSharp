using System;
using SpiceSharp.Behaviours;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// Noise behaviour for <see cref="BJT"/>
    /// </summary>
    public class BJTNoiseBehaviour : CircuitObjectBehaviourNoise
    {
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
        /// Setup the BJT behaviour for noise analysis
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(ICircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);

            BJT bjt = ComponentTyped<BJT>();
            BJTnoise.Setup(ckt,
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
        public override void Execute(Circuit ckt)
        {
            var bjt = ComponentTyped<BJT>();
            var model = bjt.Model as BJTModel;
            var state = ckt.State;
            var noise = state.Noise;

            double Kf = model.BJTfNcoef * Math.Exp(model.BJTfNexp * Math.Log(Math.Max(Math.Abs(state.States[0][bjt.BJTstate + BJT.BJTcb]), 1e-38)));

            BJTnoise.Evaluate(ckt,
                model.BJTcollectorConduct * bjt.BJTarea,
                state.States[0][bjt.BJTstate + BJT.BJTgx],
                model.BJTemitterConduct * bjt.BJTarea,
                state.States[0][bjt.BJTstate + BJT.BJTcc],
                state.States[0][bjt.BJTstate + BJT.BJTcb],
                Kf / noise.Freq);
        }
    }
}
