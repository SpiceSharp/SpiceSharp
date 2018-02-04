using System;
using System.Numerics;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// AC behavior for <see cref="Mosfet3"/>
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
        public double CapBS { get; protected set; }
        public double CapBD { get; protected set; }
        public double CapGS { get; protected set; }
        public double CapGD { get; protected set; }
        public double CapGB { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int drainNode, gateNode, sourceNode, bulkNode, drainNodePrime, sourceNodePrime;
        protected Element<Complex> DrainDrainPtr { get; private set; }
        protected Element<Complex> GateGatePtr { get; private set; }
        protected Element<Complex> SourceSourcePtr { get; private set; }
        protected Element<Complex> BulkBulkPtr { get; private set; }
        protected Element<Complex> DrainPrimeDrainPrimePtr { get; private set; }
        protected Element<Complex> SourcePrimeSourcePrimePtr { get; private set; }
        protected Element<Complex> DrainDrainPrimePtr { get; private set; }
        protected Element<Complex> GateBulkPtr { get; private set; }
        protected Element<Complex> GateDrainPrimePtr { get; private set; }
        protected Element<Complex> GateSourcePrimePtr { get; private set; }
        protected Element<Complex> SourceSourcePrimePtr { get; private set; }
        protected Element<Complex> BulkDrainPrimePtr { get; private set; }
        protected Element<Complex> BulkSourcePrimePtr { get; private set; }
        protected Element<Complex> DrainPrimeSourcePrimePtr { get; private set; }
        protected Element<Complex> DrainPrimeDrainPtr { get; private set; }
        protected Element<Complex> BulkGatePtr { get; private set; }
        protected Element<Complex> DrainPrimeGatePtr { get; private set; }
        protected Element<Complex> SourcePrimeGatePtr { get; private set; }
        protected Element<Complex> SourcePrimeSourcePtr { get; private set; }
        protected Element<Complex> DrainPrimeBulkPtr { get; private set; }
        protected Element<Complex> SourcePrimeBulkPtr { get; private set; }
        protected Element<Complex> SourcePrimeDrainPrimePtr { get; private set; }

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
        /// Gets matrix pionters
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix<Complex> matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            // Get extra equations
            drainNodePrime = load.DrainNodePrime;
            sourceNodePrime = load.SourceNodePrime;

            // Get matrix pointers
            DrainDrainPtr = matrix.GetElement(drainNode, drainNode);
            GateGatePtr = matrix.GetElement(gateNode, gateNode);
            SourceSourcePtr = matrix.GetElement(sourceNode, sourceNode);
            BulkBulkPtr = matrix.GetElement(bulkNode, bulkNode);
            DrainPrimeDrainPrimePtr = matrix.GetElement(drainNodePrime, drainNodePrime);
            SourcePrimeSourcePrimePtr = matrix.GetElement(sourceNodePrime, sourceNodePrime);
            DrainDrainPrimePtr = matrix.GetElement(drainNode, drainNodePrime);
            GateBulkPtr = matrix.GetElement(gateNode, bulkNode);
            GateDrainPrimePtr = matrix.GetElement(gateNode, drainNodePrime);
            GateSourcePrimePtr = matrix.GetElement(gateNode, sourceNodePrime);
            SourceSourcePrimePtr = matrix.GetElement(sourceNode, sourceNodePrime);
            BulkDrainPrimePtr = matrix.GetElement(bulkNode, drainNodePrime);
            BulkSourcePrimePtr = matrix.GetElement(bulkNode, sourceNodePrime);
            DrainPrimeSourcePrimePtr = matrix.GetElement(drainNodePrime, sourceNodePrime);
            DrainPrimeDrainPtr = matrix.GetElement(drainNodePrime, drainNode);
            BulkGatePtr = matrix.GetElement(bulkNode, gateNode);
            DrainPrimeGatePtr = matrix.GetElement(drainNodePrime, gateNode);
            SourcePrimeGatePtr = matrix.GetElement(sourceNodePrime, gateNode);
            SourcePrimeSourcePtr = matrix.GetElement(sourceNodePrime, sourceNode);
            DrainPrimeBulkPtr = matrix.GetElement(drainNodePrime, bulkNode);
            SourcePrimeBulkPtr = matrix.GetElement(sourceNodePrime, bulkNode);
            SourcePrimeDrainPrimePtr = matrix.GetElement(sourceNodePrime, drainNodePrime);
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
            GateDrainPrimePtr = null;
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

            double EffectiveLength,
                OxideCap, vgs, vds, vbs, vbd, vgd, von, vdsat,
                sargsw;

            vbs = load.VoltageBS;
            vbd = load.VoltageBD;
            vgs = load.VoltageGS;
            vds = load.VoltageDS;
            vgd = vgs - vds;
            von = mbp.MosfetType * load.Von;
            vdsat = mbp.MosfetType * load.SaturationVoltageDS;

            EffectiveLength = bp.Length - 2 * mbp.LateralDiffusion;
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
            if (vbs < temp.TempDepletionCap)
            {
                double arg = 1 - vbs / temp.TempBulkPotential, sarg;
                /* 
                * the following block looks somewhat long and messy, 
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (mbp.BulkJunctionBotGradingCoefficient.Value == mbp.BulkJunctionSideGradingCoefficient)
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
                /* NOSQRT */
                CapBS = temp.CapBS * sarg + temp.CapBSSidewall * sargsw;
            }
            else
            {
                CapBS = temp.F2S + temp.F3S * vbs;
            }

            if (vbd < temp.TempDepletionCap)
            {
                double arg = 1 - vbd / temp.TempBulkPotential, sarg;
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
                Transistor.MeyerCharges(vgs, vgd, von, vdsat,
                    out icapgs, out icapgd, out icapgb, temp.TempPhi, OxideCap);
            }
            else
            {
                Transistor.MeyerCharges(vgd, vgs, von, vdsat,
                    out icapgd, out icapgs, out icapgb, temp.TempPhi, OxideCap);
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

            var state = simulation.RealState;
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

            /* 
			 * charge oriented model parameters
			 */
            EffectiveLength = bp.Length - 2 * mbp.LateralDiffusion;
            GateSourceOverlapCap = mbp.GateSourceOverlapCapFactor * bp.Width;
            GateDrainOverlapCap = mbp.GateDrainOverlapCapFactor * bp.Width;
            GateBulkOverlapCap = mbp.GateBulkOverlapCapFactor * EffectiveLength;

            /* 
			 * meyer"s model parameters
			 */
            capgs = (CapGS + CapGS + GateSourceOverlapCap);
            capgd = (CapGD + CapGD + GateDrainOverlapCap);
            capgb = (CapGB + CapGB + GateBulkOverlapCap);
            xgs = capgs * cstate.Laplace.Imaginary;
            xgd = capgd * cstate.Laplace.Imaginary;
            xgb = capgb * cstate.Laplace.Imaginary;
            xbd = CapBD * cstate.Laplace.Imaginary;
            xbs = CapBS * cstate.Laplace.Imaginary;

            /* 
			 * load matrix
			 */
            GateGatePtr.Add(new Complex(0.0, xgd + xgs + xgb));
            BulkBulkPtr.Add(new Complex(load.CondBD + load.CondBS, xgb + xbd + xbs));
            DrainPrimeDrainPrimePtr.Add(new Complex(temp.DrainConductance + load.CondDS + load.CondBD + xrev * (load.Transconductance + load.TransconductanceBS), xgd + xbd));
            SourcePrimeSourcePrimePtr.Add(new Complex(temp.SourceConductance + load.CondDS + load.CondBS + xnrm * (load.Transconductance + load.TransconductanceBS), xgs + xbs));
            GateBulkPtr.Sub(new Complex(0.0, xgb));
            GateDrainPrimePtr.Sub(new Complex(0.0, xgd));
            GateSourcePrimePtr.Sub(new Complex(0.0, xgs));
            BulkGatePtr.Sub(new Complex(0.0, xgb));
            BulkDrainPrimePtr.Sub(new Complex(load.CondBD, xbd));
            BulkSourcePrimePtr.Sub(new Complex(load.CondBS, xbs));
            DrainPrimeGatePtr.Add(new Complex((xnrm - xrev) * load.Transconductance, -xgd));
            DrainPrimeBulkPtr.Add(new Complex(-load.CondBD + (xnrm - xrev) * load.TransconductanceBS, -xbd));
            SourcePrimeGatePtr.Sub(new Complex((xnrm - xrev) * load.Transconductance, xgs));
            SourcePrimeBulkPtr.Sub(new Complex(load.CondBS + (xnrm - xrev) * load.TransconductanceBS, xbs));
            DrainDrainPtr.Add((Complex)temp.DrainConductance);
            SourceSourcePtr.Add((Complex)temp.SourceConductance);
            DrainDrainPrimePtr.Sub(temp.DrainConductance);
            SourceSourcePrimePtr.Sub(temp.SourceConductance);
            DrainPrimeDrainPtr.Sub(temp.DrainConductance);
            DrainPrimeSourcePrimePtr.Sub(load.CondDS + xnrm * (load.Transconductance + load.TransconductanceBS));
            SourcePrimeSourcePtr.Sub(temp.SourceConductance);
            SourcePrimeDrainPrimePtr.Sub(load.CondDS + xrev * (load.Transconductance + load.TransconductanceBS));
        }
    }
}
