using System;
using SpiceSharp.Components;
using SpiceSharp.Components.Noise;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.MOS1
{
    /// <summary>
    /// Noise behavior for a <see cref="MOS1"/>
    /// </summary>
    public class NoiseBehavior : Behaviors.NoiseBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private TemperatureBehavior temp;
        private LoadBehavior load;
        private ModelTemperatureBehavior modeltemp;
        private ModelNoiseBehavior modelnoise;

        private const int MOS1RDNOIZ = 0;
        private const int MOS1RSNOIZ = 1;
        private const int MOS1IDNOIZ = 2;
        private const int MOS1FLNOIZ = 3;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise MOS1noise { get; } = new ComponentNoise(
            new NoiseThermal("rd", 0, 4),
            new NoiseThermal("rs", 2, 5),
            new NoiseThermal("id", 4, 5),
            new NoiseGain("1overf", 4, 5)
            );

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(Entity component, Circuit ckt)
        {
            var mos1 = component as Components.MOS1;

            // Get behaviors
            temp = GetBehavior<TemperatureBehavior>(component);
            load = GetBehavior<LoadBehavior>(component);
            modeltemp = GetBehavior<ModelTemperatureBehavior>(mos1.Model);
            modelnoise = GetBehavior<ModelNoiseBehavior>(mos1.Model);

            MOS1noise.Setup(ckt,
                mos1.MOS1dNode,
                mos1.MOS1gNode,
                mos1.MOS1sNode,
                mos1.MOS1bNode,
                load.MOS1dNodePrime,
                load.MOS1sNodePrime);
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Noise(Circuit ckt)
        {
            var state = ckt.State;
            var noise = state.Noise;

            double coxSquared;
            if (modeltemp.MOS1oxideCapFactor == 0.0)
                coxSquared = 3.9 * 8.854214871e-12 / 1e-7;
            else
                coxSquared = modeltemp.MOS1oxideCapFactor;
            coxSquared *= coxSquared;

            // Set noise parameters
            MOS1noise.Generators[MOS1RDNOIZ].Set(temp.MOS1drainConductance);
            MOS1noise.Generators[MOS1RSNOIZ].Set(temp.MOS1sourceConductance);
            MOS1noise.Generators[MOS1IDNOIZ].Set(2.0 / 3.0 * Math.Abs(load.MOS1gm));
            MOS1noise.Generators[MOS1FLNOIZ].Set(modelnoise.MOS1fNcoef * Math.Exp(modelnoise.MOS1fNexp * Math.Log(Math.Max(Math.Abs(load.MOS1cd), 1e-38))) 
                / (temp.MOS1w * (temp.MOS1l - 2 * modeltemp.MOS1latDiff) * coxSquared) / noise.Freq);

            // Evaluate noise sources
            MOS1noise.Evaluate(ckt);
        }
    }
}
