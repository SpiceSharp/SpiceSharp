using System;
using System.Numerics;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// AC behavior for <see cref="MOS3"/>
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
        public double Capbs { get; protected set; }
        public double Capbd { get; protected set; }
        public double Capgs { get; protected set; }
        public double Capgd { get; protected set; }
        public double Capgb { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int dNode, gNode, sNode, bNode, dNodePrime, sNodePrime;
        protected MatrixElement DdPtr { get; private set; }
        protected MatrixElement GgPtr { get; private set; }
        protected MatrixElement SsPtr { get; private set; }
        protected MatrixElement BbPtr { get; private set; }
        protected MatrixElement DPdpPtr { get; private set; }
        protected MatrixElement SPspPtr { get; private set; }
        protected MatrixElement DdpPtr { get; private set; }
        protected MatrixElement GbPtr { get; private set; }
        protected MatrixElement GdpPtr { get; private set; }
        protected MatrixElement GspPtr { get; private set; }
        protected MatrixElement SspPtr { get; private set; }
        protected MatrixElement BdpPtr { get; private set; }
        protected MatrixElement BspPtr { get; private set; }
        protected MatrixElement DPspPtr { get; private set; }
        protected MatrixElement DPdPtr { get; private set; }
        protected MatrixElement BgPtr { get; private set; }
        protected MatrixElement DPgPtr { get; private set; }
        protected MatrixElement SPgPtr { get; private set; }
        protected MatrixElement SPsPtr { get; private set; }
        protected MatrixElement DPbPtr { get; private set; }
        protected MatrixElement SPbPtr { get; private set; }
        protected MatrixElement SPdpPtr { get; private set; }

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
            dNode = pins[0];
            gNode = pins[1];
            sNode = pins[2];
            bNode = pins[3];
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
            dNodePrime = load.DrainNodePrime;
            sNodePrime = load.SourceNodePrime;

            // Get matrix pointers
            DdPtr = matrix.GetElement(dNode, dNode);
            GgPtr = matrix.GetElement(gNode, gNode);
            SsPtr = matrix.GetElement(sNode, sNode);
            BbPtr = matrix.GetElement(bNode, bNode);
            DPdpPtr = matrix.GetElement(dNodePrime, dNodePrime);
            SPspPtr = matrix.GetElement(sNodePrime, sNodePrime);
            DdpPtr = matrix.GetElement(dNode, dNodePrime);
            GbPtr = matrix.GetElement(gNode, bNode);
            GdpPtr = matrix.GetElement(gNode, dNodePrime);
            GspPtr = matrix.GetElement(gNode, sNodePrime);
            SspPtr = matrix.GetElement(sNode, sNodePrime);
            BdpPtr = matrix.GetElement(bNode, dNodePrime);
            BspPtr = matrix.GetElement(bNode, sNodePrime);
            DPspPtr = matrix.GetElement(dNodePrime, sNodePrime);
            DPdPtr = matrix.GetElement(dNodePrime, dNode);
            BgPtr = matrix.GetElement(bNode, gNode);
            DPgPtr = matrix.GetElement(dNodePrime, gNode);
            SPgPtr = matrix.GetElement(sNodePrime, gNode);
            SPsPtr = matrix.GetElement(sNodePrime, sNode);
            DPbPtr = matrix.GetElement(dNodePrime, bNode);
            SPbPtr = matrix.GetElement(sNodePrime, bNode);
            SPdpPtr = matrix.GetElement(sNodePrime, dNodePrime);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            DdPtr = null;
            GgPtr = null;
            SsPtr = null;
            BbPtr = null;
            DPdpPtr = null;
            SPspPtr = null;
            DdpPtr = null;
            GbPtr = null;
            GdpPtr = null;
            GspPtr = null;
            SspPtr = null;
            BdpPtr = null;
            BspPtr = null;
            DPspPtr = null;
            DPdPtr = null;
            BgPtr = null;
            DPgPtr = null;
            SPgPtr = null;
            SPsPtr = null;
            DPbPtr = null;
            SPbPtr = null;
            SPdpPtr = null;
        }

        /// <summary>
        /// Initialize AC parameters
        /// </summary>
        /// <param name="simulation"></param>
        public override void InitializeParameters(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap,
                OxideCap, vgs, vds, vbs, vbd, vgb, vgd, von, vdsat,
                sargsw;

            vbs = load.Vbs;
            vbd = load.Vbd;
            vgs = load.Vgs;
            vds = load.Vds;
            vgd = vgs - vds;
            vgb = vgs - vbs;
            von = mbp.Type * load.Von;
            vdsat = mbp.Type * load.Vdsat;

            EffectiveLength = bp.Length - 2 * mbp.LatDiff;
            GateSourceOverlapCap = mbp.GateSourceOverlapCapFactor * bp.Width;
            GateDrainOverlapCap = mbp.GateDrainOverlapCapFactor * bp.Width;
            GateBulkOverlapCap = mbp.GateBulkOverlapCapFactor * EffectiveLength;
            OxideCap = modeltemp.OxideCapFactor * EffectiveLength * bp.Width;

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
            if (vbs < temp.TDepCap)
            {
                double arg = 1 - vbs / temp.TBulkPot, sarg;
                /* 
                * the following block looks somewhat long and messy, 
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (mbp.BulkJctBotGradingCoeff.Value == mbp.BulkJctSideGradingCoeff)
                {
                    if (mbp.BulkJctBotGradingCoeff.Value == .5)
                    {
                        sarg = sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        sarg = sargsw = Math.Exp(-mbp.BulkJctBotGradingCoeff * Math.Log(arg));
                    }
                }
                else
                {
                    if (mbp.BulkJctBotGradingCoeff.Value == .5)
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-mbp.BulkJctBotGradingCoeff * Math.Log(arg));
                    }
                    if (mbp.BulkJctSideGradingCoeff.Value == .5)
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-mbp.BulkJctSideGradingCoeff * Math.Log(arg));
                    }
                }
                /* NOSQRT */
                Capbs = temp.Cbs * sarg + temp.Cbssw * sargsw;
            }
            else
            {
                Capbs = temp.F2s + temp.F3s * vbs;
            }

            if (vbd < temp.TDepCap)
            {
                double arg = 1 - vbd / temp.TBulkPot, sarg;
                /* 
                * the following block looks somewhat long and messy, 
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (mbp.BulkJctBotGradingCoeff.Value == .5 && mbp.BulkJctSideGradingCoeff.Value == .5)
                {
                    sarg = sargsw = 1 / Math.Sqrt(arg);
                }
                else
                {
                    if (mbp.BulkJctBotGradingCoeff.Value == .5)
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-mbp.BulkJctBotGradingCoeff * Math.Log(arg));
                    }
                    if (mbp.BulkJctSideGradingCoeff.Value == .5)
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-mbp.BulkJctSideGradingCoeff * Math.Log(arg));
                    }
                }
                /* NOSQRT */
                Capbd = temp.Cbd * sarg + temp.Cbdsw * sargsw;
            }
            else
            {
                Capbd = temp.F2d + vbd * temp.F3d;
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
            if (load.Mode > 0)
            {
                Transistor.DEVqmeyer(vgs, vgd, vgb, von, vdsat,
                    out icapgs, out icapgd, out icapgb, temp.TPhi, OxideCap);
            }
            else
            {
                Transistor.DEVqmeyer(vgd, vgs, vgb, von, vdsat,
                    out icapgd, out icapgs, out icapgb, temp.TPhi, OxideCap);
            }
            Capgs = icapgs;
            Capgd = icapgd;
            Capgb = icapgb;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.State;
            var cstate = state;
            int xnrm, xrev;
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap, capgs, capgd, capgb, xgs, xgd, xgb, xbd,
                xbs;

            if (load.Mode < 0)
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
            EffectiveLength = bp.Length - 2 * mbp.LatDiff;
            GateSourceOverlapCap = mbp.GateSourceOverlapCapFactor * bp.Width;
            GateDrainOverlapCap = mbp.GateDrainOverlapCapFactor * bp.Width;
            GateBulkOverlapCap = mbp.GateBulkOverlapCapFactor * EffectiveLength;

            /* 
			 * meyer"s model parameters
			 */
            capgs = (Capgs + Capgs + GateSourceOverlapCap);
            capgd = (Capgd + Capgd + GateDrainOverlapCap);
            capgb = (Capgb + Capgb + GateBulkOverlapCap);
            xgs = capgs * cstate.Laplace.Imaginary;
            xgd = capgd * cstate.Laplace.Imaginary;
            xgb = capgb * cstate.Laplace.Imaginary;
            xbd = Capbd * cstate.Laplace.Imaginary;
            xbs = Capbs * cstate.Laplace.Imaginary;

            /* 
			 * load matrix
			 */
            GgPtr.Add(new Complex(0.0, xgd + xgs + xgb));
            BbPtr.Add(new Complex(load.Gbd + load.Gbs, xgb + xbd + xbs));
            DPdpPtr.Add(new Complex(temp.DrainConductance + load.Gds + load.Gbd + xrev * (load.Gm + load.Gmbs), xgd + xbd));
            SPspPtr.Add(new Complex(temp.SourceConductance + load.Gds + load.Gbs + xnrm * (load.Gm + load.Gmbs), xgs + xbs));
            GbPtr.Sub(new Complex(0.0, xgb));
            GdpPtr.Sub(new Complex(0.0, xgd));
            GspPtr.Sub(new Complex(0.0, xgs));
            BgPtr.Sub(new Complex(0.0, xgb));
            BdpPtr.Sub(new Complex(load.Gbd, xbd));
            BspPtr.Sub(new Complex(load.Gbs, xbs));
            DPgPtr.Add(new Complex((xnrm - xrev) * load.Gm, -xgd));
            DPbPtr.Add(new Complex(-load.Gbd + (xnrm - xrev) * load.Gmbs, -xbd));
            SPgPtr.Sub(new Complex((xnrm - xrev) * load.Gm, xgs));
            SPbPtr.Sub(new Complex(load.Gbs + (xnrm - xrev) * load.Gmbs, xbs));
            DdPtr.Add(temp.DrainConductance);
            SsPtr.Add(temp.SourceConductance);
            DdpPtr.Sub(temp.DrainConductance);
            SspPtr.Sub(temp.SourceConductance);
            DPdPtr.Sub(temp.DrainConductance);
            DPspPtr.Sub(load.Gds + xnrm * (load.Gm + load.Gmbs));
            SPsPtr.Sub(temp.SourceConductance);
            SPdpPtr.Sub(load.Gds + xrev * (load.Gm + load.Gmbs));
        }
    }
}
