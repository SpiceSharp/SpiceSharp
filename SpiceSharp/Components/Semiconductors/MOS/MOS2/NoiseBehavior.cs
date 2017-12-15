using System;
using SpiceSharp.Components;
using SpiceSharp.Components.Noise;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.MOS2
{
    /// <summary>
    /// Noise behaviour for <see cref="Components.MOS2"/>
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
        private const int MOS2RDNOIZ = 0;
        private const int MOS2RSNOIZ = 1;
        private const int MOS2IDNOIZ = 2;
        private const int MOS2FLNOIZ = 3;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise MOS2noise { get; } = new ComponentNoise(
            new NoiseThermal("rd", 0, 4),
            new NoiseThermal("rs", 2, 5),
            new NoiseThermal("id", 4, 5),
            new NoiseGain("1overf", 4, 5)
            );

        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var mos2 = component as Components.MOS2;

            // Get behaviors
            load = GetBehavior<LoadBehavior>(component);
            temp = GetBehavior<TemperatureBehavior>(component);
            modeltemp = GetBehavior<ModelTemperatureBehavior>(mos2.Model);
            modelnoise = GetBehavior<ModelNoiseBehavior>(mos2.Model);

            MOS2noise.Setup(ckt,
                mos2.MOS2dNode,
                mos2.MOS2gNode,
                mos2.MOS2sNode,
                mos2.MOS2bNode,
                load.MOS2dNodePrime,
                load.MOS2sNodePrime);
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Noise(Circuit ckt)
        {
            var state = ckt.State;
            var noise = state.Noise;

            // Set noise parameters
            MOS2noise.Generators[MOS2RDNOIZ].Set(temp.MOS2drainConductance);
            MOS2noise.Generators[MOS2RSNOIZ].Set(temp.MOS2sourceConductance);
            MOS2noise.Generators[MOS2IDNOIZ].Set(2.0 / 3.0 * Math.Abs(load.MOS2gm));
            MOS2noise.Generators[MOS2FLNOIZ].Set(modelnoise.MOS2fNcoef * Math.Exp(modelnoise.MOS2fNexp * Math.Log(Math.Max(Math.Abs(load.MOS2cd), 1e-38))) 
                / (temp.MOS2w * (temp.MOS2l - 2 * modeltemp.MOS2latDiff) * modeltemp.MOS2oxideCapFactor * modeltemp.MOS2oxideCapFactor) / noise.Freq);

            // Evaluate noise sources
            MOS2noise.Evaluate(ckt);
        }
    }
}
