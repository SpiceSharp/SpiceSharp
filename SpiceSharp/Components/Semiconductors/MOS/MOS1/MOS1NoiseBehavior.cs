using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Noise behaviour for a <see cref="MOS1"/>
    /// </summary>
    public class MOS1NoiseBehavior : CircuitObjectBehaviorNoise
    {
        private const int MOS1RDNOIZ = 0;
        private const int MOS1RSNOIZ = 1;
        private const int MOS1IDNOIZ = 2;
        private const int MOS1FLNOIZ = 3;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise MOS1noise { get; } = new ComponentNoise(
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
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var mos1 = ComponentTyped<MOS1>();
            MOS1noise.Setup(ckt,
                mos1.MOS1dNode,
                mos1.MOS1gNode,
                mos1.MOS1sNode,
                mos1.MOS1bNode,
                mos1.MOS1dNodePrime,
                mos1.MOS1sNodePrime);
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Noise(Circuit ckt)
        {
            var mos1 = ComponentTyped<MOS1>();
            var model = mos1.Model as MOS1Model;
            var state = ckt.State;
            var noise = state.Noise;

            double coxSquared;
            if (model.MOS1oxideCapFactor == 0.0)
                coxSquared = 3.9 * 8.854214871e-12 / 1e-7;
            else
                coxSquared = model.MOS1oxideCapFactor;
            coxSquared *= coxSquared;

            // Set noise parameters
            MOS1noise.Generators[MOS1RDNOIZ].Set(mos1.MOS1drainConductance);
            MOS1noise.Generators[MOS1RSNOIZ].Set(mos1.MOS1sourceConductance);
            MOS1noise.Generators[MOS1IDNOIZ].Set(2.0 / 3.0 * Math.Abs(mos1.MOS1gm));
            MOS1noise.Generators[MOS1FLNOIZ].Set(model.MOS1fNcoef * Math.Exp(model.MOS1fNexp * Math.Log(Math.Max(Math.Abs(mos1.MOS1cd), 1e-38))) / (mos1.MOS1w * (mos1.MOS1l - 2 * model.MOS1latDiff) * coxSquared) / noise.Freq);

            // Evaluate noise sources
            MOS1noise.Evaluate(ckt);
        }
    }
}
