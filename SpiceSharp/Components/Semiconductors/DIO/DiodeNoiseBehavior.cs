using System;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Noise;

namespace SpiceSharp.Behaviors.DIO
{
    /// <summary>
    /// Noise behaviour for <see cref="Diode"/>
    /// </summary>
    public class NoiseBehavior : CircuitObjectBehaviorNoise
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private LoadBehavior load;
        private ModelNoiseBehavior modelnoise;
        private ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("area"), SpiceInfo("Area factor")]
        public Parameter DIOarea { get; } = new Parameter(1);

        /// <summary>
        /// Noise sources by their index
        /// </summary>
        private const int DIORSNOIZ = 0;
        private const int DIOIDNOIZ = 1;
        private const int DIOFLNOIZ = 2;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise DIOnoise { get; } = new ComponentNoise(
            new NoiseThermal("rs", 0, 1),
            new NoiseShot("id", 1, 2),
            new NoiseGain("1overf", 1, 2));

        /// <summary>
        /// Setup the diode
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var diode = component as Diode;

            // Get behaviors
            load = GetBehavior<LoadBehavior>(diode);
            modeltemp = GetBehavior<ModelTemperatureBehavior>(diode.Model);
            modelnoise = GetBehavior<ModelNoiseBehavior>(diode.Model);

            DIOnoise.Setup(ckt, diode.DIOposNode, load.DIOposPrimeNode, diode.DIOnegNode);
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
            DIOnoise.Generators[DIORSNOIZ].Set(modeltemp.DIOconductance * DIOarea);
            DIOnoise.Generators[DIOIDNOIZ].Set(state.States[0][load.DIOstate + LoadBehavior.DIOcurrent]);
            DIOnoise.Generators[DIOFLNOIZ].Set(modelnoise.DIOfNcoef * Math.Exp(modelnoise.DIOfNexp 
                * Math.Log(Math.Max(Math.Abs(state.States[0][load.DIOstate + LoadBehavior.DIOcurrent]), 1e-38))) / noise.Freq);

            // Evaluate noise
            DIOnoise.Evaluate(ckt);
        }
    }
}
