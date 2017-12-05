using System.Numerics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="Diode"/>
    /// </summary>
    public class DiodeAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Perform AC analysis
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var diode = ComponentTyped<Diode>();

            var model = diode.Model as DiodeModel;
            var state = ckt.State;
            double gspr, geq, xceq;

            gspr = model.DIOconductance * diode.DIOarea;
            geq = state.States[0][diode.DIOstate + Diode.DIOconduct];
            xceq = state.States[0][diode.DIOstate + Diode.DIOcapCurrent] * state.Laplace.Imaginary;

            diode.DIOposPosPtr.Value.Real += gspr;
            diode.DIOnegNegPtr.Value.Cplx += new Complex(geq, xceq);

            diode.DIOposPrimePosPrimePtr.Value.Cplx += new Complex(geq + gspr, xceq);

            diode.DIOposPosPrimePtr.Value.Real -= gspr;
            diode.DIOnegPosPrimePtr.Value.Cplx -= new Complex(geq, xceq);

            diode.DIOposPrimePosPtr.Value.Real -= gspr;
            diode.DIOposPrimeNegPtr.Value.Cplx -= new Complex(geq, xceq);
        }
    }
}
