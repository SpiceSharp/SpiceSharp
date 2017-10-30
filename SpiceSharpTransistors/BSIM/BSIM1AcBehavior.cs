using System.Numerics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for a <see cref="BSIM1"/>
    /// </summary>
    public class BSIM1AcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Execute(Circuit ckt)
        {
            var bsim1 = ComponentTyped<BSIM1>();
            var state = ckt.State;
            var cstate = state.Complex;
            int xnrm;
            int xrev;
            double gdpr, gspr, gm, gds, gmbs, gbd, gbs, capbd, capbs, cggb, cgsb, cgdb, cbgb, cbsb, cbdb, cdgb, cdsb, cddb;
            Complex xcdgb, xcddb, xcdsb, xcsgb, xcsdb, xcssb, xcggb, xcgdb, xcgsb, xcbgb, xcbdb, xcbsb;

            if (bsim1.B1mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
            }
            else
            {
                xnrm = 0;
                xrev = 1;
            }
            gdpr = bsim1.B1drainConductance;
            gspr = bsim1.B1sourceConductance;
            gm = state.States[0][bsim1.B1states + BSIM1.B1gm];
            gds = state.States[0][bsim1.B1states + BSIM1.B1gds];
            gmbs = state.States[0][bsim1.B1states + BSIM1.B1gmbs];
            gbd = state.States[0][bsim1.B1states + BSIM1.B1gbd];
            gbs = state.States[0][bsim1.B1states + BSIM1.B1gbs];
            capbd = state.States[0][bsim1.B1states + BSIM1.B1capbd];
            capbs = state.States[0][bsim1.B1states + BSIM1.B1capbs];
            /* 
			* charge oriented model parameters
			*/

            cggb = state.States[0][bsim1.B1states + BSIM1.B1cggb];
            cgsb = state.States[0][bsim1.B1states + BSIM1.B1cgsb];
            cgdb = state.States[0][bsim1.B1states + BSIM1.B1cgdb];

            cbgb = state.States[0][bsim1.B1states + BSIM1.B1cbgb];
            cbsb = state.States[0][bsim1.B1states + BSIM1.B1cbsb];
            cbdb = state.States[0][bsim1.B1states + BSIM1.B1cbdb];

            cdgb = state.States[0][bsim1.B1states + BSIM1.B1cdgb];
            cdsb = state.States[0][bsim1.B1states + BSIM1.B1cdsb];
            cddb = state.States[0][bsim1.B1states + BSIM1.B1cddb];

            xcdgb = (cdgb - bsim1.B1GDoverlapCap) * cstate.Laplace;
            xcddb = (cddb + capbd + bsim1.B1GDoverlapCap) * cstate.Laplace;
            xcdsb = cdsb * cstate.Laplace;
            xcsgb = -(cggb + cbgb + cdgb + bsim1.B1GSoverlapCap) * cstate.Laplace;
            xcsdb = -(cgdb + cbdb + cddb) * cstate.Laplace;
            xcssb = (capbs + bsim1.B1GSoverlapCap - (cgsb + cbsb + cdsb)) * cstate.Laplace;
            xcggb = (cggb + bsim1.B1GDoverlapCap + bsim1.B1GSoverlapCap + bsim1.B1GBoverlapCap) * cstate.Laplace;
            xcgdb = (cgdb - bsim1.B1GDoverlapCap) * cstate.Laplace;
            xcgsb = (cgsb - bsim1.B1GSoverlapCap) * cstate.Laplace;
            xcbgb = (cbgb - bsim1.B1GBoverlapCap) * cstate.Laplace;
            xcbdb = (cbdb - capbd) * cstate.Laplace;
            xcbsb = (cbsb - capbs) * cstate.Laplace;

            // cstate.Matrix[bsim1.B1gNode, bsim1.B1gNode] += xcggb;
            // cstate.Matrix[bsim1.B1bNode, bsim1.B1bNode] += -xcbgb - xcbdb - xcbsb;
            // cstate.Matrix[bsim1.B1dNodePrime, bsim1.B1dNodePrime] += xcddb;
            // cstate.Matrix[bsim1.B1sNodePrime, bsim1.B1sNodePrime] += xcssb;
            // cstate.Matrix[bsim1.B1gNode, bsim1.B1bNode] += -xcggb - xcgdb - xcgsb;
            // cstate.Matrix[bsim1.B1gNode, bsim1.B1dNodePrime] += xcgdb;
            // cstate.Matrix[bsim1.B1gNode, bsim1.B1sNodePrime] += xcgsb;
            // cstate.Matrix[bsim1.B1bNode, bsim1.B1gNode] += xcbgb;
            // cstate.Matrix[bsim1.B1bNode, bsim1.B1dNodePrime] += xcbdb;
            // cstate.Matrix[bsim1.B1bNode, bsim1.B1sNodePrime] += xcbsb;
            // cstate.Matrix[bsim1.B1dNodePrime, bsim1.B1gNode] += xcdgb;
            // cstate.Matrix[bsim1.B1dNodePrime, bsim1.B1bNode] += -xcdgb - xcddb - xcdsb;
            // cstate.Matrix[bsim1.B1dNodePrime, bsim1.B1sNodePrime] += xcdsb;
            // cstate.Matrix[bsim1.B1sNodePrime, bsim1.B1gNode] += xcsgb;
            // cstate.Matrix[bsim1.B1sNodePrime, bsim1.B1bNode] += -xcsgb - xcsdb - xcssb;
            // cstate.Matrix[bsim1.B1sNodePrime, bsim1.B1dNodePrime] += xcsdb;
            // cstate.Matrix[bsim1.B1dNode, bsim1.B1dNode] += gdpr;
            // cstate.Matrix[bsim1.B1sNode, bsim1.B1sNode] += gspr;
            // cstate.Matrix[bsim1.B1bNode, bsim1.B1bNode] += gbd + gbs + -xcbgb - xcbdb - xcbsb;
            // cstate.Matrix[bsim1.B1dNodePrime, bsim1.B1dNodePrime] += gdpr + gds + gbd + xrev * (gm + gmbs) + xcddb;
            // cstate.Matrix[bsim1.B1sNodePrime, bsim1.B1sNodePrime] += gspr + gds + gbs + xnrm * (gm + gmbs) + xcssb;
            // cstate.Matrix[bsim1.B1dNode, bsim1.B1dNodePrime] -= gdpr;
            // cstate.Matrix[bsim1.B1sNode, bsim1.B1sNodePrime] -= gspr;
            // cstate.Matrix[bsim1.B1bNode, bsim1.B1dNodePrime] -= gbd - xcbdb;
            // cstate.Matrix[bsim1.B1bNode, bsim1.B1sNodePrime] -= gbs - xcbsb;
            // cstate.Matrix[bsim1.B1dNodePrime, bsim1.B1dNode] -= gdpr;
            // cstate.Matrix[bsim1.B1dNodePrime, bsim1.B1gNode] += (xnrm - xrev) * gm + xcdgb;
            // cstate.Matrix[bsim1.B1dNodePrime, bsim1.B1bNode] += -gbd + (xnrm - xrev) * gmbs + -xcdgb - xcddb - xcdsb;
            // cstate.Matrix[bsim1.B1dNodePrime, bsim1.B1sNodePrime] += -gds - xnrm * (gm + gmbs) + xcdsb;
            // cstate.Matrix[bsim1.B1sNodePrime, bsim1.B1gNode] += -(xnrm - xrev) * gm + xcsgb;
            // cstate.Matrix[bsim1.B1sNodePrime, bsim1.B1sNode] -= gspr;
            // cstate.Matrix[bsim1.B1sNodePrime, bsim1.B1bNode] += -gbs - (xnrm - xrev) * gmbs + -xcsgb - xcsdb - xcssb;
            // cstate.Matrix[bsim1.B1sNodePrime, bsim1.B1dNodePrime] += -gds - xrev * (gm + gmbs) + xcsdb;
        }
    }
}
