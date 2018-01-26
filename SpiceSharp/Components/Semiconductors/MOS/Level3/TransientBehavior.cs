using System;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// Transient behavior for a <see cref="MOS3"/>
    /// </summary>
    public class TransientBehavior : Behaviors.TransientBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        TemperatureBehavior temp;
        LoadBehavior load;
        ModelTemperatureBehavior modeltemp;

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
        /// States
        /// </summary>
        StateHistory MOS3vgs;
        StateHistory MOS3vds;
        StateHistory MOS3vbs;
        StateHistory MOS3capgs;
        StateHistory MOS3capgd;
        StateHistory MOS3capgb;
        StateDerivative MOS3qgs;
        StateDerivative MOS3qgd;
        StateDerivative MOS3qgb;
        StateDerivative MOS3qbd;
        StateDerivative MOS3qbs;

        /// <summary>
        /// Shared parameters
        /// </summary>
        [PropertyName("cbd"), PropertyInfo("Bulk-Drain capacitance")]
        public double MOS3capbd { get; internal set; }
        [PropertyName("cbs"), PropertyInfo("Bulk-Source capacitance")]
        public double MOS3capbs { get; internal set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(Identifier name) : base(name) { }

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
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            // Get extra equations
            MOS3dNodePrime = load.MOS3dNodePrime;
            MOS3sNodePrime = load.MOS3sNodePrime;

            // Get matrix elements
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
        /// Create states
        /// </summary>
        /// <param name="states">States</param>
        public override void CreateStates(StatePool states)
        {
			if (states == null)
				throw new ArgumentNullException(nameof(states));

            MOS3vgs = states.CreateHistory();
            MOS3vds = states.CreateHistory();
            MOS3vbs = states.CreateHistory();
            MOS3capgs = states.CreateHistory();
            MOS3capgd = states.CreateHistory();
            MOS3capgb = states.CreateHistory();
            MOS3qgs = states.Create();
            MOS3qgd = states.Create();
            MOS3qgb = states.Create();
            MOS3qbd = states.Create();
            MOS3qbs = states.Create();
        }

        /// <summary>
        /// Get DC states
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void GetDCstate(TimeSimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap,
                OxideCap, vgs, vds, vbs, vbd, vgb, vgd, von, vdsat,
                sargsw, vgs1, vgd1, vgb1, capgs = 0.0, capgd = 0.0, capgb = 0.0;

            var state = sim.State;
            vbs = load.MOS3vbs;
            vgs = load.MOS3vgs;
            vds = load.MOS3vds;
            vbd = vbs - vds;
            vgd = vgs - vds;
            vgb = vgs - vbs;
            von = mbp.MOS3type * load.MOS3von;
            vdsat = mbp.MOS3type * load.MOS3vdsat;

            MOS3vgs.Value = vgs;
            MOS3vbs.Value = vbs;
            MOS3vds.Value = vds;

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
                MOS3qbs.Value = temp.MOS3tBulkPot * (temp.MOS3Cbs * (1 - arg * sarg) / (1 - mbp.MOS3bulkJctBotGradingCoeff) +
                    temp.MOS3Cbssw * (1 - arg * sargsw) / (1 - mbp.MOS3bulkJctSideGradingCoeff));
                MOS3capbs = temp.MOS3Cbs * sarg + temp.MOS3Cbssw * sargsw;
            }
            else
            {
                MOS3qbs.Value = temp.MOS3f4s + vbs * (temp.MOS3f2s + vbs * (temp.MOS3f3s / 2));
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
                MOS3qbd.Value = temp.MOS3tBulkPot * (temp.MOS3Cbd * (1 - arg * sarg) / (1 - mbp.MOS3bulkJctBotGradingCoeff) +
                    temp.MOS3Cbdsw * (1 - arg * sargsw) / (1 - mbp.MOS3bulkJctSideGradingCoeff));
                MOS3capbd = temp.MOS3Cbd * sarg + temp.MOS3Cbdsw * sargsw;
            }
            else
            {
                MOS3qbd.Value = temp.MOS3f4d + vbd * (temp.MOS3f2d + vbd * temp.MOS3f3d / 2);
                MOS3capbd = temp.MOS3f2d + vbd * temp.MOS3f3d;
            }
            /* CAPZEROBYPASS */

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
            MOS3capgs.Value = icapgs;
            MOS3capgd.Value = icapgd;
            MOS3capgb.Value = icapgb;
            vgs1 = MOS3vgs.GetPreviousValue(1);
            vgd1 = vgs1 - MOS3vds.GetPreviousValue(1);
            vgb1 = vgs1 - MOS3vbs.GetPreviousValue(1);
            capgs = 2 * MOS3capgs.Value + GateSourceOverlapCap;
            capgd = 2 * MOS3capgd.Value + GateDrainOverlapCap;
            capgb = 2 * MOS3capgb.Value + GateBulkOverlapCap;

            /* DETAILPROF */
            /* 
             * store small - signal parameters (for meyer's model)
             * all parameters already stored, so done...
             */

            /* TRANOP only */
            MOS3qgs.Value = vgs * capgs;
            MOS3qgd.Value = vgd * capgd;
            MOS3qgb.Value = vgb * capgb;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Transient(TimeSimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            var state = sim.State;
            var rstate = state;
            var method = sim.Method;
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap,
                OxideCap, vgs, vds, vbs, vbd, vgb, vgd, von, vdsat,
                sargsw, vgs1, vgd1, vgb1, capgs = 0.0, capgd = 0.0, capgb = 0.0, gcgs, ceqgs, gcgd, ceqgd, gcgb, ceqgb, ceqbs, ceqbd;

            vbs = load.MOS3vbs;
            vbd = load.MOS3vbd;
            vgs = load.MOS3vgs;
            vds = load.MOS3vds;
            vgd = load.MOS3vgs - load.MOS3vds;
            vgb = load.MOS3vgs - load.MOS3vbs;
            von = mbp.MOS3type * load.MOS3von;
            vdsat = mbp.MOS3type * load.MOS3vdsat;

            MOS3vds.Value = vds;
            MOS3vbs.Value = vbs;
            MOS3vgs.Value = vgs;

            double MOS3gbd = 0.0;
            double MOS3cbd = 0.0;
            double MOS3cd = 0.0;
            double MOS3gbs = 0.0;
            double MOS3cbs = 0.0;

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
                MOS3qbs.Value = temp.MOS3tBulkPot * (temp.MOS3Cbs * (1 - arg * sarg) / (1 - mbp.MOS3bulkJctBotGradingCoeff) +
                    temp.MOS3Cbssw * (1 - arg * sargsw) / (1 - mbp.MOS3bulkJctSideGradingCoeff));
                MOS3capbs = temp.MOS3Cbs * sarg + temp.MOS3Cbssw * sargsw;
            }
            else
            {
                MOS3qbs.Value = temp.MOS3f4s + vbs * (temp.MOS3f2s + vbs * (temp.MOS3f3s / 2));
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
                MOS3qbd.Value = temp.MOS3tBulkPot * (temp.MOS3Cbd * (1 - arg * sarg) / (1 - mbp.MOS3bulkJctBotGradingCoeff) +
                    temp.MOS3Cbdsw * (1 - arg * sargsw) / (1 - mbp.MOS3bulkJctSideGradingCoeff));
                MOS3capbd = temp.MOS3Cbd * sarg + temp.MOS3Cbdsw * sargsw;
            }
            else
            {
                MOS3qbd.Value = temp.MOS3f4d + vbd * (temp.MOS3f2d + vbd * temp.MOS3f3d / 2);
                MOS3capbd = temp.MOS3f2d + vbd * temp.MOS3f3d;
            }
            /* CAPZEROBYPASS */

            /* (above only excludes tranop, since we're only at this
            * point if tran or tranop)
            */

            /* 
            * calculate equivalent conductances and currents for
            * depletion capacitors
            */

            /* integrate the capacitors and save results */
            MOS3qbd.Integrate();
            MOS3gbd += MOS3qbd.Jacobian(MOS3capbd);
            MOS3cbd += MOS3qbd.Derivative;
            MOS3cd -= MOS3qbd.Derivative;
            MOS3qbs.Integrate();
            MOS3gbs += MOS3qbs.Jacobian(MOS3capbs);
            MOS3cbs += MOS3qbs.Derivative;

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
            MOS3capgs.Value = icapgs;
            MOS3capgd.Value = icapgd;
            MOS3capgb.Value = icapgb;

            vgs1 = MOS3vgs.GetPreviousValue(1);
            vgd1 = vgs1 - MOS3vds.GetPreviousValue(1);
            vgb1 = vgs1 - MOS3vbs.GetPreviousValue(1);
            capgs = (MOS3capgs.Value + MOS3capgs.GetPreviousValue(1) + GateSourceOverlapCap);
            capgd = (MOS3capgd.Value + MOS3capgd.GetPreviousValue(1) + GateDrainOverlapCap);
            capgb = (MOS3capgb.Value + MOS3capgb.GetPreviousValue(1) + GateBulkOverlapCap);

            MOS3qgs.Value = (vgs - vgs1) * capgs + MOS3qgs.GetPreviousValue(1);
            MOS3qgd.Value = (vgd - vgd1) * capgd + MOS3qgd.GetPreviousValue(1);
            MOS3qgb.Value = (vgb - vgb1) * capgb + MOS3qgb.GetPreviousValue(1);

            /* 
             * calculate equivalent conductances and currents for
             * meyer"s capacitors
             */
            MOS3qgs.Integrate();
            gcgs = MOS3qgs.Jacobian(capgs);
            ceqgs = MOS3qgs.Current(gcgs, vgs);
            MOS3qgd.Integrate();
            gcgd = MOS3qgd.Jacobian(capgd);
            ceqgd = MOS3qgd.Current(gcgd, vgd);
            MOS3qgb.Integrate();
            gcgb = MOS3qgb.Jacobian(capgb);
            ceqgb = MOS3qgb.Current(gcgb, vgb);

            /* 
             * load current vector
             */
            ceqbs = mbp.MOS3type * (MOS3cbs - MOS3gbs * vbs);
            ceqbd = mbp.MOS3type * (MOS3cbd - MOS3gbd * vbd);
            rstate.Rhs[MOS3gNode] -= (mbp.MOS3type * (ceqgs + ceqgb + ceqgd));
            rstate.Rhs[MOS3bNode] -= (ceqbs + ceqbd - mbp.MOS3type * ceqgb);
            rstate.Rhs[MOS3dNodePrime] += (ceqbd + mbp.MOS3type * ceqgd);
            rstate.Rhs[MOS3sNodePrime] += ceqbs + mbp.MOS3type * ceqgs;

            /* 
			 * load y matrix
			 */
            MOS3GgPtr.Add(gcgd + gcgs + gcgb);
            MOS3BbPtr.Add(MOS3gbd + MOS3gbs + gcgb);
            MOS3DPdpPtr.Add(MOS3gbd + gcgd);
            MOS3SPspPtr.Add(MOS3gbs + gcgs);
            MOS3GbPtr.Sub(gcgb);
            MOS3GdpPtr.Sub(gcgd);
            MOS3GspPtr.Sub(gcgs);
            MOS3BgPtr.Sub(gcgb);
            MOS3BdpPtr.Sub(MOS3gbd);
            MOS3BspPtr.Sub(MOS3gbs);
            MOS3DPgPtr.Add(-gcgd);
            MOS3DPbPtr.Add(-MOS3gbd);
            MOS3SPgPtr.Add(-gcgs);
            MOS3SPbPtr.Add(-MOS3gbs);
        }

        /// <summary>
        /// Truncate timestep
        /// </summary>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(ref double timestep)
        {
            MOS3qgs.LocalTruncationError(ref timestep);
            MOS3qgd.LocalTruncationError(ref timestep);
            MOS3qgb.LocalTruncationError(ref timestep);
        }
    }
}
