using System;
using SpiceSharp.Behaviours;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// Noise behaviour for <see cref="MOS2"/>
    /// </summary>
    public class MOS2NoiseBehaviour : CircuitObjectBehaviourNoise
    {
        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise MOS2noise { get; } = new ComponentNoise(
            new Noise.NoiseThermal("rd", 0, 4),
            new Noise.NoiseThermal("rs", 2, 5),
            new Noise.NoiseThermal("id", 4, 5),
            new Noise.NoiseGain("1overf", 4, 5)
            );

        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(ICircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var mos2 = ComponentTyped<MOS2>();
            MOS2noise.Setup(ckt,
                mos2.MOS2dNode,
                mos2.MOS2gNode,
                mos2.MOS2sNode,
                mos2.MOS2bNode,
                mos2.MOS2dNodePrime,
                mos2.MOS2sNodePrime);
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            var mos2 = ComponentTyped<MOS2>();
            var model = mos2.Model as MOS2Model;
            var state = ckt.State;
            var noise = state.Noise;

            double Kf = model.MOS2fNcoef * Math.Exp(model.MOS2fNexp * Math.Log(Math.Max(Math.Abs(mos2.MOS2cd), 1e-38))) / (mos2.MOS2w * (mos2.MOS2l - 2 * model.MOS2latDiff) * model.MOS2oxideCapFactor * model.MOS2oxideCapFactor);

            MOS2noise.Evaluate(ckt,
                mos2.MOS2drainConductance,
                mos2.MOS2sourceConductance,
                2.0 / 3.0 * Math.Abs(mos2.MOS2gm),
                Kf / noise.Freq);
        }
    }
}
