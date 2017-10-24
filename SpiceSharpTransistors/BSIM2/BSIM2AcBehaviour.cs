using System.Numerics;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for a <see cref="BSIM2"/>
    /// </summary>
    public class BSIM2AcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Execute the behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var bsim2 = ComponentTyped<BSIM2>();
            var model = bsim2.Model as BSIM2Model;
            var state = ckt.State;
            var cstate = state.Complex;
            int xnrm;
            int xrev;
            double gdpr, gspr, gm, gds, gmbs, gbd, gbs, capbd, capbs, cggb, cgsb, cgdb, cbgb, cbsb, cbdb, cdgb, cdsb, cddb;
            Complex xcdgb, xcddb, xcdsb, xcsgb, xcsdb, xcssb, xcggb, xcgdb, xcgsb, xcbgb, xcbdb, xcbsb;

            if (bsim2.B2mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
            }
            else
            {
                xnrm = 0;
                xrev = 1;
            }
            gdpr = bsim2.B2drainConductance;
            gspr = bsim2.B2sourceConductance;
            gm = state.States[0][bsim2.B2states + BSIM2.B2gm];
            gds = state.States[0][bsim2.B2states + BSIM2.B2gds];
            gmbs = state.States[0][bsim2.B2states + BSIM2.B2gmbs];
            gbd = state.States[0][bsim2.B2states + BSIM2.B2gbd];
            gbs = state.States[0][bsim2.B2states + BSIM2.B2gbs];
            capbd = state.States[0][bsim2.B2states + BSIM2.B2capbd];
            capbs = state.States[0][bsim2.B2states + BSIM2.B2capbs];
            /* 
            * charge oriented model parameters
            */

            cggb = state.States[0][bsim2.B2states + BSIM2.B2cggb];
            cgsb = state.States[0][bsim2.B2states + BSIM2.B2cgsb];
            cgdb = state.States[0][bsim2.B2states + BSIM2.B2cgdb];

            cbgb = state.States[0][bsim2.B2states + BSIM2.B2cbgb];
            cbsb = state.States[0][bsim2.B2states + BSIM2.B2cbsb];
            cbdb = state.States[0][bsim2.B2states + BSIM2.B2cbdb];

            cdgb = state.States[0][bsim2.B2states + BSIM2.B2cdgb];
            cdsb = state.States[0][bsim2.B2states + BSIM2.B2cdsb];
            cddb = state.States[0][bsim2.B2states + BSIM2.B2cddb];

            xcdgb = (cdgb - bsim2.pParam.B2GDoverlapCap) * cstate.Laplace;
            xcddb = (cddb + capbd + bsim2.pParam.B2GDoverlapCap) * cstate.Laplace;
            xcdsb = cdsb * cstate.Laplace;
            xcsgb = -(cggb + cbgb + cdgb + bsim2.pParam.B2GSoverlapCap) * cstate.Laplace;
            xcsdb = -(cgdb + cbdb + cddb) * cstate.Laplace;
            xcssb = (capbs + bsim2.pParam.B2GSoverlapCap - (cgsb + cbsb + cdsb)) * cstate.Laplace;
            xcggb = (cggb + bsim2.pParam.B2GDoverlapCap + bsim2.pParam.B2GSoverlapCap + bsim2.pParam.B2GBoverlapCap) * cstate.Laplace;
            xcgdb = (cgdb - bsim2.pParam.B2GDoverlapCap) * cstate.Laplace;
            xcgsb = (cgsb - bsim2.pParam.B2GSoverlapCap) * cstate.Laplace;
            xcbgb = (cbgb - bsim2.pParam.B2GBoverlapCap) * cstate.Laplace;
            xcbdb = (cbdb - capbd) * cstate.Laplace;
            xcbsb = (cbsb - capbs) * cstate.Laplace;

            cstate.Matrix[bsim2.B2gNode, bsim2.B2gNode] += xcggb;
            cstate.Matrix[bsim2.B2bNode, bsim2.B2bNode] += -xcbgb - xcbdb - xcbsb;
            cstate.Matrix[bsim2.B2dNodePrime, bsim2.B2dNodePrime] += xcddb;
            cstate.Matrix[bsim2.B2sNodePrime, bsim2.B2sNodePrime] += xcssb;
            cstate.Matrix[bsim2.B2gNode, bsim2.B2bNode] += -xcggb - xcgdb - xcgsb;
            cstate.Matrix[bsim2.B2gNode, bsim2.B2dNodePrime] += xcgdb;
            cstate.Matrix[bsim2.B2gNode, bsim2.B2sNodePrime] += xcgsb;
            cstate.Matrix[bsim2.B2bNode, bsim2.B2gNode] += xcbgb;
            cstate.Matrix[bsim2.B2bNode, bsim2.B2dNodePrime] += xcbdb;
            cstate.Matrix[bsim2.B2bNode, bsim2.B2sNodePrime] += xcbsb;
            cstate.Matrix[bsim2.B2dNodePrime, bsim2.B2gNode] += xcdgb;
            cstate.Matrix[bsim2.B2dNodePrime, bsim2.B2bNode] += -xcdgb - xcddb - xcdsb;
            cstate.Matrix[bsim2.B2dNodePrime, bsim2.B2sNodePrime] += xcdsb;
            cstate.Matrix[bsim2.B2sNodePrime, bsim2.B2gNode] += xcsgb;
            cstate.Matrix[bsim2.B2sNodePrime, bsim2.B2bNode] += -xcsgb - xcsdb - xcssb;
            cstate.Matrix[bsim2.B2sNodePrime, bsim2.B2dNodePrime] += xcsdb;
            cstate.Matrix[bsim2.B2dNode, bsim2.B2dNode] += gdpr;
            cstate.Matrix[bsim2.B2sNode, bsim2.B2sNode] += gspr;
            cstate.Matrix[bsim2.B2bNode, bsim2.B2bNode] += gbd + gbs + -xcbgb - xcbdb - xcbsb;
            cstate.Matrix[bsim2.B2dNodePrime, bsim2.B2dNodePrime] += gdpr + gds + gbd + xrev * (gm + gmbs) + xcddb;
            cstate.Matrix[bsim2.B2sNodePrime, bsim2.B2sNodePrime] += gspr + gds + gbs + xnrm * (gm + gmbs) + xcssb;
            cstate.Matrix[bsim2.B2dNode, bsim2.B2dNodePrime] -= gdpr;
            cstate.Matrix[bsim2.B2sNode, bsim2.B2sNodePrime] -= gspr;
            cstate.Matrix[bsim2.B2bNode, bsim2.B2dNodePrime] -= gbd - xcbdb;
            cstate.Matrix[bsim2.B2bNode, bsim2.B2sNodePrime] -= gbs - xcbsb;
            cstate.Matrix[bsim2.B2dNodePrime, bsim2.B2dNode] -= gdpr;
            cstate.Matrix[bsim2.B2dNodePrime, bsim2.B2gNode] += (xnrm - xrev) * gm + xcdgb;
            cstate.Matrix[bsim2.B2dNodePrime, bsim2.B2bNode] += -gbd + (xnrm - xrev) * gmbs + -xcdgb - xcddb - xcdsb;
            cstate.Matrix[bsim2.B2dNodePrime, bsim2.B2sNodePrime] += -gds - xnrm * (gm + gmbs) + xcdsb;
            cstate.Matrix[bsim2.B2sNodePrime, bsim2.B2gNode] += -(xnrm - xrev) * gm + xcsgb;
            cstate.Matrix[bsim2.B2sNodePrime, bsim2.B2sNode] -= gspr;
            cstate.Matrix[bsim2.B2sNodePrime, bsim2.B2bNode] += -gbs - (xnrm - xrev) * gmbs + -xcsgb - xcsdb - xcssb;
            cstate.Matrix[bsim2.B2sNodePrime, bsim2.B2dNodePrime] += -gds - xrev * (gm + gmbs) + xcsdb;
        }
    }
}
