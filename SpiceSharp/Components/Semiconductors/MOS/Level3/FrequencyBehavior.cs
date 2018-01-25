using System;
using System.Numerics;
using SpiceSharp.Sparse;
using SpiceSharp.Components.Mosfet.Level3;
using SpiceSharp.Components.Transistors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.Mosfet.Level3
{
    /// <summary>
    /// AC behavior for <see cref="Components.MOS3"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
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
        public double MOS3capbs { get; protected set; }
        public double MOS3capbd { get; protected set; }
        public double MOS3capgs { get; protected set; }
        public double MOS3capgd { get; protected set; }
        public double MOS3capgb { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int MOS3dNode, MOS3gNode, MOS3sNode, MOS3bNode, MOS3dNodePrime, MOS3sNodePrime;
        protected MatrixElement MOS3DdPtr { get; private set; }
        protected MatrixElement MOS3GgPtr { get; private set; }
        protected MatrixElement MOS3SsPtr { get; private set; }
        protected MatrixElement MOS3BbPtr { get; private set; }
        protected MatrixElement MOS3DPdpPtr { get; private set; }
        protected MatrixElement MOS3SPspPtr { get; private set; }
        protected MatrixElement MOS3DdpPtr { get; private set; }
        protected MatrixElement MOS3GbPtr { get; private set; }
        protected MatrixElement MOS3GdpPtr { get; private set; }
        protected MatrixElement MOS3GspPtr { get; private set; }
        protected MatrixElement MOS3SspPtr { get; private set; }
        protected MatrixElement MOS3BdpPtr { get; private set; }
        protected MatrixElement MOS3BspPtr { get; private set; }
        protected MatrixElement MOS3DPspPtr { get; private set; }
        protected MatrixElement MOS3DPdPtr { get; private set; }
        protected MatrixElement MOS3BgPtr { get; private set; }
        protected MatrixElement MOS3DPgPtr { get; private set; }
        protected MatrixElement MOS3SPgPtr { get; private set; }
        protected MatrixElement MOS3SPsPtr { get; private set; }
        protected MatrixElement MOS3DPbPtr { get; private set; }
        protected MatrixElement MOS3SPbPtr { get; private set; }
        protected MatrixElement MOS3SPdpPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
            mbp = provider.GetParameterSet<ModelBaseParameters>(1);

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>(0);
            load = provider.GetBehavior<LoadBehavior>(0);
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 4)
                throw new Diagnostics.CircuitException($"Pin count mismatch: 4 pins expected, {pins.Length} given");
            MOS3dNode = pins[0];
            MOS3gNode = pins[1];
            MOS3sNode = pins[2];
            MOS3bNode = pins[3];
        }

        /// <summary>
        /// Get matrix pionters
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            // Get extra equations
            MOS3dNodePrime = load.MOS3dNodePrime;
            MOS3sNodePrime = load.MOS3sNodePrime;

            // Get matrix pointers
            MOS3DdPtr = matrix.GetElement(MOS3dNode, MOS3dNode);
            MOS3GgPtr = matrix.GetElement(MOS3gNode, MOS3gNode);
            MOS3SsPtr = matrix.GetElement(MOS3sNode, MOS3sNode);
            MOS3BbPtr = matrix.GetElement(MOS3bNode, MOS3bNode);
            MOS3DPdpPtr = matrix.GetElement(MOS3dNodePrime, MOS3dNodePrime);
            MOS3SPspPtr = matrix.GetElement(MOS3sNodePrime, MOS3sNodePrime);
            MOS3DdpPtr = matrix.GetElement(MOS3dNode, MOS3dNodePrime);
            MOS3GbPtr = matrix.GetElement(MOS3gNode, MOS3bNode);
            MOS3GdpPtr = matrix.GetElement(MOS3gNode, MOS3dNodePrime);
            MOS3GspPtr = matrix.GetElement(MOS3gNode, MOS3sNodePrime);
            MOS3SspPtr = matrix.GetElement(MOS3sNode, MOS3sNodePrime);
            MOS3BdpPtr = matrix.GetElement(MOS3bNode, MOS3dNodePrime);
            MOS3BspPtr = matrix.GetElement(MOS3bNode, MOS3sNodePrime);
            MOS3DPspPtr = matrix.GetElement(MOS3dNodePrime, MOS3sNodePrime);
            MOS3DPdPtr = matrix.GetElement(MOS3dNodePrime, MOS3dNode);
            MOS3BgPtr = matrix.GetElement(MOS3bNode, MOS3gNode);
            MOS3DPgPtr = matrix.GetElement(MOS3dNodePrime, MOS3gNode);
            MOS3SPgPtr = matrix.GetElement(MOS3sNodePrime, MOS3gNode);
            MOS3SPsPtr = matrix.GetElement(MOS3sNodePrime, MOS3sNode);
            MOS3DPbPtr = matrix.GetElement(MOS3dNodePrime, MOS3bNode);
            MOS3SPbPtr = matrix.GetElement(MOS3sNodePrime, MOS3bNode);
            MOS3SPdpPtr = matrix.GetElement(MOS3sNodePrime, MOS3dNodePrime);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            MOS3DdPtr = null;
            MOS3GgPtr = null;
            MOS3SsPtr = null;
            MOS3BbPtr = null;
            MOS3DPdpPtr = null;
            MOS3SPspPtr = null;
            MOS3DdpPtr = null;
            MOS3GbPtr = null;
            MOS3GdpPtr = null;
            MOS3GspPtr = null;
            MOS3SspPtr = null;
            MOS3BdpPtr = null;
            MOS3BspPtr = null;
            MOS3DPspPtr = null;
            MOS3DPdPtr = null;
            MOS3BgPtr = null;
            MOS3DPgPtr = null;
            MOS3SPgPtr = null;
            MOS3SPsPtr = null;
            MOS3DPbPtr = null;
            MOS3SPbPtr = null;
            MOS3SPdpPtr = null;
        }

        /// <summary>
        /// Initialize AC parameters
        /// </summary>
        /// <param name="sim"></param>
        public override void InitializeParameters(FrequencySimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap,
                OxideCap, vgs, vds, vbs, vbd, vgb, vgd, von, vdsat,
                sargsw;

            vbs = load.MOS3vbs;
            vbd = load.MOS3vbd;
            vgs = load.MOS3vgs;
            vds = load.MOS3vds;
            vgd = vgs - vds;
            vgb = vgs - vbs;
            von = mbp.MOS3type * load.MOS3von;
            vdsat = mbp.MOS3type * load.MOS3vdsat;

            EffectiveLength = bp.MOS3l - 2 * mbp.MOS3latDiff;
            GateSourceOverlapCap = mbp.MOS3gateSourceOverlapCapFactor * bp.MOS3w;
            GateDrainOverlapCap = mbp.MOS3gateDrainOverlapCapFactor * bp.MOS3w;
            GateBulkOverlapCap = mbp.MOS3gateBulkOverlapCapFactor * EffectiveLength;
            OxideCap = modeltemp.MOS3oxideCapFactor * EffectiveLength * bp.MOS3w;

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
            if (vbs < temp.MOS3tDepCap)
            {
                double arg = 1 - vbs / temp.MOS3tBulkPot, sarg;
                /* 
                * the following block looks somewhat long and messy, 
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (mbp.MOS3bulkJctBotGradingCoeff.Value == mbp.MOS3bulkJctSideGradingCoeff)
                {
                    if (mbp.MOS3bulkJctBotGradingCoeff.Value == .5)
                    {
                        sarg = sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        sarg = sargsw = Math.Exp(-mbp.MOS3bulkJctBotGradingCoeff * Math.Log(arg));
                    }
                }
                else
                {
                    if (mbp.MOS3bulkJctBotGradingCoeff.Value == .5)
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-mbp.MOS3bulkJctBotGradingCoeff * Math.Log(arg));
                    }
                    if (mbp.MOS3bulkJctSideGradingCoeff.Value == .5)
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-mbp.MOS3bulkJctSideGradingCoeff * Math.Log(arg));
                    }
                }
                /* NOSQRT */
                MOS3capbs = temp.MOS3Cbs * sarg + temp.MOS3Cbssw * sargsw;
            }
            else
            {
                MOS3capbs = temp.MOS3f2s + temp.MOS3f3s * vbs;
            }

            if (vbd < temp.MOS3tDepCap)
            {
                double arg = 1 - vbd / temp.MOS3tBulkPot, sarg;
                /* 
                * the following block looks somewhat long and messy, 
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (mbp.MOS3bulkJctBotGradingCoeff.Value == .5 && mbp.MOS3bulkJctSideGradingCoeff.Value == .5)
                {
                    sarg = sargsw = 1 / Math.Sqrt(arg);
                }
                else
                {
                    if (mbp.MOS3bulkJctBotGradingCoeff.Value == .5)
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-mbp.MOS3bulkJctBotGradingCoeff * Math.Log(arg));
                    }
                    if (mbp.MOS3bulkJctSideGradingCoeff.Value == .5)
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-mbp.MOS3bulkJctSideGradingCoeff * Math.Log(arg));
                    }
                }
                /* NOSQRT */
                MOS3capbd = temp.MOS3Cbd * sarg + temp.MOS3Cbdsw * sargsw;
            }
            else
            {
                MOS3capbd = temp.MOS3f2d + vbd * temp.MOS3f3d;
            }
            /* CAPZEROBYPASS */

            /* (above only excludes tranop, since we're only at this
            * point if tran or tranop)
            */

            /* 
             * calculate meyer's capacitors
             */
            /* 
             * new cmeyer - this just evaluates at the current time, 
             * expects you to remember values from previous time
             * returns 1 / 2 of non - constant portion of capacitance
             * you must add in the other half from previous time
             * and the constant part
             */
            double icapgs, icapgd, icapgb;
            if (load.MOS3mode > 0)
            {
                Transistor.DEVqmeyer(vgs, vgd, vgb, von, vdsat,
                    out icapgs, out icapgd, out icapgb, temp.MOS3tPhi, OxideCap);
            }
            else
            {
                Transistor.DEVqmeyer(vgd, vgs, vgb, von, vdsat,
                    out icapgd, out icapgs, out icapgb, temp.MOS3tPhi, OxideCap);
            }
            MOS3capgs = icapgs;
            MOS3capgd = icapgd;
            MOS3capgb = icapgb;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            var state = sim.State;
            var cstate = state;
            int xnrm, xrev;
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, capgs, capgd, capgb, xgs, xgd, xgb, xbd,
                xbs;

            if (load.MOS3mode < 0)
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
			 * charge oriented model parameters
			 */
            EffectiveLength = bp.MOS3l - 2 * mbp.MOS3latDiff;
            GateSourceOverlapCap = mbp.MOS3gateSourceOverlapCapFactor * bp.MOS3w;
            GateDrainOverlapCap = mbp.MOS3gateDrainOverlapCapFactor * bp.MOS3w;
            GateBulkOverlapCap = mbp.MOS3gateBulkOverlapCapFactor * EffectiveLength;

            /* 
			 * meyer"s model parameters
			 */
            capgs = (MOS3capgs + MOS3capgs + GateSourceOverlapCap);
            capgd = (MOS3capgd + MOS3capgd + GateDrainOverlapCap);
            capgb = (MOS3capgb + MOS3capgb + GateBulkOverlapCap);
            xgs = capgs * cstate.Laplace.Imaginary;
            xgd = capgd * cstate.Laplace.Imaginary;
            xgb = capgb * cstate.Laplace.Imaginary;
            xbd = MOS3capbd * cstate.Laplace.Imaginary;
            xbs = MOS3capbs * cstate.Laplace.Imaginary;

            /* 
			 * load matrix
			 */
            MOS3GgPtr.Add(new Complex(0.0, xgd + xgs + xgb));
            MOS3BbPtr.Add(new Complex(load.MOS3gbd + load.MOS3gbs, xgb + xbd + xbs));
            MOS3DPdpPtr.Add(new Complex(temp.MOS3drainConductance + load.MOS3gds + load.MOS3gbd + xrev * (load.MOS3gm + load.MOS3gmbs), xgd + xbd));
            MOS3SPspPtr.Add(new Complex(temp.MOS3sourceConductance + load.MOS3gds + load.MOS3gbs + xnrm * (load.MOS3gm + load.MOS3gmbs), xgs + xbs));
            MOS3GbPtr.Sub(new Complex(0.0, xgb));
            MOS3GdpPtr.Sub(new Complex(0.0, xgd));
            MOS3GspPtr.Sub(new Complex(0.0, xgs));
            MOS3BgPtr.Sub(new Complex(0.0, xgb));
            MOS3BdpPtr.Sub(new Complex(load.MOS3gbd, xbd));
            MOS3BspPtr.Sub(new Complex(load.MOS3gbs, xbs));
            MOS3DPgPtr.Add(new Complex((xnrm - xrev) * load.MOS3gm, -xgd));
            MOS3DPbPtr.Add(new Complex(-load.MOS3gbd + (xnrm - xrev) * load.MOS3gmbs, -xbd));
            MOS3SPgPtr.Sub(new Complex((xnrm - xrev) * load.MOS3gm, xgs));
            MOS3SPbPtr.Sub(new Complex(load.MOS3gbs + (xnrm - xrev) * load.MOS3gmbs, xbs));
            MOS3DdPtr.Add(temp.MOS3drainConductance);
            MOS3SsPtr.Add(temp.MOS3sourceConductance);
            MOS3DdpPtr.Sub(temp.MOS3drainConductance);
            MOS3SspPtr.Sub(temp.MOS3sourceConductance);
            MOS3DPdPtr.Sub(temp.MOS3drainConductance);
            MOS3DPspPtr.Sub(load.MOS3gds + xnrm * (load.MOS3gm + load.MOS3gmbs));
            MOS3SPsPtr.Sub(temp.MOS3sourceConductance);
            MOS3SPdpPtr.Sub(load.MOS3gds + xrev * (load.MOS3gm + load.MOS3gmbs));
        }
    }
}
