using System;
using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Components.Mosfet.Level2;
using SpiceSharp.Simulations;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Behaviors.Mosfet.Level2
{
    /// <summary>
    /// AC behavior for a <see cref="Components.MOS2"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        LoadBehavior load;
        TemperatureBehavior temp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Shared variables
        /// </summary>
        public double MOS2capgs { get; protected set; }
        public double MOS2capgd { get; protected set; }
        public double MOS2capgb { get; protected set; }
        public double MOS2capbd { get; protected set; }
        public double MOS2capbs { get; protected set; }

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
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcBehavior(Identifier name) : base(name) { }

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
        /// Initialize AC parameters
        /// </summary>
        /// <param name="sim"></param>
        public override void InitializeParameters(FrequencySimulation sim)
        {
            var state = sim.Circuit.State;
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, Beta,
                OxideCap, vgs, vbs, vbd, vgb, vgd, von,
                vdsat, sargsw;

            vbs = load.MOS2vbs;
            vbd = load.MOS2vbd;
            vgs = load.MOS2vgs;
            vgd = load.MOS2vgs - load.MOS2vds;
            vgb = load.MOS2vgs - load.MOS2vbs;
            von = mbp.MOS2type * load.MOS2von;
            vdsat = mbp.MOS2type * load.MOS2vdsat;

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
                MOS2capbs = temp.MOS2Cbs * sarg + temp.MOS2Cbssw * sargsw;
            }
            else
            {
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
                MOS2capbd = temp.MOS2Cbd * sarg + temp.MOS2Cbdsw * sargsw;
            }
            else
            {
                MOS2capbd = temp.MOS2f2d + vbd * temp.MOS2f3d;
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
            MOS2capgs = icapgs;
            MOS2capgd = icapgd;
            MOS2capgb = icapgb;
        }

        /// <summary>
        /// Execute the behavior
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var cstate = state;
            int xnrm, xrev;
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, capgs, capgd, capgb, xgs, xgd, xgb, xbd,
                xbs;

            if (load.MOS2mode < 0)
            {
                xnrm = 0;
                xrev = 1;
            }
            else
            {
                xnrm = 1;
                xrev = 0;
            }
            /* 
			 * meyer's model parameters
			 */
            EffectiveLength = bp.MOS2l - 2 * mbp.MOS2latDiff;
            GateSourceOverlapCap = mbp.MOS2gateSourceOverlapCapFactor * bp.MOS2w;
            GateDrainOverlapCap = mbp.MOS2gateDrainOverlapCapFactor * bp.MOS2w;
            GateBulkOverlapCap = mbp.MOS2gateBulkOverlapCapFactor * EffectiveLength;
            capgs = MOS2capgs + MOS2capgs + GateSourceOverlapCap;
            capgd = MOS2capgd + MOS2capgd + GateDrainOverlapCap;
            capgb = MOS2capgb + MOS2capgb + GateBulkOverlapCap;
            xgs = capgs * cstate.Laplace.Imaginary;
            xgd = capgd * cstate.Laplace.Imaginary;
            xgb = capgb * cstate.Laplace.Imaginary;
            xbd = MOS2capbd * cstate.Laplace.Imaginary;
            xbs = MOS2capbs * cstate.Laplace.Imaginary;

            /* 
			 * load matrix
			 */
            MOS2GgPtr.Add(new Complex(0.0, xgd + xgs + xgb));
            MOS2BbPtr.Add(new Complex(load.MOS2gbd + load.MOS2gbs, xgb + xbd + xbs));
            MOS2DPdpPtr.Add(new Complex(temp.MOS2drainConductance + load.MOS2gds + load.MOS2gbd + xrev * (load.MOS2gm + load.MOS2gmbs), xgd + xbd));
            MOS2SPspPtr.Add(new Complex(temp.MOS2sourceConductance + load.MOS2gds + load.MOS2gbs + xnrm * (load.MOS2gm + load.MOS2gmbs), xgs + xbs));
            MOS2GbPtr.Sub(new Complex(0.0, xgb));
            MOS2GdpPtr.Sub(new Complex(0.0, xgd));
            MOS2GspPtr.Sub(new Complex(0.0, xgs));
            MOS2BgPtr.Sub(new Complex(0.0, xgb));
            MOS2BdpPtr.Sub(new Complex(load.MOS2gbd, xbd));
            MOS2BspPtr.Sub(new Complex(load.MOS2gbs, xbs));
            MOS2DPgPtr.Add(new Complex((xnrm - xrev) * load.MOS2gm, -xgd));
            MOS2DPbPtr.Add(new Complex(-load.MOS2gbd + (xnrm - xrev) * load.MOS2gmbs, -xbd));
            MOS2SPgPtr.Sub(new Complex((xnrm - xrev) * load.MOS2gm, xgs));
            MOS2SPbPtr.Sub(new Complex(load.MOS2gbs + (xnrm - xrev) * load.MOS2gmbs, xbs));
            MOS2DdPtr.Add(temp.MOS2drainConductance);
            MOS2SsPtr.Add(temp.MOS2sourceConductance);
            MOS2DdpPtr.Sub(temp.MOS2drainConductance);
            MOS2SspPtr.Sub(temp.MOS2sourceConductance);
            MOS2DPdPtr.Sub(temp.MOS2drainConductance);
            MOS2DPspPtr.Sub(load.MOS2gds + xnrm * (load.MOS2gm + load.MOS2gmbs));
            MOS2SPsPtr.Sub(temp.MOS2sourceConductance);
            MOS2SPdpPtr.Sub(load.MOS2gds + xrev * (load.MOS2gm + load.MOS2gmbs));
        }
    }
}
