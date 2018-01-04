using System;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Components.Noise;
using SpiceSharp.Components.DIO;

namespace SpiceSharp.Behaviors.DIO
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

        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();
            mnp = provider.GetParameters<ModelNoiseParameters>(1);

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>();
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }
        
        /// <summary>
        /// Connect the noise source
        /// </summary>
        public override void ConnectNoise() => DIOnoise.Setup(DIOposNode, DIOposPrimeNode, DIOnegNode);
        
        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            DIOposNode = pins[0];
            DIOnegNode = pins[1];
        }

        /// <summary>
        /// Perform diode noise calculations
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Noise(Circuit ckt)
        {
            var state = ckt.State;
            var noise = ckt.State.Noise;

            // Set noise parameters
            DIOnoise.Generators[DIORSNOIZ].Set(modeltemp.DIOconductance * bp.DIOarea);
            DIOnoise.Generators[DIOIDNOIZ].Set(load.DIOcurrent);
            DIOnoise.Generators[DIOFLNOIZ].Set(mnp.DIOfNcoef * Math.Exp(mnp.DIOfNexp 
                * Math.Log(Math.Max(Math.Abs(load.DIOcurrent), 1e-38))) / noise.Freq);

            // Evaluate noise
            DIOnoise.Evaluate(ckt);
        }
    }
}
