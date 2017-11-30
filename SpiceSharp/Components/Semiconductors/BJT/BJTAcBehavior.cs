using System.Numerics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="BJT"/>
    /// </summary>
    public class BJTAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Execute AC behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            BJT bjt = ComponentTyped<BJT>();
            BJTModel model = bjt.Model as BJTModel;
            var state = ckt.State;
            var cstate = state;
            double gcpr, gepr, gpi, gmu, go, td, gx, xgm;
            Complex gm, xcpi, xcmu, xcbx, xccs, xcmcb;

            gcpr = model.BJTcollectorConduct * bjt.BJTarea;
            gepr = model.BJTemitterConduct * bjt.BJTarea;
            gpi = state.States[0][bjt.BJTstate + BJT.BJTgpi];
            gmu = state.States[0][bjt.BJTstate + BJT.BJTgmu];
            gm = state.States[0][bjt.BJTstate + BJT.BJTgm];
            go = state.States[0][bjt.BJTstate + BJT.BJTgo];
            td = model.BJTexcessPhaseFactor;
            if (td != 0)
            {
                Complex arg = td * cstate.Laplace;

                gm = gm + go;
                gm = gm * Complex.Exp(-arg);
                gm = gm - go;
            }
            gx = state.States[0][bjt.BJTstate + BJT.BJTgx];
            xcpi = state.States[0][bjt.BJTstate + BJT.BJTcqbe] * cstate.Laplace;
            xcmu = state.States[0][bjt.BJTstate + BJT.BJTcqbc] * cstate.Laplace;
            xcbx = state.States[0][bjt.BJTstate + BJT.BJTcqbx] * cstate.Laplace;
            xccs = state.States[0][bjt.BJTstate + BJT.BJTcqcs] * cstate.Laplace;
            xcmcb = state.States[0][bjt.BJTstate + BJT.BJTcexbc] * cstate.Laplace;

            bjt.BJTcolColPtr.Add(gcpr);
            bjt.BJTbaseBasePtr.Add(gx + xcbx);
            bjt.BJTemitEmitPtr.Add(gepr);
            bjt.BJTcolPrimeColPrimePtr.Add(gmu + go + gcpr + xcmu + xccs + xcbx);
            bjt.BJTbasePrimeBasePrimePtr.Add(gx + gpi + gmu + xcpi + xcmu + xcmcb);
            bjt.BJTemitPrimeEmitPrimePtr.Add(gpi + gepr + gm + go + xcpi);
            bjt.BJTcolColPrimePtr.Add(-gcpr);
            bjt.BJTbaseBasePrimePtr.Add(-gx);
            bjt.BJTemitEmitPrimePtr.Add(-gepr);
            bjt.BJTcolPrimeColPtr.Add(-gcpr);
            bjt.BJTcolPrimeBasePrimePtr.Add(-gmu + gm - xcmu);
            bjt.BJTcolPrimeEmitPrimePtr.Add(-gm - go);
            bjt.BJTbasePrimeBasePtr.Add(-gx);
            bjt.BJTbasePrimeColPrimePtr.Add(-gmu - xcmu - xcmcb);
            bjt.BJTbasePrimeEmitPrimePtr.Add(-gpi - xcpi);
            bjt.BJTemitPrimeEmitPtr.Add(-gepr);
            bjt.BJTemitPrimeColPrimePtr.Add(-go + xcmcb);
            bjt.BJTemitPrimeBasePrimePtr.Add(-gpi - gm - xcpi - xcmcb);
            bjt.BJTsubstSubstPtr.Add(xccs);
            bjt.BJTcolPrimeSubstPtr.Add(-xccs);
            bjt.BJTsubstColPrimePtr.Add(-xccs);
            bjt.BJTbaseColPrimePtr.Add(-xcbx);
            bjt.BJTcolPrimeBasePtr.Add(-xcbx);
        }
    }
}
