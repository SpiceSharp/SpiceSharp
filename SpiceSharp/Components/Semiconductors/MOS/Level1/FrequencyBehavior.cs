using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// AC behavior for a <see cref="Mosfet1"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        TemperatureBehavior temp;
        LoadBehavior load;
        ModelTemperatureBehavior modeltemp;

        public double CapGS { get; protected set; }
        public double CapGD { get; protected set; }
        public double CapBS { get; protected set; }
        public double CapBD { get; protected set; }
        public double CapGB { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int drainNode, gateNode, sourceNode, bulkNode, sourceNodePrime, drainNodePrime;
        protected MatrixElement<Complex> DrainDrainPtr { get; private set; }
        protected MatrixElement<Complex> GateGatePtr { get; private set; }
        protected MatrixElement<Complex> SourceSourcePtr { get; private set; }
        protected MatrixElement<Complex> BulkBulkPtr { get; private set; }
        protected MatrixElement<Complex> DrainPrimeDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> SourcePrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> DrainDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> GateBulkPtr { get; private set; }
        protected MatrixElement<Complex> GateDrainPrimePtrPtr { get; private set; }
        protected MatrixElement<Complex> GateSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> SourceSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> BulkDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> BulkSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> DrainPrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> DrainPrimeDrainPtr { get; private set; }
        protected MatrixElement<Complex> BulkGatePtr { get; private set; }
        protected MatrixElement<Complex> DrainPrimeGatePtr { get; private set; }
        protected MatrixElement<Complex> SourcePrimeGatePtr { get; private set; }
        protected MatrixElement<Complex> SourcePrimeSourcePtr { get; private set; }
        protected MatrixElement<Complex> DrainPrimeBulkPtr { get; private set; }
        protected MatrixElement<Complex> SourcePrimeBulkPtr { get; private set; }
        protected MatrixElement<Complex> SourcePrimeDrainPrimePtr { get; private set; }

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
            bp = provider.GetParameterSet<BaseParameters>("entity");
            mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            temp = provider.GetBehavior<TemperatureBehavior>("entity");
            load = provider.GetBehavior<LoadBehavior>("entity");
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
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
                throw new Diagnostics.CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            drainNode = pins[0];
            gateNode = pins[1];
            sourceNode = pins[2];
            bulkNode = pins[3];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Matrix</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get extra equations
            drainNodePrime = load.DrainNodePrime;
            sourceNodePrime = load.SourceNodePrime;

            // Get matrix pointers
            DrainDrainPtr = solver.GetMatrixElement(drainNode, drainNode);
            GateGatePtr = solver.GetMatrixElement(gateNode, gateNode);
            SourceSourcePtr = solver.GetMatrixElement(sourceNode, sourceNode);
            BulkBulkPtr = solver.GetMatrixElement(bulkNode, bulkNode);
            DrainPrimeDrainPrimePtr = solver.GetMatrixElement(drainNodePrime, drainNodePrime);
            SourcePrimeSourcePrimePtr = solver.GetMatrixElement(sourceNodePrime, sourceNodePrime);
            DrainDrainPrimePtr = solver.GetMatrixElement(drainNode, drainNodePrime);
            GateBulkPtr = solver.GetMatrixElement(gateNode, bulkNode);
            GateDrainPrimePtrPtr = solver.GetMatrixElement(gateNode, drainNodePrime);
            GateSourcePrimePtr = solver.GetMatrixElement(gateNode, sourceNodePrime);
            SourceSourcePrimePtr = solver.GetMatrixElement(sourceNode, sourceNodePrime);
            BulkDrainPrimePtr = solver.GetMatrixElement(bulkNode, drainNodePrime);
            BulkSourcePrimePtr = solver.GetMatrixElement(bulkNode, sourceNodePrime);
            DrainPrimeSourcePrimePtr = solver.GetMatrixElement(drainNodePrime, sourceNodePrime);
            DrainPrimeDrainPtr = solver.GetMatrixElement(drainNodePrime, drainNode);
            BulkGatePtr = solver.GetMatrixElement(bulkNode, gateNode);
            DrainPrimeGatePtr = solver.GetMatrixElement(drainNodePrime, gateNode);
            SourcePrimeGatePtr = solver.GetMatrixElement(sourceNodePrime, gateNode);
            SourcePrimeSourcePtr = solver.GetMatrixElement(sourceNodePrime, sourceNode);
            DrainPrimeBulkPtr = solver.GetMatrixElement(drainNodePrime, bulkNode);
            SourcePrimeBulkPtr = solver.GetMatrixElement(sourceNodePrime, bulkNode);
            SourcePrimeDrainPrimePtr = solver.GetMatrixElement(sourceNodePrime, drainNodePrime);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            DrainDrainPtr = null;
            GateGatePtr = null;
            SourceSourcePtr = null;
            BulkBulkPtr = null;
            DrainPrimeDrainPrimePtr = null;
            SourcePrimeSourcePrimePtr = null;
            DrainDrainPrimePtr = null;
            GateBulkPtr = null;
            GateDrainPrimePtrPtr = null;
            GateSourcePrimePtr = null;
            SourceSourcePrimePtr = null;
            BulkDrainPrimePtr = null;
            BulkSourcePrimePtr = null;
            DrainPrimeSourcePrimePtr = null;
            DrainPrimeDrainPtr = null;
            BulkGatePtr = null;
            DrainPrimeGatePtr = null;
            SourcePrimeGatePtr = null;
            SourcePrimeSourcePtr = null;
            DrainPrimeBulkPtr = null;
            SourcePrimeBulkPtr = null;
            SourcePrimeDrainPrimePtr = null;
        }

        /// <summary>
        /// Initialize AC parameters
        /// </summary>
        /// <param name="simulation"></param>
        public override void InitializeParameters(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double arg, sarg, sargsw;

            // Get voltages
            double vbd = load.VoltageBD;
            double vbs = load.VoltageBS;
            double vgs = load.VoltageGS;
            double vds = load.VoltageDS;
            double vgd = vgs - vds;

            double EffectiveLength = bp.Length - 2 * mbp.LateralDiffusion;
            double OxideCap = modeltemp.OxideCapFactor * EffectiveLength * bp.Width;

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
            if (vbs < temp.TempDepletionCap)
            {
                arg = 1 - vbs / temp.TempBulkPotential;
                /* 
                 * the following block looks somewhat long and messy, 
                 * but since most users use the default grading
                 * coefficients of .5, and sqrt is MUCH faster than an
                 * Math.Exp(Math.Log()) we use this special case code to buy time.
                 * (as much as 10% of total job time!)
                 */
                if (mbp.BulkJunctionBotGradingCoefficient.Value == mbp.BulkJunctionSideGradingCoefficient.Value)
                {
                    if (mbp.BulkJunctionBotGradingCoefficient.Value == .5)
                    {
                        sarg = sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        sarg = sargsw = Math.Exp(-mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    }
                }
                else
                {
                    if (mbp.BulkJunctionBotGradingCoefficient.Value == .5)
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    }
                    if (mbp.BulkJunctionSideGradingCoefficient.Value == .5)
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-mbp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
                    }
                }

                CapBS = temp.CapBS * sarg + temp.CapBSSidewall * sargsw;
            }
            else
            {
                CapBS = temp.F2S + temp.F3S * vbs;
            }

            /* can't bypass the diode capacitance calculations */

            /* CAPZEROBYPASS */
            if (vbd < temp.TempDepletionCap)
            {
                arg = 1 - vbd / temp.TempBulkPotential;
                /* 
                * the following block looks somewhat long and messy, 
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (mbp.BulkJunctionBotGradingCoefficient.Value == .5 && mbp.BulkJunctionSideGradingCoefficient.Value == .5)
                {
                    sarg = sargsw = 1 / Math.Sqrt(arg);
                }
                else
                {
                    if (mbp.BulkJunctionBotGradingCoefficient.Value == .5)
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    }
                    if (mbp.BulkJunctionSideGradingCoefficient.Value == .5)
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-mbp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
                    }
                }
                /* NOSQRT */
                CapBD = temp.CapBD * sarg + temp.CapBDSidewall * sargsw;
            }
            else
            {
                CapBD = temp.F2D + vbd * temp.F3D;
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
            if (load.Mode > 0)
            {
                Transistor.MeyerCharges(vgs, vgd, mbp.MosfetType * load.Von, mbp.MosfetType * load.SaturationVoltageDS,
                    out icapgs, out icapgd, out icapgb,
                    temp.TempPhi, OxideCap);
            }
            else
            {
                Transistor.MeyerCharges(vgd, vgs, mbp.MosfetType * load.Von, mbp.MosfetType * load.SaturationVoltageDS,
                    out icapgd, out icapgs, out icapgb,
                    temp.TempPhi, OxideCap);
            }
            CapGS = icapgs;
            CapGD = icapgd;
            CapGB = icapgb;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var cstate = simulation.ComplexState;
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

            // Meyer's model parameters
            EffectiveLength = bp.Length - 2 * mbp.LateralDiffusion;
            GateSourceOverlapCap = mbp.GateSourceOverlapCapFactor * bp.Width;
            GateDrainOverlapCap = mbp.GateDrainOverlapCapFactor * bp.Width;
            GateBulkOverlapCap = mbp.GateBulkOverlapCapFactor * EffectiveLength;
            capgs = (CapGS + CapGS + GateSourceOverlapCap);
            capgd = (CapGD + CapGD + GateDrainOverlapCap);
            capgb = (CapGB + CapGB + GateBulkOverlapCap);
            xgs = capgs * cstate.Laplace.Imaginary;
            xgd = capgd * cstate.Laplace.Imaginary;
            xgb = capgb * cstate.Laplace.Imaginary;
            xbd = CapBD * cstate.Laplace.Imaginary;
            xbs = CapBS * cstate.Laplace.Imaginary;

            // Load Y-matrix
            GateGatePtr.Value += new Complex(0.0, xgd + xgs + xgb);
            BulkBulkPtr.Value += new Complex(load.CondBD + load.CondBS, xgb + xbd + xbs);
            DrainPrimeDrainPrimePtr.Value += new Complex(temp.DrainConductance + load.CondDS + load.CondBD + xrev * (load.Transconductance + load.TransconductanceBS), xgd + xbd);
            SourcePrimeSourcePrimePtr.Value += new Complex(temp.SourceConductance + load.CondDS + load.CondBS + xnrm * (load.Transconductance + load.TransconductanceBS), xgs + xbs);
            GateBulkPtr.Value -= new Complex(0.0, xgb);
            GateDrainPrimePtrPtr.Value -= new Complex(0.0, xgd);
            GateSourcePrimePtr.Value -= new Complex(0.0, xgs);
            BulkGatePtr.Value -= new Complex(0.0, xgb);
            BulkDrainPrimePtr.Value -= new Complex(load.CondBD, xbd);
            BulkSourcePrimePtr.Value -= new Complex(load.CondBS, xbs);
            DrainPrimeGatePtr.Value += new Complex((xnrm - xrev) * load.Transconductance, -xgd);
            DrainPrimeBulkPtr.Value += new Complex(-load.CondBD + (xnrm - xrev) * load.TransconductanceBS, -xbd);
            SourcePrimeGatePtr.Value -= new Complex((xnrm - xrev) * load.Transconductance, xgs);
            SourcePrimeBulkPtr.Value -= new Complex(load.CondBS + (xnrm - xrev) * load.TransconductanceBS, xbs);
            DrainDrainPtr.Value += (Complex)temp.DrainConductance;
            SourceSourcePtr.Value += (Complex)temp.SourceConductance;
            DrainDrainPrimePtr.Value -= temp.DrainConductance;
            SourceSourcePrimePtr.Value -= temp.SourceConductance;
            DrainPrimeDrainPtr.Value -= temp.DrainConductance;
            DrainPrimeSourcePrimePtr.Value -= load.CondDS + xnrm * (load.Transconductance + load.TransconductanceBS);
            SourcePrimeSourcePtr.Value -= temp.SourceConductance;
            SourcePrimeDrainPrimePtr.Value -= load.CondDS + xrev * (load.Transconductance + load.TransconductanceBS);
        }
    }
}
