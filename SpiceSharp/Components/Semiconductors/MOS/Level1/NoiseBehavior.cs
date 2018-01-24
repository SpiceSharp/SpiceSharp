using System;
using SpiceSharp.Components.Mosfet.Level1;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.Mosfet.Level1
{
    /// <summary>
    /// Noise behavior for a <see cref="MOS1"/>
    /// </summary>
    public class NoiseBehavior : Behaviors.NoiseBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        TemperatureBehavior temp;
        LoadBehavior load;
        ModelTemperatureBehavior modeltemp;
        ModelNoiseParameters mnp;

        /// <summary>
        /// Nodes
        /// </summary>
        int MOS1dNode, MOS1gNode, MOS1sNode, MOS1bNode, MOS1sNodePrime, MOS1dNodePrime;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise MOS1noise { get; } = new ComponentNoise(
            new NoiseThermal("rd", 0, 4),
            new NoiseThermal("rs", 2, 5),
            new NoiseThermal("id", 4, 5),
            new NoiseGain("1overf", 4, 5)
            );
        const int MOS1RDNOIZ = 0;
        const int MOS1RSNOIZ = 1;
        const int MOS1IDNOIZ = 2;
        const int MOS1FLNOIZ = 3;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public NoiseBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();
            mbp = provider.GetParameters<ModelBaseParameters>(1);
            mnp = provider.GetParameters<ModelNoiseParameters>(1);

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>();
            load = provider.GetBehavior<LoadBehavior>();
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            MOS1dNode = pins[0];
            MOS1gNode = pins[1];
            MOS1sNode = pins[2];
            MOS1bNode = pins[3];
        }

        /// <summary>
        /// Connect the noise sources
        /// </summary>
        public override void ConnectNoise()
        {
            // Get extra equations
            MOS1dNodePrime = load.MOS1dNodePrime;
            MOS1sNodePrime = load.MOS1sNodePrime;

            // Connect noise
            MOS1noise.Setup(MOS1dNode, MOS1gNode, MOS1sNode, MOS1bNode,
                MOS1dNodePrime, MOS1sNodePrime);
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        /// <param name="sim">Noise simulation</param>
        public override void Noise(Noise sim)
        {
            var state = sim.State;
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
            MOS1noise.Generators[MOS1FLNOIZ].Set(mnp.MOS1fNcoef * Math.Exp(mnp.MOS1fNexp * Math.Log(Math.Max(Math.Abs(load.MOS1cd), 1e-38))) 
                / (bp.MOS1w * (bp.MOS1l - 2 * mbp.MOS1latDiff) * coxSquared) / noise.Freq);

            // Evaluate noise sources
            MOS1noise.Evaluate(sim);
        }
    }
}
