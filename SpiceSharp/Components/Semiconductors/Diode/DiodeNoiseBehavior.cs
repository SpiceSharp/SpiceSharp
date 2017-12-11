using System;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Parameters;
using SpiceSharp.Components.Noise;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Noise behaviour for <see cref="Diode"/>
    /// </summary>
    public class DiodeNoiseBehavior : CircuitObjectBehaviorNoise
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private DiodeModelNoiseBehavior modelnoise;
        private DiodeModelTemperatureBehavior modeltemp;

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
        /// Private variables
        /// </summary>
        private int DIOstate;

        /// <summary>
        /// Setup the diode
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            var diode = component as Diode;
            var model = diode.Model as DiodeModel;

            // Get behaviors
            modeltemp = model.GetBehavior(typeof(CircuitObjectBehaviorTemperature)) as DiodeModelTemperatureBehavior;
            modelnoise = model.GetBehavior(typeof(CircuitObjectBehaviorNoise)) as DiodeModelNoiseBehavior;
            var load = diode.GetBehavior(typeof(CircuitObjectBehaviorLoad)) as DiodeLoadBehavior;
            DIOstate = load.DIOstate;

            DIOnoise.Setup(ckt, diode.DIOposNode, load.DIOposPrimeNode, diode.DIOnegNode);
            return true;
        }

        /// <summary>
        /// Perform diode noise calculations
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Noise(Circuit ckt)
        {
            var diode = ComponentTyped<Diode>();

            var model = diode.Model as DiodeModel;
            var state = ckt.State;
            var noise = ckt.State.Noise;

            // Set noise parameters
            DIOnoise.Generators[DIORSNOIZ].Set(modeltemp.DIOconductance * DIOarea);
            DIOnoise.Generators[DIOIDNOIZ].Set(state.States[0][DIOstate + DiodeLoadBehavior.DIOcurrent]);
            DIOnoise.Generators[DIOFLNOIZ].Set(modelnoise.DIOfNcoef * Math.Exp(modelnoise.DIOfNexp 
                * Math.Log(Math.Max(Math.Abs(state.States[0][DIOstate + DiodeLoadBehavior.DIOcurrent]), 1e-38))) / noise.Freq);

            // Evaluate noise
            DIOnoise.Evaluate(ckt);
        }
    }
}
