using System.Numerics;
using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors.BJT
{
    /// <summary>
    /// AC behavior for <see cref="BJT"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private LoadBehavior load;
        private ModelTemperatureBehavior modeltemp;
        
        /// <summary>
        /// Nodes
        /// </summary>
        private int BJTcolNode, BJTbaseNode, BJTemitNode, BJTsubstNode, BJTcolPrimeNode, BJTbasePrimeNode, BJTemitPrimeNode;
        private MatrixElement BJTcolColPrimePtr;
        private MatrixElement BJTbaseBasePrimePtr;
        private MatrixElement BJTemitEmitPrimePtr;
        private MatrixElement BJTcolPrimeColPtr;
        private MatrixElement BJTcolPrimeBasePrimePtr;
        private MatrixElement BJTcolPrimeEmitPrimePtr;
        private MatrixElement BJTbasePrimeBasePtr;
        private MatrixElement BJTbasePrimeColPrimePtr;
        private MatrixElement BJTbasePrimeEmitPrimePtr;
        private MatrixElement BJTemitPrimeEmitPtr;
        private MatrixElement BJTemitPrimeColPrimePtr;
        private MatrixElement BJTemitPrimeBasePrimePtr;
        private MatrixElement BJTcolColPtr;
        private MatrixElement BJTbaseBasePtr;
        private MatrixElement BJTemitEmitPtr;
        private MatrixElement BJTcolPrimeColPrimePtr;
        private MatrixElement BJTbasePrimeBasePrimePtr;
        private MatrixElement BJTemitPrimeEmitPrimePtr;
        private MatrixElement BJTsubstSubstPtr;
        private MatrixElement BJTcolPrimeSubstPtr;
        private MatrixElement BJTsubstColPrimePtr;
        private MatrixElement BJTbaseColPrimePtr;
        private MatrixElement BJTcolPrimeBasePtr;

        /// <summary>
        /// Private variables
        /// </summary>
        private int BJTstate;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(Entity component, Circuit ckt)
        {
            var bjt = component as Components.BJT;

            // Get behaviors
            load = GetBehavior<LoadBehavior>(component);
            modeltemp = GetBehavior<ModelTemperatureBehavior>(bjt.Model);

            // Get connected nodes
            BJTcolNode = bjt.BJTcolNode;
            BJTbaseNode = bjt.BJTbaseNode;
            BJTemitNode = bjt.BJTemitNode;
            BJTsubstNode = bjt.BJTsubstNode;

            BJTcolPrimeNode = load.BJTcolPrimeNode;
            BJTemitPrimeNode = load.BJTemitPrimeNode;
            BJTbasePrimeNode = load.BJTbasePrimeNode;

            BJTstate = load.BJTstate;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            BJTcolColPrimePtr = matrix.GetElement(BJTcolNode, BJTcolPrimeNode);
            BJTbaseBasePrimePtr = matrix.GetElement(BJTbaseNode, BJTbasePrimeNode);
            BJTemitEmitPrimePtr = matrix.GetElement(BJTemitNode, BJTemitPrimeNode);
            BJTcolPrimeColPtr = matrix.GetElement(BJTcolPrimeNode, BJTcolNode);
            BJTcolPrimeBasePrimePtr = matrix.GetElement(BJTcolPrimeNode, BJTbasePrimeNode);
            BJTcolPrimeEmitPrimePtr = matrix.GetElement(BJTcolPrimeNode, BJTemitPrimeNode);
            BJTbasePrimeBasePtr = matrix.GetElement(BJTbasePrimeNode, BJTbaseNode);
            BJTbasePrimeColPrimePtr = matrix.GetElement(BJTbasePrimeNode, BJTcolPrimeNode);
            BJTbasePrimeEmitPrimePtr = matrix.GetElement(BJTbasePrimeNode, BJTemitPrimeNode);
            BJTemitPrimeEmitPtr = matrix.GetElement(BJTemitPrimeNode, BJTemitNode);
            BJTemitPrimeColPrimePtr = matrix.GetElement(BJTemitPrimeNode, BJTcolPrimeNode);
            BJTemitPrimeBasePrimePtr = matrix.GetElement(BJTemitPrimeNode, BJTbasePrimeNode);
            BJTcolColPtr = matrix.GetElement(BJTcolNode, BJTcolNode);
            BJTbaseBasePtr = matrix.GetElement(BJTbaseNode, BJTbaseNode);
            BJTemitEmitPtr = matrix.GetElement(BJTemitNode, BJTemitNode);
            BJTcolPrimeColPrimePtr = matrix.GetElement(BJTcolPrimeNode, BJTcolPrimeNode);
            BJTbasePrimeBasePrimePtr = matrix.GetElement(BJTbasePrimeNode, BJTbasePrimeNode);
            BJTemitPrimeEmitPrimePtr = matrix.GetElement(BJTemitPrimeNode, BJTemitPrimeNode);
            BJTsubstSubstPtr = matrix.GetElement(BJTsubstNode, BJTsubstNode);
            BJTcolPrimeSubstPtr = matrix.GetElement(BJTcolPrimeNode, BJTsubstNode);
            BJTsubstColPrimePtr = matrix.GetElement(BJTsubstNode, BJTcolPrimeNode);
            BJTbaseColPrimePtr = matrix.GetElement(BJTbaseNode, BJTcolPrimeNode);
            BJTcolPrimeBasePtr = matrix.GetElement(BJTcolPrimeNode, BJTbaseNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Unsetup()
        {
            // Remove references
            BJTcolColPrimePtr = null;
            BJTbaseBasePrimePtr = null;
            BJTemitEmitPrimePtr = null;
            BJTcolPrimeColPtr = null;
            BJTcolPrimeBasePrimePtr = null;
            BJTcolPrimeEmitPrimePtr = null;
            BJTbasePrimeBasePtr = null;
            BJTbasePrimeColPrimePtr = null;
            BJTbasePrimeEmitPrimePtr = null;
            BJTemitPrimeEmitPtr = null;
            BJTemitPrimeColPrimePtr = null;
            BJTemitPrimeBasePrimePtr = null;
            BJTcolColPtr = null;
            BJTbaseBasePtr = null;
            BJTemitEmitPtr = null;
            BJTcolPrimeColPrimePtr = null;
            BJTbasePrimeBasePrimePtr = null;
            BJTemitPrimeEmitPrimePtr = null;
            BJTsubstSubstPtr = null;
            BJTcolPrimeSubstPtr = null;
            BJTsubstColPrimePtr = null;
            BJTbaseColPrimePtr = null;
            BJTcolPrimeBasePtr = null;
        }

        /// <summary>
        /// Execute AC behavior
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var cstate = state;
            double gcpr, gepr, gpi, gmu, go, td, gx, xgm;
            Complex gm, xcpi, xcmu, xcbx, xccs, xcmcb;

            gcpr = modeltemp.BJTcollectorConduct * load.BJTarea;
            gepr = modeltemp.BJTemitterConduct * load.BJTarea;
            gpi = state.States[0][BJTstate + LoadBehavior.BJTgpi];
            gmu = state.States[0][BJTstate + LoadBehavior.BJTgmu];
            gm = state.States[0][BJTstate + LoadBehavior.BJTgm];
            go = state.States[0][BJTstate + LoadBehavior.BJTgo];
            td = modeltemp.BJTexcessPhaseFactor;
            if (td != 0)
            {
                Complex arg = td * cstate.Laplace;

                gm = gm + go;
                gm = gm * Complex.Exp(-arg);
                gm = gm - go;
            }
            gx = state.States[0][BJTstate + LoadBehavior.BJTgx];
            xcpi = state.States[0][BJTstate + LoadBehavior.BJTcqbe] * cstate.Laplace;
            xcmu = state.States[0][BJTstate + LoadBehavior.BJTcqbc] * cstate.Laplace;
            xcbx = state.States[0][BJTstate + LoadBehavior.BJTcqbx] * cstate.Laplace;
            xccs = state.States[0][BJTstate + LoadBehavior.BJTcqcs] * cstate.Laplace;
            xcmcb = state.States[0][BJTstate + LoadBehavior.BJTcexbc] * cstate.Laplace;

            BJTcolColPtr.Add(gcpr);
            BJTbaseBasePtr.Add(gx + xcbx);
            BJTemitEmitPtr.Add(gepr);
            BJTcolPrimeColPrimePtr.Add(gmu + go + gcpr + xcmu + xccs + xcbx);
            BJTbasePrimeBasePrimePtr.Add(gx + gpi + gmu + xcpi + xcmu + xcmcb);
            BJTemitPrimeEmitPrimePtr.Add(gpi + gepr + gm + go + xcpi);
            BJTcolColPrimePtr.Add(-gcpr);
            BJTbaseBasePrimePtr.Add(-gx);
            BJTemitEmitPrimePtr.Add(-gepr);
            BJTcolPrimeColPtr.Add(-gcpr);
            BJTcolPrimeBasePrimePtr.Add(-gmu + gm - xcmu);
            BJTcolPrimeEmitPrimePtr.Add(-gm - go);
            BJTbasePrimeBasePtr.Add(-gx);
            BJTbasePrimeColPrimePtr.Add(-gmu - xcmu - xcmcb);
            BJTbasePrimeEmitPrimePtr.Add(-gpi - xcpi);
            BJTemitPrimeEmitPtr.Add(-gepr);
            BJTemitPrimeColPrimePtr.Add(-go + xcmcb);
            BJTemitPrimeBasePrimePtr.Add(-gpi - gm - xcpi - xcmcb);
            BJTsubstSubstPtr.Add(xccs);
            BJTcolPrimeSubstPtr.Add(-xccs);
            BJTsubstColPrimePtr.Add(-xccs);
            BJTbaseColPrimePtr.Add(-xcbx);
            BJTcolPrimeBasePtr.Add(-xcbx);
        }
    }
}
