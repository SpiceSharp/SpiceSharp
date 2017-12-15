using System;
using SpiceSharp.Components;
using SpiceSharp.Components.Noise;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.MOS3
{
    /// <summary>
    /// Noise behaviour for <see cref="Components.MOS3"/>
    /// </summary>
    public class NoiseBehavior : Behaviors.NoiseBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private LoadBehavior load;
        private TemperatureBehavior temp;
        private ModelTemperatureBehavior modeltemp;
        private ModelNoiseBehavior modelnoise;

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
            new NoiseThermal("rd", 0, 4),
            new NoiseThermal("rs", 2, 5),
            new NoiseThermal("id", 4, 5),
            new NoiseGain("1overf", 4, 5)
            );

        /// <summary>
        /// Setup behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(Entity component, Circuit ckt)
        {
            var mos3 = component as Components.MOS3;

            // Get behaviors
            load = GetBehavior<LoadBehavior>(component);
            temp = GetBehavior<TemperatureBehavior>(component);
            modeltemp = GetBehavior<ModelTemperatureBehavior>(mos3.Model);
            modelnoise = GetBehavior<ModelNoiseBehavior>(mos3.Model);

            MOS3noise.Setup(ckt,
                mos3.MOS3dNode,
                mos3.MOS3gNode,
                mos3.MOS3sNode,
                mos3.MOS3bNode,
                load.MOS3dNodePrime,
                load.MOS3sNodePrime);
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Noise(Circuit ckt)
        {
            var state = ckt.State;
            var noise = state.Noise;

            // Set noise parameters
            MOS3noise.Generators[MOS3RDNOIZ].Set(temp.MOS3drainConductance);
            MOS3noise.Generators[MOS3RSNOIZ].Set(temp.MOS3sourceConductance);
            MOS3noise.Generators[MOS3IDNOIZ].Set(2.0 / 3.0 * Math.Abs(load.MOS3gm));
            MOS3noise.Generators[MOS3FLNOIZ].Set(modelnoise.MOS3fNcoef * Math.Exp(modelnoise.MOS3fNexp 
                * Math.Log(Math.Max(Math.Abs(load.MOS3cd), 1e-38))) / (temp.MOS3w * (temp.MOS3l - 2 * modeltemp.MOS3latDiff) 
                * modeltemp.MOS3oxideCapFactor * modeltemp.MOS3oxideCapFactor) / noise.Freq);

            // Evaluate noise sources
            MOS3noise.Evaluate(ckt);
        }
    }
}
