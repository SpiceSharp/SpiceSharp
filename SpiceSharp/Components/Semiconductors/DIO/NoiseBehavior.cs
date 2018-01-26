using System;
using SpiceSharp.Components.NoiseSources;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Noise behavior for <see cref="Diode"/>
    /// </summary>
    public class NoiseBehavior : Behaviors.NoiseBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        LoadBehavior load;
        BaseParameters bp;
        ModelNoiseParameters mnp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Nodes
        /// </summary>
        int DIOposNode, DIOnegNode, DIOposPrimeNode;

        /// <summary>
        /// Noise sources by their index
        /// </summary>
        const int DIORSNOIZ = 0;
        const int DIOIDNOIZ = 1;
        const int DIOFLNOIZ = 2;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise DIOnoise { get; } = new ComponentNoise(
            new NoiseThermal("rs", 0, 1),
            new NoiseShot("id", 1, 2),
            new NoiseGain("1overf", 1, 2));

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
            mnp = provider.GetParameterSet<ModelNoiseParameters>(1);

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>(0);
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Connect the noise source
        /// </summary>
        public override void ConnectNoise()
        {
            // Get extra equations
            DIOposPrimeNode = load.DIOposPrimeNode;

            // Connect noise sources
            DIOnoise.Setup(DIOposNode, DIOposPrimeNode, DIOnegNode);
        }
        
        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new Diagnostics.CircuitException($"Pin count mismatch: 2 pins expected, {pins.Length} given");
            DIOposNode = pins[0];
            DIOnegNode = pins[1];
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
            DIOnoise.Generators[DIORSNOIZ].Set(modeltemp.DIOconductance * bp.DIOarea);
            DIOnoise.Generators[DIOIDNOIZ].Set(load.DIOcurrent);
            DIOnoise.Generators[DIOFLNOIZ].Set(mnp.DIOfNcoef * Math.Exp(mnp.DIOfNexp 
                * Math.Log(Math.Max(Math.Abs(load.DIOcurrent), 1e-38))) / noise.Freq);

            // Evaluate noise
            DIOnoise.Evaluate(sim);
        }
    }
}
