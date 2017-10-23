using System;
using SpiceSharp.Behaviours;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// Noise behaviour for <see cref="MOS3"/>
    /// </summary>
    public class MOS3NoiseBehaviour : CircuitObjectBehaviourNoise
    {
        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise MOS3noise { get; } = new ComponentNoise(
            new Noise.NoiseThermal("rd", 0, 4),
            new Noise.NoiseThermal("rs", 2, 5),
            new Noise.NoiseThermal("id", 4, 5),
            new Noise.NoiseGain("1overf", 4, 5)
            );

        /// <summary>
        /// Setup behaviour
        /// </summary>
        /// <param name="component"></param>
        /// <param name="ckt"></param>
        public override void Setup(ICircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var mos3 = ComponentTyped<MOS3>();
            MOS3noise.Setup(ckt,
                mos3.MOS3dNode,
                mos3.MOS3gNode,
                mos3.MOS3sNode,
                mos3.MOS3bNode,
                mos3.MOS3dNodePrime,
                mos3.MOS3sNodePrime);
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var mos3 = ComponentTyped<MOS3>();
            var model = mos3.Model as MOS3Model;
            var state = ckt.State;
            var noise = state.Noise;

            double Kf = model.MOS3fNcoef * Math.Exp(model.MOS3fNexp * Math.Log(Math.Max(Math.Abs(mos3.MOS3cd), 1e-38))) / (mos3.MOS3w * (mos3.MOS3l - 2 * model.MOS3latDiff) * model.MOS3oxideCapFactor * model.MOS3oxideCapFactor);

            MOS3noise.Evaluate(ckt,
                mos3.MOS3drainConductance,
                mos3.MOS3sourceConductance,
                2.0 / 3.0 * Math.Abs(mos3.MOS3gm),
                Kf / noise.Freq);
        }
    }
}
