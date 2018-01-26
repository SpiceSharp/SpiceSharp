using System;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
{
    /// <summary>
    /// Noise behavior for <see cref="MOS2"/>
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
        const int RdNoise = 0;
        const int RsNoise = 1;
        const int IdNoise = 2;
        const int FlickerNoise = 3;

        /// <summary>
        /// Nodes
        /// </summary>
        int dNode, gNode, sNode, bNode, sNodePrime, dNodePrime;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise MOS2Noise { get; } = new ComponentNoise(
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
            dNode = pins[0];
            gNode = pins[1];
            sNode = pins[2];
            bNode = pins[3];
        }

        /// <summary>
        /// Connect noise sources
        /// </summary>
        public override void ConnectNoise()
        {
            // Get extra equations
            dNodePrime = load.DrainNodePrime;
            sNodePrime = load.SourceNodePrime;

            // Connect noise source
            MOS2Noise.Setup(dNode, gNode, sNode, bNode, dNodePrime, sNodePrime);
        }

        /// <summary>
        /// Noise calculations
        /// </summary>
        /// <param name="simulation">Noise simulation</param>
        public override void Noise(Noise simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var noise = simulation.NoiseState;

            // Set noise parameters
            MOS2Noise.Generators[RdNoise].Set(temp.DrainConductance);
            MOS2Noise.Generators[RsNoise].Set(temp.SourceConductance);
            MOS2Noise.Generators[IdNoise].Set(2.0 / 3.0 * Math.Abs(load.Gm));
            MOS2Noise.Generators[FlickerNoise].Set(mnp.FNcoef * Math.Exp(mnp.FNexp * Math.Log(Math.Max(Math.Abs(load.Cd), 1e-38))) 
                / (bp.Width * (bp.Length - 2 * mbp.LatDiff) * modeltemp.OxideCapFactor * modeltemp.OxideCapFactor) / noise.Freq);

            // Evaluate noise sources
            MOS2Noise.Evaluate(simulation);
        }
    }
}
