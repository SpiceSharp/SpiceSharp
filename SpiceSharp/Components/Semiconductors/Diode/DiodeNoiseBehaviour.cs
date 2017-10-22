using System;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviours;
using SpiceSharp.Components.Noise;

namespace SpiceSharp.Components.ComponentBehaviours
{
    /// <summary>
    /// Behaviour of a diode
    /// </summary>
    public class DiodeNoiseBehaviour : CircuitObjectBehaviourNoise
    {
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
        public override void Setup(ICircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var diode = ComponentTyped<Diode>();
            DIOnoise.Setup(ckt, diode.DIOposNode, diode.DIOposPrimeNode, diode.DIOnegNode);
        }

        /// <summary>
        /// Perform diode noise calculations
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var diode = ComponentTyped<Diode>();

            var model = diode.Model as DiodeModel;
            var state = ckt.State;
            var noise = ckt.State.Noise;

            double Kf = model.DIOfNcoef * Math.Exp(model.DIOfNexp * Math.Log(Math.Max(Math.Abs(state.States[0][diode.DIOstate + Diode.DIOcurrent]), 1e-38)));

            DIOnoise.Evaluate(ckt,
                model.DIOconductance * diode.DIOarea, // Thermal noise
                state.States[0][diode.DIOstate + Diode.DIOcurrent], // Shot noise
                Kf / noise.Freq); // 1 over f noise
        }
    }
}
