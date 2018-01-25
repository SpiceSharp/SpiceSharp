using System;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Sparse;
using SpiceSharp.Components.Mosfet.Level1;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Behaviors.Mosfet.Level1
{
    /// <summary>
    /// Transient behavior for a <see cref="Components.MOS1"/>
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
        /// Shared parameters
        /// </summary>
        [PropertyName("cbd"), PropertyInfo("Bulk-Drain capacitance")]
        public double MOS1capbd { get; protected set; }
        [PropertyName("cbs"), PropertyInfo("Bulk-Source capacitance")]
        public double MOS1capbs { get; protected set; }

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
        /// State variables
        /// </summary>
        protected StateDerivative MOS1qgs { get; private set; }
        protected StateDerivative MOS1qgd { get; private set; }
        protected StateDerivative MOS1qgb { get; private set; }
        protected StateDerivative MOS1qbd { get; private set; }
        protected StateDerivative MOS1qbs { get; private set; }
        protected StateHistory MOS1capgs { get; private set; }
        protected StateHistory MOS1capgd { get; private set; }
        protected StateHistory MOS1capgb { get; private set; }
        protected StateHistory MOS1vgs { get; private set; }
        protected StateHistory MOS1vds { get; private set; }
        protected StateHistory MOS1vbs { get; private set; }

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
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

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
        /// Create states
        /// </summary>
        /// <param name="states">States</param>
        public override void CreateStates(StatePool states)
        {
			if (states == null)
				throw new ArgumentNullException(nameof(states));

            MOS1qgs = states.Create();
            MOS1qgd = states.Create();
            MOS1qgb = states.Create();
            MOS1qbd = states.Create();
            MOS1qbs = states.Create();

            MOS1capgs = states.CreateHistory();
            MOS1capgd = states.CreateHistory();
            MOS1capgb = states.CreateHistory();
            MOS1vgs = states.CreateHistory();
            MOS1vds = states.CreateHistory();
            MOS1vbs = states.CreateHistory();
        }

        /// <summary>
        /// Calculate DC state variables
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void GetDCstate(TimeSimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            var state = sim.State;
            double arg, sarg, sargsw;
            double capgs, capgd, capgb;

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
            
            if (vbs < temp.MOS1tDepCap)
            {
                arg = 1 - vbs / temp.MOS1tBulkPot;
                if (mbp.MOS1bulkJctBotGradingCoeff.Value == mbp.MOS1bulkJctSideGradingCoeff.Value)
                {
                    if (mbp.MOS1bulkJctBotGradingCoeff.Value == .5)
                        sarg = sargsw = 1 / Math.Sqrt(arg);
                    else
                        sarg = sargsw = Math.Exp(-mbp.MOS1bulkJctBotGradingCoeff * Math.Log(arg));
                }
                else
                {
                    if (mbp.MOS1bulkJctBotGradingCoeff.Value == .5)
                        sarg = 1 / Math.Sqrt(arg);
                    else
                        sarg = Math.Exp(-mbp.MOS1bulkJctBotGradingCoeff * Math.Log(arg));
                    if (mbp.MOS1bulkJctSideGradingCoeff.Value == .5)
                        sargsw = 1 / Math.Sqrt(arg);
                    else
                        sargsw = Math.Exp(-mbp.MOS1bulkJctSideGradingCoeff * Math.Log(arg));
                }
                MOS1qbs.Value = temp.MOS1tBulkPot * (temp.MOS1Cbs * (1 - arg * sarg) / (1 - mbp.MOS1bulkJctBotGradingCoeff) +
                    temp.MOS1Cbssw * (1 - arg * sargsw) / (1 - mbp.MOS1bulkJctSideGradingCoeff));
            }
            else
                MOS1qbs.Value = temp.MOS1f4s + vbs * (temp.MOS1f2s + vbs * (temp.MOS1f3s / 2));

            if (vbd < temp.MOS1tDepCap)
            {
                arg = 1 - vbd / temp.MOS1tBulkPot;
                if (mbp.MOS1bulkJctBotGradingCoeff.Value == .5 && mbp.MOS1bulkJctSideGradingCoeff.Value == .5)
                    sarg = sargsw = 1 / Math.Sqrt(arg);
                else
                {
                    if (mbp.MOS1bulkJctBotGradingCoeff.Value == .5)
                        sarg = 1 / Math.Sqrt(arg);
                    else
                        sarg = Math.Exp(-mbp.MOS1bulkJctBotGradingCoeff * Math.Log(arg));
                    if (mbp.MOS1bulkJctSideGradingCoeff.Value == .5)
                        sargsw = 1 / Math.Sqrt(arg);
                    else
                        sargsw = Math.Exp(-mbp.MOS1bulkJctSideGradingCoeff * Math.Log(arg));
                }
                MOS1qbd.Value = temp.MOS1tBulkPot * (temp.MOS1Cbd * (1 - arg * sarg) / (1 - mbp.MOS1bulkJctBotGradingCoeff) +
                    temp.MOS1Cbdsw * (1 - arg * sargsw) / (1 - mbp.MOS1bulkJctSideGradingCoeff));
            }
            else
                MOS1qbd.Value = temp.MOS1f4d + vbd * (temp.MOS1f2d + vbd * temp.MOS1f3d / 2);

            /* 
             * calculate meyer's capacitors
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
            MOS1capgs.Value = icapgs;
            MOS1capgd.Value = icapgd;
            MOS1capgb.Value = icapgb;
            capgs = 2 * MOS1capgs.Value + GateSourceOverlapCap;
            capgd = 2 * MOS1capgd.Value + GateDrainOverlapCap;
            capgb = 2 * MOS1capgb.Value + GateBulkOverlapCap;

            /* TRANOP only */
            MOS1qgs.Value = vgs * capgs;
            MOS1qgd.Value = vgd * capgd;
            MOS1qgb.Value = vgb * capgb;

            // Store these voltages
            MOS1vgs.Value = vgs;
            MOS1vds.Value = vds;
            MOS1vbs.Value = vbs;
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
            double arg, sarg, sargsw;
            double vgs1, vgd1, vgb1, capgs, capgd, capgb;

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

            double MOS1gbd = 0.0;
            double MOS1cbd = 0.0;
            double MOS1cd = 0.0;
            double MOS1gbs = 0.0;
            double MOS1cbs = 0.0;

            // Store these voltages
            MOS1vgs.Value = vgs;
            MOS1vds.Value = vds;
            MOS1vbs.Value = vbs;

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

                /* NOSQRT */
                MOS1qbs.Value = temp.MOS1tBulkPot * (temp.MOS1Cbs * (1 - arg * sarg) / (1 - mbp.MOS1bulkJctBotGradingCoeff) +
                    temp.MOS1Cbssw * (1 - arg * sargsw) / (1 - mbp.MOS1bulkJctSideGradingCoeff));
                MOS1capbs = temp.MOS1Cbs * sarg + temp.MOS1Cbssw * sargsw;
            }
            else
            {
                MOS1qbs.Value = temp.MOS1f4s + vbs * (temp.MOS1f2s + vbs * (temp.MOS1f3s / 2));
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
                MOS1qbd.Value = temp.MOS1tBulkPot * (temp.MOS1Cbd * (1 - arg * sarg) / (1 - mbp.MOS1bulkJctBotGradingCoeff) +
                    temp.MOS1Cbdsw * (1 - arg * sargsw) / (1 - mbp.MOS1bulkJctSideGradingCoeff));
                MOS1capbd = temp.MOS1Cbd * sarg + temp.MOS1Cbdsw * sargsw;
            }
            else
            {
                MOS1qbd.Value = temp.MOS1f4d + vbd * (temp.MOS1f2d + vbd * temp.MOS1f3d / 2);
                MOS1capbd = temp.MOS1f2d + vbd * temp.MOS1f3d;
            }

            // integrate the capacitors and save results
            MOS1qbd.Integrate();
            MOS1gbd += MOS1qbd.Jacobian(MOS1capbd);
            MOS1cbd += MOS1qbd.Derivative;
            MOS1cd -= MOS1qbd.Derivative;
            // NOTE: The derivative of MOS1qbd should be added to MOS1cd (drain current). Figure out a way later.
            MOS1qbs.Integrate();
            MOS1gbs += MOS1qbs.Jacobian(MOS1capbs);
            MOS1cbs += MOS1qbs.Derivative;

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
            MOS1capgs.Value = icapgs;
            MOS1capgd.Value = icapgd;
            MOS1capgb.Value = icapgb;
            vgs1 = MOS1vgs.GetPreviousValue(1);
            vgd1 = vgs1 - MOS1vds.GetPreviousValue(1);
            vgb1 = vgs1 - MOS1vbs.GetPreviousValue(1);
            capgs = MOS1capgs.Value + MOS1capgs.GetPreviousValue(1) + GateSourceOverlapCap;
            capgd = MOS1capgd.Value + MOS1capgd.GetPreviousValue(1) + GateDrainOverlapCap;
            capgb = MOS1capgb.Value + MOS1capgb.GetPreviousValue(1) + GateBulkOverlapCap;

            MOS1qgs.Value = (vgs - vgs1) * capgs + MOS1qgs.GetPreviousValue(1);
            MOS1qgd.Value = (vgd - vgd1) * capgd + MOS1qgd.GetPreviousValue(1);
            MOS1qgb.Value = (vgb - vgb1) * capgb + MOS1qgb.GetPreviousValue(1);


            /* NOTE: We can't reset derivatives!
            if (capgs == 0)
                state.States[0][MOS1states + MOS1cqgs] = 0;
            if (capgd == 0)
                state.States[0][MOS1states + MOS1cqgd] = 0;
            if (capgb == 0)
                state.States[0][MOS1states + MOS1cqgb] = 0;
            */

            /* NOTE: The formula with the method.Slope is to make it work for nonlinear capacitances!
             * The correct formula is: ceq = dQ/dt - geq * vq where geq = slope * dQ/dvq
             * The formula in Spice 3f5 is: ceq = dQ/dt - slope * Q where it assumes a linear capacitance
            method.Integrate(state, out gcgs, out ceqgs, MOS1states + MOS1qgs, capgs);
            method.Integrate(state, out gcgd, out ceqgd, MOS1states + MOS1qgd, capgd);
            method.Integrate(state, out gcgb, out ceqgb, MOS1states + MOS1qgb, capgb);
            ceqgs = ceqgs - gcgs * vgs + method.Slope * state.States[0][MOS1states + MOS1qgs];
            ceqgd = ceqgd - gcgd * vgd + method.Slope * state.States[0][MOS1states + MOS1qgd];
            ceqgb = ceqgb - gcgb * vgb + method.Slope * state.States[0][MOS1states + MOS1qgb];
            */

            MOS1qgs.Integrate();
            double gcgs = MOS1qgs.Jacobian(capgs);
            double ceqgs = MOS1qgs.Current(gcgs, vgs);
            MOS1qgd.Integrate();
            double gcgd = MOS1qgd.Jacobian(capgd);
            double ceqgd = MOS1qgd.Current(gcgd, vgd);
            MOS1qgb.Integrate();
            double gcgb = MOS1qgb.Jacobian(capgb);
            double ceqgb = MOS1qgb.Current(gcgb, vgb);

            /* 
			 * load current vector
			 */
            double ceqbs = mbp.MOS1type * (MOS1cbs - MOS1gbs * vbs);
            double ceqbd = mbp.MOS1type * (MOS1cbd - MOS1gbd * vbd);
            state.Rhs[MOS1gNode] -= mbp.MOS1type * (ceqgs + ceqgb + ceqgd);
            state.Rhs[MOS1bNode] -= ceqbs + ceqbd - mbp.MOS1type * ceqgb;
            state.Rhs[MOS1dNodePrime] += ceqbd + mbp.MOS1type * ceqgd;
            state.Rhs[MOS1sNodePrime] += ceqbs + mbp.MOS1type * ceqgs;

            /* 
			 * load y matrix
			 */
            MOS1GgPtr.Add(gcgd + gcgs + gcgb);
            MOS1BbPtr.Add((MOS1gbd + MOS1gbs + gcgb));
            MOS1DPdpPtr.Add(MOS1gbd + gcgd);
            MOS1SPspPtr.Add(MOS1gbs + gcgs);
            MOS1GbPtr.Sub(gcgb);
            MOS1GdpPtr.Sub(gcgd);
            MOS1GspPtr.Sub(gcgs);
            MOS1BgPtr.Sub(gcgb);
            MOS1BdpPtr.Sub(MOS1gbd);
            MOS1BspPtr.Sub(MOS1gbs);
            MOS1DPgPtr.Sub(gcgd);
            MOS1DPbPtr.Sub(MOS1gbd);
            MOS1SPgPtr.Sub(gcgs);
            MOS1SPbPtr.Sub(MOS1gbs);
        }

        /// <summary>
        /// Truncate timestep
        /// </summary>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(ref double timestep)
        {
            MOS1qgs.LocalTruncationError(ref timestep);
            MOS1qgd.LocalTruncationError(ref timestep);
            MOS1qgb.LocalTruncationError(ref timestep);
        }
    }
}
