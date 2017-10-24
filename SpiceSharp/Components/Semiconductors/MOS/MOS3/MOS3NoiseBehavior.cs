using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Noise behaviour for <see cref="MOS3"/>
    /// </summary>
    public class MOS3NoiseBehavior : CircuitObjectBehaviorNoise
    {
        /// <summary>
        /// Noise generators by their index
        /// </summary>
        private const int MOS3RDNOIZ = 0;
        private const int MOS3RSNOIZ = 1;
        private const int MOS3IDNOIZ = 2;
        private const int MOS3FLNOIZ = 3;

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

            // Set noise parameters
            MOS3noise.Generators[MOS3RDNOIZ].Set(mos3.MOS3drainConductance);
            MOS3noise.Generators[MOS3RSNOIZ].Set(mos3.MOS3sourceConductance);
            MOS3noise.Generators[MOS3IDNOIZ].Set(2.0 / 3.0 * Math.Abs(mos3.MOS3gm));
            MOS3noise.Generators[MOS3FLNOIZ].Set(model.MOS3fNcoef * Math.Exp(model.MOS3fNexp * Math.Log(Math.Max(Math.Abs(mos3.MOS3cd), 1e-38))) / (mos3.MOS3w * (mos3.MOS3l - 2 * model.MOS3latDiff) * model.MOS3oxideCapFactor * model.MOS3oxideCapFactor) / noise.Freq);

            // Evaluate noise sources
            MOS3noise.Evaluate(ckt);
        }
    }
}
