using System;
using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Components.Mosfet.Level1;
using SpiceSharp.Components.Transistors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.Mosfet.Level1
{
    /// <summary>
    /// AC behavior for a <see cref="Components.MOS1"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        TemperatureBehavior temp;
        LoadBehavior load;
        ModelTemperatureBehavior modeltemp;

        public double MOS1capgs { get; protected set; }
        public double MOS1capgd { get; protected set; }
        public double MOS1capbs { get; protected set; }
        public double MOS1capbd { get; protected set; }
        public double MOS1capgb { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int MOS1dNode, MOS1gNode, MOS1sNode, MOS1bNode, MOS1sNodePrime, MOS1dNodePrime;
        protected MatrixElement MOS1DdPtr { get; private set; }
        protected MatrixElement MOS1GgPtr { get; private set; }
        protected MatrixElement MOS1SsPtr { get; private set; }
        protected MatrixElement MOS1BbPtr { get; private set; }
        protected MatrixElement MOS1DPdpPtr { get; private set; }
        protected MatrixElement MOS1SPspPtr { get; private set; }
        protected MatrixElement MOS1DdpPtr { get; private set; }
        protected MatrixElement MOS1GbPtr { get; private set; }
        protected MatrixElement MOS1GdpPtr { get; private set; }
        protected MatrixElement MOS1GspPtr { get; private set; }
        protected MatrixElement MOS1SspPtr { get; private set; }
        protected MatrixElement MOS1BdpPtr { get; private set; }
        protected MatrixElement MOS1BspPtr { get; private set; }
        protected MatrixElement MOS1DPspPtr { get; private set; }
        protected MatrixElement MOS1DPdPtr { get; private set; }
        protected MatrixElement MOS1BgPtr { get; private set; }
        protected MatrixElement MOS1DPgPtr { get; private set; }
        protected MatrixElement MOS1SPgPtr { get; private set; }
        protected MatrixElement MOS1SPsPtr { get; private set; }
        protected MatrixElement MOS1DPbPtr { get; private set; }
        protected MatrixElement MOS1SPbPtr { get; private set; }
        protected MatrixElement MOS1SPdpPtr { get; private set; }

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
            MOS1dNode = pins[0];
            MOS1gNode = pins[1];
            MOS1sNode = pins[2];
            MOS1bNode = pins[3];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            // Get extra equations
            MOS1dNodePrime = load.MOS1dNodePrime;
            MOS1sNodePrime = load.MOS1sNodePrime;

            // Get matrix pointers
            MOS1DdPtr = matrix.GetElement(MOS1dNode, MOS1dNode);
            MOS1GgPtr = matrix.GetElement(MOS1gNode, MOS1gNode);
            MOS1SsPtr = matrix.GetElement(MOS1sNode, MOS1sNode);
            MOS1BbPtr = matrix.GetElement(MOS1bNode, MOS1bNode);
            MOS1DPdpPtr = matrix.GetElement(MOS1dNodePrime, MOS1dNodePrime);
            MOS1SPspPtr = matrix.GetElement(MOS1sNodePrime, MOS1sNodePrime);
            MOS1DdpPtr = matrix.GetElement(MOS1dNode, MOS1dNodePrime);
            MOS1GbPtr = matrix.GetElement(MOS1gNode, MOS1bNode);
            MOS1GdpPtr = matrix.GetElement(MOS1gNode, MOS1dNodePrime);
            MOS1GspPtr = matrix.GetElement(MOS1gNode, MOS1sNodePrime);
            MOS1SspPtr = matrix.GetElement(MOS1sNode, MOS1sNodePrime);
            MOS1BdpPtr = matrix.GetElement(MOS1bNode, MOS1dNodePrime);
            MOS1BspPtr = matrix.GetElement(MOS1bNode, MOS1sNodePrime);
            MOS1DPspPtr = matrix.GetElement(MOS1dNodePrime, MOS1sNodePrime);
            MOS1DPdPtr = matrix.GetElement(MOS1dNodePrime, MOS1dNode);
            MOS1BgPtr = matrix.GetElement(MOS1bNode, MOS1gNode);
            MOS1DPgPtr = matrix.GetElement(MOS1dNodePrime, MOS1gNode);
            MOS1SPgPtr = matrix.GetElement(MOS1sNodePrime, MOS1gNode);
            MOS1SPsPtr = matrix.GetElement(MOS1sNodePrime, MOS1sNode);
            MOS1DPbPtr = matrix.GetElement(MOS1dNodePrime, MOS1bNode);
            MOS1SPbPtr = matrix.GetElement(MOS1sNodePrime, MOS1bNode);
            MOS1SPdpPtr = matrix.GetElement(MOS1sNodePrime, MOS1dNodePrime);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            MOS1DdPtr = null;
            MOS1GgPtr = null;
            MOS1SsPtr = null;
            MOS1BbPtr = null;
            MOS1DPdpPtr = null;
            MOS1SPspPtr = null;
            MOS1DdpPtr = null;
            MOS1GbPtr = null;
            MOS1GdpPtr = null;
            MOS1GspPtr = null;
            MOS1SspPtr = null;
            MOS1BdpPtr = null;
            MOS1BspPtr = null;
            MOS1DPspPtr = null;
            MOS1DPdPtr = null;
            MOS1BgPtr = null;
            MOS1DPgPtr = null;
            MOS1SPgPtr = null;
            MOS1SPsPtr = null;
            MOS1DPbPtr = null;
            MOS1SPbPtr = null;
            MOS1SPdpPtr = null;
        }

        /// <summary>
        /// Initialize AC parameters
        /// </summary>
        /// <param name="sim"></param>
        public override void InitializeParameters(FrequencySimulation sim)
        {
            double arg, sarg, sargsw;

            // Get voltages
            double vbd = load.MOS1vbd;
            double vbs = load.MOS1vbs;
            double vgs = load.MOS1vgs;
            double vds = load.MOS1vds;
            double vgd = vgs - vds;
            double vgb = vgs - vbs;

            double EffectiveLength = bp.MOS1l - 2 * mbp.MOS1latDiff;
            double GateSourceOverlapCap = mbp.MOS1gateSourceOverlapCapFactor * bp.MOS1w;
            double GateDrainOverlapCap = mbp.MOS1gateDrainOverlapCapFactor * bp.MOS1w;
            double GateBulkOverlapCap = mbp.MOS1gateBulkOverlapCapFactor * EffectiveLength;
            double OxideCap = modeltemp.MOS1oxideCapFactor * EffectiveLength * bp.MOS1w;

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
            /* CAPBYPASS */

            /* can't bypass the diode capacitance calculations */
            /* CAPZEROBYPASS */
            if (vbs < temp.MOS1tDepCap)
            {
                arg = 1 - vbs / temp.MOS1tBulkPot;
                /* 
                 * the following block looks somewhat long and messy, 
                 * but since most users use the default grading
                 * coefficients of .5, and sqrt is MUCH faster than an
                 * Math.Exp(Math.Log()) we use this special case code to buy time.
                 * (as much as 10% of total job time!)
                 */
                if (mbp.MOS1bulkJctBotGradingCoeff.Value == mbp.MOS1bulkJctSideGradingCoeff.Value)
                {
                    if (mbp.MOS1bulkJctBotGradingCoeff.Value == .5)
                    {
                        sarg = sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        sarg = sargsw = Math.Exp(-mbp.MOS1bulkJctBotGradingCoeff * Math.Log(arg));
                    }
                }
                else
                {
                    if (mbp.MOS1bulkJctBotGradingCoeff.Value == .5)
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-mbp.MOS1bulkJctBotGradingCoeff * Math.Log(arg));
                    }
                    if (mbp.MOS1bulkJctSideGradingCoeff.Value == .5)
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-mbp.MOS1bulkJctSideGradingCoeff * Math.Log(arg));
                    }
                }

                MOS1capbs = temp.MOS1Cbs * sarg + temp.MOS1Cbssw * sargsw;
            }
            else
            {
                MOS1capbs = temp.MOS1f2s + temp.MOS1f3s * vbs;
            }

            /* can't bypass the diode capacitance calculations */

            /* CAPZEROBYPASS */
            if (vbd < temp.MOS1tDepCap)
            {
                arg = 1 - vbd / temp.MOS1tBulkPot;
                /* 
                * the following block looks somewhat long and messy, 
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (mbp.MOS1bulkJctBotGradingCoeff.Value == .5 && mbp.MOS1bulkJctSideGradingCoeff.Value == .5)
                {
                    sarg = sargsw = 1 / Math.Sqrt(arg);
                }
                else
                {
                    if (mbp.MOS1bulkJctBotGradingCoeff.Value == .5)
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-mbp.MOS1bulkJctBotGradingCoeff * Math.Log(arg));
                    }
                    if (mbp.MOS1bulkJctSideGradingCoeff.Value == .5)
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-mbp.MOS1bulkJctSideGradingCoeff * Math.Log(arg));
                    }
                }
                /* NOSQRT */
                MOS1capbd = temp.MOS1Cbd * sarg + temp.MOS1Cbdsw * sargsw;
            }
            else
            {
                MOS1capbd = temp.MOS1f2d + vbd * temp.MOS1f3d;
            }

            /* 
             * calculate meyer's capacitors
             */
            /* 
             * new cmeyer - this just evaluates at the current time, 
             * expects you to remember values from previous time
             * returns 1 / 2 of non-constant portion of capacitance
             * you must add in the other half from previous time
             * and the constant part
             */
            double icapgs, icapgd, icapgb;
            if (load.MOS1mode > 0)
            {
                Transistor.DEVqmeyer(vgs, vgd, vgb, mbp.MOS1type * load.MOS1von, mbp.MOS1type * load.MOS1vdsat,
                    out icapgs, out icapgd, out icapgb,
                    temp.MOS1tPhi, OxideCap);
            }
            else
            {
                Transistor.DEVqmeyer(vgd, vgs, vgb, mbp.MOS1type * load.MOS1von, mbp.MOS1type * load.MOS1vdsat,
                    out icapgd, out icapgs, out icapgb,
                    temp.MOS1tPhi, OxideCap);
            }
            MOS1capgs = icapgs;
            MOS1capgd = icapgd;
            MOS1capgb = icapgb;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var cstate = state;
            int xnrm, xrev;
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, capgs, capgd, capgb, xgs, xgd, xgb, xbd,
                xbs;

            if (load.MOS1mode < 0)
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
            EffectiveLength = bp.MOS1l - 2 * mbp.MOS1latDiff;
            GateSourceOverlapCap = mbp.MOS1gateSourceOverlapCapFactor * bp.MOS1w;
            GateDrainOverlapCap = mbp.MOS1gateDrainOverlapCapFactor * bp.MOS1w;
            GateBulkOverlapCap = mbp.MOS1gateBulkOverlapCapFactor * EffectiveLength;
            capgs = (MOS1capgs + MOS1capgs + GateSourceOverlapCap);
            capgd = (MOS1capgd + MOS1capgd + GateDrainOverlapCap);
            capgb = (MOS1capgb + MOS1capgb + GateBulkOverlapCap);
            xgs = capgs * cstate.Laplace.Imaginary;
            xgd = capgd * cstate.Laplace.Imaginary;
            xgb = capgb * cstate.Laplace.Imaginary;
            xbd = MOS1capbd * cstate.Laplace.Imaginary;
            xbs = MOS1capbs * cstate.Laplace.Imaginary;

            /* 
			 * load matrix
			 */
            MOS1GgPtr.Add(new Complex(0.0, xgd + xgs + xgb));
            MOS1BbPtr.Add(new Complex(load.MOS1gbd + load.MOS1gbs, xgb + xbd + xbs));
            MOS1DPdpPtr.Add(new Complex(temp.MOS1drainConductance + load.MOS1gds + load.MOS1gbd + xrev * (load.MOS1gm + load.MOS1gmbs), xgd + xbd));
            MOS1SPspPtr.Add(new Complex(temp.MOS1sourceConductance + load.MOS1gds + load.MOS1gbs + xnrm * (load.MOS1gm + load.MOS1gmbs), xgs + xbs));
            MOS1GbPtr.Sub(new Complex(0.0, xgb));
            MOS1GdpPtr.Sub(new Complex(0.0, xgd));
            MOS1GspPtr.Sub(new Complex(0.0, xgs));
            MOS1BgPtr.Sub(new Complex(0.0, xgb));
            MOS1BdpPtr.Sub(new Complex(load.MOS1gbd, xbd));
            MOS1BspPtr.Sub(new Complex(load.MOS1gbs, xbs));
            MOS1DPgPtr.Add(new Complex((xnrm - xrev) * load.MOS1gm, -xgd));
            MOS1DPbPtr.Add(new Complex(-load.MOS1gbd + (xnrm - xrev) * load.MOS1gmbs, -xbd));
            MOS1SPgPtr.Sub(new Complex((xnrm - xrev) * load.MOS1gm, xgs));
            MOS1SPbPtr.Sub(new Complex(load.MOS1gbs + (xnrm - xrev) * load.MOS1gmbs, xbs));
            MOS1DdPtr.Add(temp.MOS1drainConductance);
            MOS1SsPtr.Add(temp.MOS1sourceConductance);
            MOS1DdpPtr.Sub(temp.MOS1drainConductance);
            MOS1SspPtr.Sub(temp.MOS1sourceConductance);
            MOS1DPdPtr.Sub(temp.MOS1drainConductance);
            MOS1DPspPtr.Sub(load.MOS1gds + xnrm * (load.MOS1gm + load.MOS1gmbs));
            MOS1SPsPtr.Sub(temp.MOS1sourceConductance);
            MOS1SPdpPtr.Sub(load.MOS1gds + xrev * (load.MOS1gm + load.MOS1gmbs));
        }
    }
}
