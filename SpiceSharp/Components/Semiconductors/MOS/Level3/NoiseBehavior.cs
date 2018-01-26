using System;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// Noise behavior for <see cref="MOS3"/>
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
        /// Nodes
        /// </summary>
        int MOS3dNode, MOS3gNode, MOS3sNode, MOS3bNode, MOS3dNodePrime, MOS3sNodePrime;

        /// <summary>
        /// Noise generators by their index
        /// </summary>
        const int MOS3RDNOIZ = 0;
        const int MOS3RSNOIZ = 1;
        const int MOS3IDNOIZ = 2;
        const int MOS3FLNOIZ = 3;

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
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

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
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 4)
                throw new Diagnostics.CircuitException($"Pin count mismatch: 4 pins expected, {pins.Length} given");
            MOS3dNode = pins[0];
            MOS3gNode = pins[1];
            MOS3sNode = pins[2];
            MOS3bNode = pins[3];
        }

        /// <summary>
        /// Connect noise
        /// </summary>
        public override void ConnectNoise()
        {
            // Get extra equations
            MOS3dNodePrime = load.MOS3dNodePrime;
            MOS3sNodePrime = load.MOS3sNodePrime;

            // Connect noise sources
            MOS3noise.Setup(MOS3dNode, MOS3gNode, MOS3sNode, MOS3bNode, MOS3dNodePrime, MOS3sNodePrime);
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        /// <param name="sim">Noise simulation</param>
        public override void Noise(Noise sim)
        {
            if (sim == null)
                throw new ArgumentNullException(nameof(sim));

            var state = sim.State;
            var noise = sim.NoiseState;

            // Set noise parameters
            MOS3noise.Generators[MOS3RDNOIZ].Set(temp.MOS3drainConductance);
            MOS3noise.Generators[MOS3RSNOIZ].Set(temp.MOS3sourceConductance);
            MOS3noise.Generators[MOS3IDNOIZ].Set(2.0 / 3.0 * Math.Abs(load.MOS3gm));
            MOS3noise.Generators[MOS3FLNOIZ].Set(mnp.MOS3fNcoef * Math.Exp(mnp.MOS3fNexp 
                * Math.Log(Math.Max(Math.Abs(load.MOS3cd), 1e-38))) / (bp.MOS3w * (bp.MOS3l - 2 * mbp.MOS3latDiff) 
                * modeltemp.MOS3oxideCapFactor * modeltemp.MOS3oxideCapFactor) / noise.Freq);

            // Evaluate noise sources
            MOS3noise.Evaluate(sim);
        }
    }
}
