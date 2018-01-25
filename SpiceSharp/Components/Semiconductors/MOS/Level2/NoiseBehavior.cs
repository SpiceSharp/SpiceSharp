using System;
using SpiceSharp.Components;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Circuits;
using SpiceSharp.Components.Mosfet.Level2;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.Mosfet.Level2
{
    /// <summary>
    /// Noise behavior for <see cref="Components.MOS2"/>
    /// </summary>
    public class NoiseBehavior : Behaviors.NoiseBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        ModelNoiseParameters mnp;
        LoadBehavior load;
        TemperatureBehavior temp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Noise generators by their index
        /// </summary>
        const int MOS2RDNOIZ = 0;
        const int MOS2RSNOIZ = 1;
        const int MOS2IDNOIZ = 2;
        const int MOS2FLNOIZ = 3;

        /// <summary>
        /// Nodes
        /// </summary>
        int MOS2dNode, MOS2gNode, MOS2sNode, MOS2bNode, MOS2sNodePrime, MOS2dNodePrime;

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
            bp = provider.GetParameterSet<BaseParameters>(0);
            mbp = provider.GetParameterSet<ModelBaseParameters>(1);
            mnp = provider.GetParameterSet<ModelNoiseParameters>(1);

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>(0);
            load = provider.GetBehavior<LoadBehavior>(0);
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            MOS2dNode = pins[0];
            MOS2gNode = pins[1];
            MOS2sNode = pins[2];
            MOS2bNode = pins[3];
        }

        /// <summary>
        /// Connect noise sources
        /// </summary>
        public override void ConnectNoise()
        {
            // Get extra equations
            MOS2dNodePrime = load.MOS2dNodePrime;
            MOS2sNodePrime = load.MOS2sNodePrime;

            // Connect noise source
            MOS2noise.Setup(MOS2dNode, MOS2gNode, MOS2sNode, MOS2bNode, MOS2dNodePrime, MOS2sNodePrime);
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        /// <param name="sim">Noise simulation</param>
        public override void Noise(Noise sim)
        {
            var noise = sim.NoiseState;

            // Set noise parameters
            MOS2noise.Generators[MOS2RDNOIZ].Set(temp.MOS2drainConductance);
            MOS2noise.Generators[MOS2RSNOIZ].Set(temp.MOS2sourceConductance);
            MOS2noise.Generators[MOS2IDNOIZ].Set(2.0 / 3.0 * Math.Abs(load.MOS2gm));
            MOS2noise.Generators[MOS2FLNOIZ].Set(mnp.MOS2fNcoef * Math.Exp(mnp.MOS2fNexp * Math.Log(Math.Max(Math.Abs(load.MOS2cd), 1e-38))) 
                / (bp.MOS2w * (bp.MOS2l - 2 * mbp.MOS2latDiff) * modeltemp.MOS2oxideCapFactor * modeltemp.MOS2oxideCapFactor) / noise.Freq);

            // Evaluate noise sources
            MOS2noise.Evaluate(sim);
        }
    }
}
