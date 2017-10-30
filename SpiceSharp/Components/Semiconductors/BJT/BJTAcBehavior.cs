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
        public override void Execute(Circuit ckt)
        {
            BJT bjt = ComponentTyped<BJT>();
            BJTModel model = bjt.Model as BJTModel;
            var state = ckt.State;
            var cstate = state;
            double gcpr, gepr, gpi, gmu, go, td, gx;
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

            // cstate.Matrix[bjt.BJTcolNode, bjt.BJTcolNode] += gcpr;
            // cstate.Matrix[bjt.BJTbaseNode, bjt.BJTbaseNode] += gx + xcbx;
            // cstate.Matrix[bjt.BJTemitNode, bjt.BJTemitNode] += gepr;
            // cstate.Matrix[bjt.BJTcolPrimeNode, bjt.BJTcolPrimeNode] += (gmu + go + gcpr) + (xcmu + xccs + xcbx);
            // cstate.Matrix[bjt.BJTbasePrimeNode, bjt.BJTbasePrimeNode] += (gx + gpi + gmu) + (xcpi + xcmu + xcmcb);
            // cstate.Matrix[bjt.BJTemitPrimeNode, bjt.BJTemitPrimeNode] += (gpi + gepr + gm + go) + xcpi;

            // cstate.Matrix[bjt.BJTcolNode, bjt.BJTcolPrimeNode] -= gcpr;
            // cstate.Matrix[bjt.BJTbaseNode, bjt.BJTbasePrimeNode] -= gx;
            // cstate.Matrix[bjt.BJTemitNode, bjt.BJTemitPrimeNode] -= gepr;

            // cstate.Matrix[bjt.BJTcolPrimeNode, bjt.BJTcolNode] -= gcpr;
            // cstate.Matrix[bjt.BJTcolPrimeNode, bjt.BJTbasePrimeNode] += (-gmu + gm) + (-xcmu);
            // cstate.Matrix[bjt.BJTcolPrimeNode, bjt.BJTemitPrimeNode] += (-gm - go);
            // cstate.Matrix[bjt.BJTbasePrimeNode, bjt.BJTbaseNode] -= gx;
            // cstate.Matrix[bjt.BJTbasePrimeNode, bjt.BJTcolPrimeNode] -= gmu + xcmu + xcmcb;
            // cstate.Matrix[bjt.BJTbasePrimeNode, bjt.BJTemitPrimeNode] -= gpi + xcpi;
            // cstate.Matrix[bjt.BJTemitPrimeNode, bjt.BJTemitNode] -= gepr;
            // cstate.Matrix[bjt.BJTemitPrimeNode, bjt.BJTcolPrimeNode] += -go + xcmcb;
            // cstate.Matrix[bjt.BJTemitPrimeNode, bjt.BJTbasePrimeNode] -= (gpi + gm) + (xcpi + xcmcb);

            // cstate.Matrix[bjt.BJTsubstNode, bjt.BJTsubstNode] += xccs;
            // cstate.Matrix[bjt.BJTcolPrimeNode, bjt.BJTsubstNode] -= xccs;
            // cstate.Matrix[bjt.BJTsubstNode, bjt.BJTcolPrimeNode] -= xccs;
            // cstate.Matrix[bjt.BJTbaseNode, bjt.BJTcolPrimeNode] -= xcbx;
            // cstate.Matrix[bjt.BJTcolPrimeNode, bjt.BJTbaseNode] -= xcbx;
        }
    }
}
