using System;
using SpiceSharp.Sparse;
using SpiceSharp.Components.Mosfet.Level2;
using SpiceSharp.Circuits;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Behaviors.Mosfet.Level2
{
    /// <summary>
    /// Transient behavior for a <see cref="Components.MOS2"/>
    /// </summary>
    public class TransientBehavior : Behaviors.TransientBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        LoadBehavior load;
        TemperatureBehavior temp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Nodes
        /// </summary>
        int MOS2dNode, MOS2gNode, MOS2sNode, MOS2bNode, MOS2dNodePrime, MOS2sNodePrime;
        protected MatrixElement MOS2DdPtr { get; private set; }
        protected MatrixElement MOS2GgPtr { get; private set; }
        protected MatrixElement MOS2SsPtr { get; private set; }
        protected MatrixElement MOS2BbPtr { get; private set; }
        protected MatrixElement MOS2DPdpPtr { get; private set; }
        protected MatrixElement MOS2SPspPtr { get; private set; }
        protected MatrixElement MOS2DdpPtr { get; private set; }
        protected MatrixElement MOS2GbPtr { get; private set; }
        protected MatrixElement MOS2GdpPtr { get; private set; }
        protected MatrixElement MOS2GspPtr { get; private set; }
        protected MatrixElement MOS2SspPtr { get; private set; }
        protected MatrixElement MOS2BdpPtr { get; private set; }
        protected MatrixElement MOS2BspPtr { get; private set; }
        protected MatrixElement MOS2DPspPtr { get; private set; }
        protected MatrixElement MOS2DPdPtr { get; private set; }
        protected MatrixElement MOS2BgPtr { get; private set; }
        protected MatrixElement MOS2DPgPtr { get; private set; }
        protected MatrixElement MOS2SPgPtr { get; private set; }
        protected MatrixElement MOS2SPsPtr { get; private set; }
        protected MatrixElement MOS2DPbPtr { get; private set; }
        protected MatrixElement MOS2SPbPtr { get; private set; }
        protected MatrixElement MOS2SPdpPtr { get; private set; }

        /// <summary>
        /// Shared variables
        /// </summary>
        public double MOS2capbs { get; protected set; }
        public double MOS2capbd { get; protected set; }

        /// <summary>
        /// Constants
        /// </summary>
        StateHistory MOS2vbs;
        StateHistory MOS2vgs;
        StateHistory MOS2vds;
        StateHistory MOS2capgs;
        StateHistory MOS2capgd;
        StateHistory MOS2capgb;
        StateDerivative MOS2qgs;
        StateDerivative MOS2qgd;
        StateDerivative MOS2qgb;
        StateDerivative MOS2qbd;
        StateDerivative MOS2qbs;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public TransientBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();
            mbp = provider.GetParameters<ModelBaseParameters>(1);

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>();
            load = provider.GetBehavior<LoadBehavior>();
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            MOS2dNode = pins[0];
            MOS2gNode = pins[1];
            MOS2sNode = pins[2];
            MOS2bNode = pins[3];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            // Get extra equations
            MOS2sNodePrime = load.MOS2sNodePrime;
            MOS2dNodePrime = load.MOS2dNodePrime;

            // Get matrix elements
            MOS2DdPtr = matrix.GetElement(MOS2dNode, MOS2dNode);
            MOS2GgPtr = matrix.GetElement(MOS2gNode, MOS2gNode);
            MOS2SsPtr = matrix.GetElement(MOS2sNode, MOS2sNode);
            MOS2BbPtr = matrix.GetElement(MOS2bNode, MOS2bNode);
            MOS2DPdpPtr = matrix.GetElement(MOS2dNodePrime, MOS2dNodePrime);
            MOS2SPspPtr = matrix.GetElement(MOS2sNodePrime, MOS2sNodePrime);
            MOS2DdpPtr = matrix.GetElement(MOS2dNode, MOS2dNodePrime);
            MOS2GbPtr = matrix.GetElement(MOS2gNode, MOS2bNode);
            MOS2GdpPtr = matrix.GetElement(MOS2gNode, MOS2dNodePrime);
            MOS2GspPtr = matrix.GetElement(MOS2gNode, MOS2sNodePrime);
            MOS2SspPtr = matrix.GetElement(MOS2sNode, MOS2sNodePrime);
            MOS2BdpPtr = matrix.GetElement(MOS2bNode, MOS2dNodePrime);
            MOS2BspPtr = matrix.GetElement(MOS2bNode, MOS2sNodePrime);
            MOS2DPspPtr = matrix.GetElement(MOS2dNodePrime, MOS2sNodePrime);
            MOS2DPdPtr = matrix.GetElement(MOS2dNodePrime, MOS2dNode);
            MOS2BgPtr = matrix.GetElement(MOS2bNode, MOS2gNode);
            MOS2DPgPtr = matrix.GetElement(MOS2dNodePrime, MOS2gNode);
            MOS2SPgPtr = matrix.GetElement(MOS2sNodePrime, MOS2gNode);
            MOS2SPsPtr = matrix.GetElement(MOS2sNodePrime, MOS2sNode);
            MOS2DPbPtr = matrix.GetElement(MOS2dNodePrime, MOS2bNode);
            MOS2SPbPtr = matrix.GetElement(MOS2sNodePrime, MOS2bNode);
            MOS2SPdpPtr = matrix.GetElement(MOS2sNodePrime, MOS2dNodePrime);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            MOS2DdPtr = null;
            MOS2GgPtr = null;
            MOS2SsPtr = null;
            MOS2BbPtr = null;
            MOS2DPdpPtr = null;
            MOS2SPspPtr = null;
            MOS2DdpPtr = null;
            MOS2GbPtr = null;
            MOS2GdpPtr = null;
            MOS2GspPtr = null;
            MOS2SspPtr = null;
            MOS2BdpPtr = null;
            MOS2BspPtr = null;
            MOS2DPspPtr = null;
            MOS2DPdPtr = null;
            MOS2BgPtr = null;
            MOS2DPgPtr = null;
            MOS2SPgPtr = null;
            MOS2SPsPtr = null;
            MOS2DPbPtr = null;
            MOS2SPbPtr = null;
            MOS2SPdpPtr = null;
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="states">States</param>
        public override void CreateStates(StatePool states)
        {
            MOS2vbs = states.CreateHistory();
            MOS2vgs = states.CreateHistory();
            MOS2vds = states.CreateHistory();
            MOS2capgs = states.CreateHistory();
            MOS2capgd = states.CreateHistory();
            MOS2capgb = states.CreateHistory();
            MOS2qgs = states.Create();
            MOS2qgd = states.Create();
            MOS2qgb = states.Create();
            MOS2qbd = states.Create();
            MOS2qbs = states.Create();
        }

        /// <summary>
        /// Calculate initial states
        /// </summary>
        /// <param name="sim">Simulation</param>
        public override void GetDCstate(TimeSimulation sim)
        {
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap,
                OxideCap, vgs, vbs, vbd, vgb, vgd, von,
                vdsat, sargsw, vgs1, vgd1, vgb1, capgs = 0.0, capgd = 0.0, capgb = 0.0; ;

            vbs = load.MOS2vbs;
            vbd = load.MOS2vbd;
            vgs = load.MOS2vgs;
            vgd = load.MOS2vgs - load.MOS2vds;
            vgb = load.MOS2vgs - load.MOS2vbs;
            von = mbp.MOS2type * load.MOS2von;
            vdsat = mbp.MOS2type * load.MOS2vdsat;

            MOS2vds.Value = load.MOS2vds;
            MOS2vbs.Value = vbs;
            MOS2vgs.Value = vgs;

            EffectiveLength = bp.MOS2l - 2 * mbp.MOS2latDiff;
            GateSourceOverlapCap = mbp.MOS2gateSourceOverlapCapFactor * bp.MOS2w;
            GateDrainOverlapCap = mbp.MOS2gateDrainOverlapCapFactor * bp.MOS2w;
            GateBulkOverlapCap = mbp.MOS2gateBulkOverlapCapFactor * EffectiveLength;
            OxideCap = modeltemp.MOS2oxideCapFactor * EffectiveLength * bp.MOS2w;

            /* 
            * now we do the hard part of the bulk - drain and bulk - source
            * diode - we evaluate the non - linear capacitance and
            * charge
            * 
            * the basic equations are not hard, but the implementation
            * is somewhat long in an attempt to avoid log / exponential
            * evaluations
            */
            /* 
            * charge storage elements
            * 
            * .. bulk - drain and bulk - source depletion capacitances
            */
            if (vbs < temp.MOS2tDepCap)
            {
                double arg = 1 - vbs / temp.MOS2tBulkPot, sarg;
                /* 
                * the following block looks somewhat long and messy, 
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (mbp.MOS2bulkJctBotGradingCoeff.Value == mbp.MOS2bulkJctSideGradingCoeff)
                {
                    if (mbp.MOS2bulkJctBotGradingCoeff.Value == .5)
                    {
                        sarg = sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        sarg = sargsw = Math.Exp(-mbp.MOS2bulkJctBotGradingCoeff * Math.Log(arg));
                    }
                }
                else
                {
                    if (mbp.MOS2bulkJctBotGradingCoeff.Value == .5)
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-mbp.MOS2bulkJctBotGradingCoeff * Math.Log(arg));
                    }
                    if (mbp.MOS2bulkJctSideGradingCoeff.Value == .5)
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-mbp.MOS2bulkJctSideGradingCoeff * Math.Log(arg));
                    }
                }
                // NOSQRT
                MOS2qbs.Value = temp.MOS2tBulkPot * (temp.MOS2Cbs * (1 - arg * sarg) / (1 - mbp.MOS2bulkJctBotGradingCoeff) +
                    temp.MOS2Cbssw * (1 - arg * sargsw) / (1 - mbp.MOS2bulkJctSideGradingCoeff));
            }
            else
            {
                MOS2qbs.Value = temp.MOS2f4s + vbs * (temp.MOS2f2s + vbs * (temp.MOS2f3s / 2));
            }

            if (vbd < temp.MOS2tDepCap)
            {
                double arg = 1 - vbd / temp.MOS2tBulkPot, sarg;
                /* 
                * the following block looks somewhat long and messy, 
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (mbp.MOS2bulkJctBotGradingCoeff.Value == .5 && mbp.MOS2bulkJctSideGradingCoeff.Value == .5)
                {
                    sarg = sargsw = 1 / Math.Sqrt(arg);
                }
                else
                {
                    if (mbp.MOS2bulkJctBotGradingCoeff.Value == .5)
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-mbp.MOS2bulkJctBotGradingCoeff * Math.Log(arg));
                    }
                    if (mbp.MOS2bulkJctSideGradingCoeff.Value == .5)
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-mbp.MOS2bulkJctSideGradingCoeff * Math.Log(arg));
                    }
                }
                /* NOSQRT */
                MOS2qbd.Value = temp.MOS2tBulkPot * (temp.MOS2Cbd * (1 - arg * sarg) / (1 - mbp.MOS2bulkJctBotGradingCoeff) +
                    temp.MOS2Cbdsw * (1 - arg * sargsw) / (1 - mbp.MOS2bulkJctSideGradingCoeff));
            }
            else
            {
                MOS2qbd.Value = temp.MOS2f4d + vbd * (temp.MOS2f2d + vbd * temp.MOS2f3d / 2);
            }

            /* 
             * calculate meyer's capacitors
             */
            double icapgs, icapgd, icapgb;
            if (load.MOS2mode > 0)
            {
                Transistor.DEVqmeyer(vgs, vgd, vgb, von, vdsat,
                    out icapgs, out icapgd, out icapgb, temp.MOS2tPhi, OxideCap);
            }
            else
            {
                Transistor.DEVqmeyer(vgd, vgs, vgb, von, vdsat,
                    out icapgd, out icapgs, out icapgb, temp.MOS2tPhi, OxideCap);
            }
            MOS2capgs.Value = icapgs;
            MOS2capgd.Value = icapgd;
            MOS2capgb.Value = icapgb;

            vgs1 = MOS2vgs.GetPreviousValue(1);
            vgd1 = vgs1 - MOS2vds.GetPreviousValue(1);
            vgb1 = vgs1 - MOS2vbs.GetPreviousValue(1);

            capgs = 2 * MOS2capgs.Value + GateSourceOverlapCap;
            capgd = 2 * MOS2capgd.Value + GateDrainOverlapCap;
            capgb = 2 * MOS2capgb.Value + GateBulkOverlapCap;

            /* TRANOP */
            MOS2qgs.Value = capgs * vgs;
            MOS2qgd.Value = capgd * vgd;
            MOS2qgb.Value = capgb * vgb;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="sim"></param>
        public override void Transient(TimeSimulation sim)
        {
            var state = sim.Circuit.State;
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap,
                OxideCap, vgs, vbs, vbd, vgb, vgd, von,
                vdsat, sargsw, vgs1, vgd1, vgb1, capgs = 0.0, capgd = 0.0, capgb = 0.0, gcgs, ceqgs, gcgd, ceqgd, gcgb, ceqgb, ceqbs,
                ceqbd;

            vbs = load.MOS2vbs;
            vbd = load.MOS2vbd;
            vgs = load.MOS2vgs;
            vgd = load.MOS2vgs - load.MOS2vds;
            vgb = load.MOS2vgs - load.MOS2vbs;
            von = mbp.MOS2type * load.MOS2von;
            vdsat = mbp.MOS2type * load.MOS2vdsat;

            MOS2vds.Value = load.MOS2vds;
            MOS2vbs.Value = vbs;
            MOS2vgs.Value = vgs;

            double MOS2gbd = 0.0;
            double MOS2cbd = 0.0;
            double MOS2cd = 0.0;
            double MOS2gbs = 0.0;
            double MOS2cbs = 0.0;

            EffectiveLength = bp.MOS2l - 2 * mbp.MOS2latDiff;
            GateSourceOverlapCap = mbp.MOS2gateSourceOverlapCapFactor * bp.MOS2w;
            GateDrainOverlapCap = mbp.MOS2gateDrainOverlapCapFactor * bp.MOS2w;
            GateBulkOverlapCap = mbp.MOS2gateBulkOverlapCapFactor * EffectiveLength;
            OxideCap = modeltemp.MOS2oxideCapFactor * EffectiveLength * bp.MOS2w;

            /* 
            * now we do the hard part of the bulk - drain and bulk - source
            * diode - we evaluate the non - linear capacitance and
            * charge
            * 
            * the basic equations are not hard, but the implementation
            * is somewhat long in an attempt to avoid log / exponential
            * evaluations
            */
            /* 
            * charge storage elements
            * 
            * .. bulk - drain and bulk - source depletion capacitances
            */
            if (vbs < temp.MOS2tDepCap)
            {
                double arg = 1 - vbs / temp.MOS2tBulkPot, sarg;
                /* 
                * the following block looks somewhat long and messy, 
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (mbp.MOS2bulkJctBotGradingCoeff.Value == mbp.MOS2bulkJctSideGradingCoeff)
                {
                    if (mbp.MOS2bulkJctBotGradingCoeff.Value == .5)
                    {
                        sarg = sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        sarg = sargsw = Math.Exp(-mbp.MOS2bulkJctBotGradingCoeff * Math.Log(arg));
                    }
                }
                else
                {
                    if (mbp.MOS2bulkJctBotGradingCoeff.Value == .5)
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-mbp.MOS2bulkJctBotGradingCoeff * Math.Log(arg));
                    }
                    if (mbp.MOS2bulkJctSideGradingCoeff.Value == .5)
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-mbp.MOS2bulkJctSideGradingCoeff * Math.Log(arg));
                    }
                }
                // NOSQRT
                MOS2qbs.Value = temp.MOS2tBulkPot * (temp.MOS2Cbs * (1 - arg * sarg) / (1 - mbp.MOS2bulkJctBotGradingCoeff) +
                    temp.MOS2Cbssw * (1 - arg * sargsw) / (1 - mbp.MOS2bulkJctSideGradingCoeff));
                MOS2capbs = temp.MOS2Cbs * sarg + temp.MOS2Cbssw * sargsw;
            }
            else
            {
                MOS2qbs.Value = temp.MOS2f4s + vbs * (temp.MOS2f2s + vbs * (temp.MOS2f3s / 2));
                MOS2capbs = temp.MOS2f2s + temp.MOS2f3s * vbs;
            }

            if (vbd < temp.MOS2tDepCap)
            {
                double arg = 1 - vbd / temp.MOS2tBulkPot, sarg;
                /* 
                * the following block looks somewhat long and messy, 
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (mbp.MOS2bulkJctBotGradingCoeff.Value == .5 && mbp.MOS2bulkJctSideGradingCoeff.Value == .5)
                {
                    sarg = sargsw = 1 / Math.Sqrt(arg);
                }
                else
                {
                    if (mbp.MOS2bulkJctBotGradingCoeff.Value == .5)
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-mbp.MOS2bulkJctBotGradingCoeff * Math.Log(arg));
                    }
                    if (mbp.MOS2bulkJctSideGradingCoeff.Value == .5)
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-mbp.MOS2bulkJctSideGradingCoeff * Math.Log(arg));
                    }
                }
                /* NOSQRT */
                MOS2qbd.Value = temp.MOS2tBulkPot * (temp.MOS2Cbd * (1 - arg * sarg) / (1 - mbp.MOS2bulkJctBotGradingCoeff) +
                    temp.MOS2Cbdsw * (1 - arg * sargsw) / (1 - mbp.MOS2bulkJctSideGradingCoeff));
                MOS2capbd = temp.MOS2Cbd * sarg + temp.MOS2Cbdsw * sargsw;
            }
            else
            {
                MOS2qbd.Value = temp.MOS2f4d + vbd * (temp.MOS2f2d + vbd * temp.MOS2f3d / 2);
                MOS2capbd = temp.MOS2f2d + vbd * temp.MOS2f3d;
            }

            /* (above only excludes tranop, since we're only at this
            * point if tran or tranop)
            */
            /* 
            * calculate equivalent conductances and currents for
            * depletion capacitors
            */

            // integrate the capacitors and save results
            MOS2qbd.Integrate();
            MOS2gbd += MOS2qbd.Jacobian(MOS2capbd);
            MOS2cbd += MOS2qbd.Derivative;
            MOS2cd -= MOS2qbd.Derivative;
            MOS2qbs.Integrate();
            MOS2gbs += MOS2qbs.Jacobian(MOS2capbs);
            MOS2cbs += MOS2qbs.Derivative;

            /* 
             * calculate meyer's capacitors
             */
            double icapgs, icapgd, icapgb;
            if (load.MOS2mode > 0)
            {
                Transistor.DEVqmeyer(vgs, vgd, vgb, von, vdsat,
                    out icapgs, out icapgd, out icapgb, temp.MOS2tPhi, OxideCap);
            }
            else
            {
                Transistor.DEVqmeyer(vgd, vgs, vgb, von, vdsat,
                    out icapgd, out icapgs, out icapgb, temp.MOS2tPhi, OxideCap);
            }
            MOS2capgs.Value = icapgs;
            MOS2capgd.Value = icapgd;
            MOS2capgb.Value = icapgb;

            vgs1 = MOS2vgs.GetPreviousValue(1);
            vgd1 = vgs1 - MOS2vds.GetPreviousValue(1);
            vgb1 = vgs1 - MOS2vbs.GetPreviousValue(1);
            capgs = MOS2capgs.Value + MOS2capgs.GetPreviousValue(1) + GateSourceOverlapCap;
            capgd = MOS2capgd.Value + MOS2capgd.GetPreviousValue(1) + GateDrainOverlapCap;
            capgb = MOS2capgb.Value + MOS2capgb.GetPreviousValue(1) + GateBulkOverlapCap;

            /* 
            * store small - signal parameters (for meyer's model)
            * all parameters already stored, so done...
            */

            MOS2qgs.Value = (vgs - vgs1) * capgs + MOS2qgs.GetPreviousValue(1);
            MOS2qgd.Value = (vgd - vgd1) * capgd + MOS2qgd.GetPreviousValue(1);
            MOS2qgb.Value = (vgb - vgb1) * capgb + MOS2qgb.GetPreviousValue(1);

            /* 
             * calculate equivalent conductances and currents for
             * meyer"s capacitors
             */
            MOS2qgs.Integrate();
            gcgs = MOS2qgs.Jacobian(capgs);
            ceqgs = MOS2qgs.Current(gcgs, vgs);
            MOS2qgd.Integrate();
            gcgd = MOS2qgd.Jacobian(capgd);
            ceqgd = MOS2qgd.Current(gcgd, vgd);
            MOS2qgb.Integrate();
            gcgb = MOS2qgb.Jacobian(capgb);
            ceqgb = MOS2qgb.Current(gcgb, vgb);

            /* 
			* store charge storage info for meyer's cap in lx table
			*/

            /* 
            * load current vector
            */
            ceqbs = mbp.MOS2type * (MOS2cbs - (MOS2gbs - state.Gmin) * vbs);
            ceqbd = mbp.MOS2type * (MOS2cbd - (MOS2gbd - state.Gmin) * vbd);
            state.Rhs[MOS2gNode] -= (mbp.MOS2type * (ceqgs + ceqgb + ceqgd));
            state.Rhs[MOS2bNode] -= (ceqbs + ceqbd - mbp.MOS2type * ceqgb);
            state.Rhs[MOS2dNodePrime] += (ceqbd + mbp.MOS2type * ceqgd);
            state.Rhs[MOS2sNodePrime] += ceqbs + mbp.MOS2type * ceqgs;

            /* 
			 * load y matrix
			 */
            MOS2GgPtr.Add(gcgd + gcgs + gcgb);
            MOS2BbPtr.Add(MOS2gbd + MOS2gbs + gcgb);
            MOS2DPdpPtr.Add(MOS2gbd + gcgd);
            MOS2SPspPtr.Add(MOS2gbs + gcgs);
            MOS2GbPtr.Sub(gcgb);
            MOS2GdpPtr.Sub(gcgd);
            MOS2GspPtr.Sub(gcgs);
            MOS2BgPtr.Sub(gcgb);
            MOS2BdpPtr.Sub(MOS2gbd);
            MOS2BspPtr.Sub(MOS2gbs);
            MOS2DPgPtr.Add(-gcgd);
            MOS2DPbPtr.Add(-MOS2gbd);
            MOS2SPgPtr.Add(-gcgs);
            MOS2SPbPtr.Add(-MOS2gbs);
        }

        /// <summary>
        /// Truncate timestep
        /// </summary>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(ref double timestep)
        {
            MOS2qgs.LocalTruncationError(ref timestep);
            MOS2qgd.LocalTruncationError(ref timestep);
            MOS2qgb.LocalTruncationError(ref timestep);
        }
    }
}
