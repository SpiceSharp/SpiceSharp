using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// General behaviour for <see cref="Capacitor"/>
    /// </summary>
    public class CapacitorLoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Execute behaviour for DC and Transient analysis
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            Capacitor cap = ComponentTyped<Capacitor>();

            double vcap;
            var state = ckt.State;
            var rstate = state;
            var method = ckt.Method;

            bool cond1 = (state.UseDC && state.Init == Circuits.CircuitState.InitFlags.InitJct) || state.UseIC;

            if (cond1)
                vcap = cap.CAPinitCond;
            else
                vcap = rstate.Solution[cap.CAPposNode] - rstate.Solution[cap.CAPnegNode];

            if (state.Domain == CircuitState.DomainTypes.Time)
            {
                // Fill the matrix
                state.States[0][cap.CAPstate + Capacitor.CAPqcap] = cap.CAPcapac * vcap;
                if (state.Init == CircuitState.InitFlags.InitTransient)
                    state.States[1][cap.CAPstate + Capacitor.CAPqcap] = state.States[0][cap.CAPstate + Capacitor.CAPqcap];

                // Without integration, a capacitor cannot do anything
                if (method != null)
                {
                    var result = ckt.Method.Integrate(state, cap.CAPstate + Capacitor.CAPqcap, cap.CAPcapac);
                    if (state.Init == CircuitState.InitFlags.InitTransient)
                        state.States[1][cap.CAPstate + Capacitor.CAPqcap] = state.States[0][cap.CAPstate + Capacitor.CAPqcap];

                    cap.CAPposPosptr.Add(result.Geq);
                    cap.CAPnegNegptr.Add(result.Geq);
                    cap.CAPposNegptr.Sub(result.Geq);
                    cap.CAPnegPosptr.Sub(result.Geq);
                    state.Rhs[cap.CAPposNode] -= result.Ceq;
                    state.Rhs[cap.CAPnegNode] += result.Ceq;
                }
            }
            else
                state.States[0][cap.CAPstate + Capacitor.CAPqcap] = cap.CAPcapac * vcap;
        }
    }
}
