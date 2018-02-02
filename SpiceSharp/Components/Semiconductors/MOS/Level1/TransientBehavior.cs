using System;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Sparse;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.MosfetBehaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Transient behavior for a <see cref="Mosfet1"/>
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
        public double CapBD { get; protected set; }
        [PropertyName("cbs"), PropertyInfo("Bulk-Source capacitance")]
        public double CapBS { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int drainNode, gateNode, sourceNode, bulkNode, sourceNodePrime, drainNodePrime;
        protected Element<double> DrainDrainPtr { get; private set; }
        protected Element<double> GateGatePtr { get; private set; }
        protected Element<double> SourceSourcePtr { get; private set; }
        protected Element<double> BulkBulkPtr { get; private set; }
        protected Element<double> DrainPrimeDrainPrimePtr { get; private set; }
        protected Element<double> SourcePrimeSourcePrimePtr { get; private set; }
        protected Element<double> DrainDrainPrimePtr { get; private set; }
        protected Element<double> GateBulkPtr { get; private set; }
        protected Element<double> GateDrainPrimePtr { get; private set; }
        protected Element<double> GateSourcePrimePtr { get; private set; }
        protected Element<double> SourceSourcePrimePtr { get; private set; }
        protected Element<double> BulkDrainPrimePtr { get; private set; }
        protected Element<double> BulkSourcePrimePtr { get; private set; }
        protected Element<double> DrainPrimeSourcePrimePtr { get; private set; }
        protected Element<double> DrainPrimeDrainPtr { get; private set; }
        protected Element<double> BulkGatePtr { get; private set; }
        protected Element<double> DrainPrimeGatePtr { get; private set; }
        protected Element<double> SourcePrimeGatePtr { get; private set; }
        protected Element<double> SourcePrimeSourcePtr { get; private set; }
        protected Element<double> DrainPrimeBulkPtr { get; private set; }
        protected Element<double> SourcePrimeBulkPtr { get; private set; }
        protected Element<double> SourcePrimeDrainPrimePtr { get; private set; }

        /// <summary>
        /// State variables
        /// </summary>
        protected StateDerivative ChargeGS { get; private set; }
        protected StateDerivative ChargeGD { get; private set; }
        protected StateDerivative ChargeGB { get; private set; }
        protected StateDerivative ChargeBD { get; private set; }
        protected StateDerivative ChargeBS { get; private set; }
        protected StateHistory CapGS { get; private set; }
        protected StateHistory CapGD { get; private set; }
        protected StateHistory CapGB { get; private set; }
        protected StateHistory VoltageGS { get; private set; }
        protected StateHistory VoltageDS { get; private set; }
        protected StateHistory VoltageBS { get; private set; }

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
                throw new Diagnostics.CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            drainNode = pins[0];
            gateNode = pins[1];
            sourceNode = pins[2];
            bulkNode = pins[3];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix<double> matrix)
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
        /// Create states
        /// </summary>
        /// <param name="states">States</param>
        public override void CreateStates(StatePool states)
        {
			if (states == null)
				throw new ArgumentNullException(nameof(states));

            ChargeGS = states.CreateDerivative();
            ChargeGD = states.CreateDerivative();
            ChargeGB = states.CreateDerivative();
            ChargeBD = states.CreateDerivative();
            ChargeBS = states.CreateDerivative();

            CapGS = states.CreateHistory();
            CapGD = states.CreateHistory();
            CapGB = states.CreateHistory();
            VoltageGS = states.CreateHistory();
            VoltageDS = states.CreateHistory();
            VoltageBS = states.CreateHistory();
        }

        /// <summary>
        /// Calculate DC state variables
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void GetDCState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double arg, sarg, sargsw;
            double capgs, capgd, capgb;

            // Get voltages
            double vbd = load.VoltageBD;
            double vbs = load.VoltageBS;
            double vgs = load.VoltageGS;
            double vds = load.VoltageDS;
            double vgd = vgs - vds;
            double vgb = vgs - vbs;

            double EffectiveLength = bp.Length - 2 * mbp.LateralDiffusion;
            double GateSourceOverlapCap = mbp.GateSourceOverlapCapFactor * bp.Width;
            double GateDrainOverlapCap = mbp.GateDrainOverlapCapFactor * bp.Width;
            double GateBulkOverlapCap = mbp.GateBulkOverlapCapFactor * EffectiveLength;
            double OxideCap = modeltemp.OxideCapFactor * EffectiveLength * bp.Width;
            
            if (vbs < temp.TempDepletionCap)
            {
                arg = 1 - vbs / temp.TempBulkPotential;
                if (mbp.BulkJunctionBotGradingCoefficient.Value == mbp.BulkJunctionSideGradingCoefficient.Value)
                {
                    if (mbp.BulkJunctionBotGradingCoefficient.Value == .5)
                        sarg = sargsw = 1 / Math.Sqrt(arg);
                    else
                        sarg = sargsw = Math.Exp(-mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                }
                else
                {
                    if (mbp.BulkJunctionBotGradingCoefficient.Value == .5)
                        sarg = 1 / Math.Sqrt(arg);
                    else
                        sarg = Math.Exp(-mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    if (mbp.BulkJunctionSideGradingCoefficient.Value == .5)
                        sargsw = 1 / Math.Sqrt(arg);
                    else
                        sargsw = Math.Exp(-mbp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
                }
                ChargeBS.Current = temp.TempBulkPotential * (temp.CapBS * (1 - arg * sarg) / (1 - mbp.BulkJunctionBotGradingCoefficient) +
                    temp.CapBSSidewall * (1 - arg * sargsw) / (1 - mbp.BulkJunctionSideGradingCoefficient));
            }
            else
                ChargeBS.Current = temp.F4S + vbs * (temp.F2S + vbs * (temp.F3S / 2));

            if (vbd < temp.TempDepletionCap)
            {
                arg = 1 - vbd / temp.TempBulkPotential;
                if (mbp.BulkJunctionBotGradingCoefficient.Value == .5 && mbp.BulkJunctionSideGradingCoefficient.Value == .5)
                    sarg = sargsw = 1 / Math.Sqrt(arg);
                else
                {
                    if (mbp.BulkJunctionBotGradingCoefficient.Value == .5)
                        sarg = 1 / Math.Sqrt(arg);
                    else
                        sarg = Math.Exp(-mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    if (mbp.BulkJunctionSideGradingCoefficient.Value == .5)
                        sargsw = 1 / Math.Sqrt(arg);
                    else
                        sargsw = Math.Exp(-mbp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
                }
                ChargeBD.Current = temp.TempBulkPotential * (temp.CapBD * (1 - arg * sarg) / (1 - mbp.BulkJunctionBotGradingCoefficient) +
                    temp.CapBDSidewall * (1 - arg * sargsw) / (1 - mbp.BulkJunctionSideGradingCoefficient));
            }
            else
                ChargeBD.Current = temp.F4D + vbd * (temp.F2D + vbd * temp.F3D / 2);

            /* 
             * calculate meyer's capacitors
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
            CapGS.Current = icapgs;
            CapGD.Current = icapgd;
            CapGB.Current = icapgb;
            capgs = 2 * CapGS.Current + GateSourceOverlapCap;
            capgd = 2 * CapGD.Current + GateDrainOverlapCap;
            capgb = 2 * CapGB.Current + GateBulkOverlapCap;

            /* TRANOP only */
            ChargeGS.Current = vgs * capgs;
            ChargeGD.Current = vgd * capgd;
            ChargeGB.Current = vgb * capgb;

            // Store these voltages
            VoltageGS.Current = vgs;
            VoltageDS.Current = vds;
            VoltageBS.Current = vbs;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Transient(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double arg, sarg, sargsw;
            double vgs1, vgd1, vgb1, capgs, capgd, capgb;

            // Get voltages
            double vbd = load.VoltageBD;
            double vbs = load.VoltageBS;
            double vgs = load.VoltageGS;
            double vds = load.VoltageDS;
            double vgd = vgs - vds;
            double vgb = vgs - vbs;

            double EffectiveLength = bp.Length - 2 * mbp.LateralDiffusion;
            double GateSourceOverlapCap = mbp.GateSourceOverlapCapFactor * bp.Width;
            double GateDrainOverlapCap = mbp.GateDrainOverlapCapFactor * bp.Width;
            double GateBulkOverlapCap = mbp.GateBulkOverlapCapFactor * EffectiveLength;
            double OxideCap = modeltemp.OxideCapFactor * EffectiveLength * bp.Width;

            double Gbd = 0.0;
            double Cbd = 0.0;
            double Cd = 0.0;
            double Gbs = 0.0;
            double Cbs = 0.0;

            // Store these voltages
            VoltageGS.Current = vgs;
            VoltageDS.Current = vds;
            VoltageBS.Current = vbs;

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

                /* NOSQRT */
                ChargeBS.Current = temp.TempBulkPotential * (temp.CapBS * (1 - arg * sarg) / (1 - mbp.BulkJunctionBotGradingCoefficient) +
                    temp.CapBSSidewall * (1 - arg * sargsw) / (1 - mbp.BulkJunctionSideGradingCoefficient));
                CapBS = temp.CapBS * sarg + temp.CapBSSidewall * sargsw;
            }
            else
            {
                ChargeBS.Current = temp.F4S + vbs * (temp.F2S + vbs * (temp.F3S / 2));
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
                ChargeBD.Current = temp.TempBulkPotential * (temp.CapBD * (1 - arg * sarg) / (1 - mbp.BulkJunctionBotGradingCoefficient) +
                    temp.CapBDSidewall * (1 - arg * sargsw) / (1 - mbp.BulkJunctionSideGradingCoefficient));
                CapBD = temp.CapBD * sarg + temp.CapBDSidewall * sargsw;
            }
            else
            {
                ChargeBD.Current = temp.F4D + vbd * (temp.F2D + vbd * temp.F3D / 2);
                CapBD = temp.F2D + vbd * temp.F3D;
            }

            // integrate the capacitors and save results
            ChargeBD.Integrate();
            Gbd += ChargeBD.Jacobian(CapBD);
            Cbd += ChargeBD.Derivative;
            Cd -= ChargeBD.Derivative;
            // NOTE: The derivative of Qbd should be added to Cd (drain current). Figure out a way later.
            ChargeBS.Integrate();
            Gbs += ChargeBS.Jacobian(CapBS);
            Cbs += ChargeBS.Derivative;

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
                Transistor.MeyerCharges(vgs, vgd,  mbp.MosfetType * load.Von, mbp.MosfetType * load.SaturationVoltageDS,
                    out icapgs, out icapgd, out icapgb,
                    temp.TempPhi, OxideCap);
            }
            else
            {
                Transistor.MeyerCharges(vgd, vgs, mbp.MosfetType * load.Von, mbp.MosfetType * load.SaturationVoltageDS,
                    out icapgd, out icapgs, out icapgb,
                    temp.TempPhi, OxideCap);
            }
            CapGS.Current = icapgs;
            CapGD.Current = icapgd;
            CapGB.Current = icapgb;
            vgs1 = VoltageGS[1];
            vgd1 = vgs1 - VoltageDS[1];
            vgb1 = vgs1 - VoltageBS[1];
            capgs = CapGS.Current + CapGS[1] + GateSourceOverlapCap;
            capgd = CapGD.Current + CapGD[1] + GateDrainOverlapCap;
            capgb = CapGB.Current + CapGB[1] + GateBulkOverlapCap;

            ChargeGS.Current = (vgs - vgs1) * capgs + ChargeGS[1];
            ChargeGD.Current = (vgd - vgd1) * capgd + ChargeGD[1];
            ChargeGB.Current = (vgb - vgb1) * capgb + ChargeGB[1];


            /* NOTE: We can't reset derivatives!
            if (capgs == 0)
                state.States[0][States + Cqgs] = 0;
            if (capgd == 0)
                state.States[0][States + Cqgd] = 0;
            if (capgb == 0)
                state.States[0][States + Cqgb] = 0;
            */

            /* NOTE: The formula with the method.Slope is to make it work for nonlinear capacitances!
             * The correct formula is: ceq = dQ/dt - geq * vq where geq = slope * dQ/dvq
             * The formula in Spice 3f5 is: ceq = dQ/dt - slope * Q where it assumes a linear capacitance
            method.Integrate(state, out gcgs, out ceqgs, States + Qgs, capgs);
            method.Integrate(state, out gcgd, out ceqgd, States + Qgd, capgd);
            method.Integrate(state, out gcgb, out ceqgb, States + Qgb, capgb);
            ceqgs = ceqgs - gcgs * vgs + method.Slope * state.States[0][States + Qgs];
            ceqgd = ceqgd - gcgd * vgd + method.Slope * state.States[0][States + Qgd];
            ceqgb = ceqgb - gcgb * vgb + method.Slope * state.States[0][States + Qgb];
            */

            ChargeGS.Integrate();
            double gcgs = ChargeGS.Jacobian(capgs);
            double ceqgs = ChargeGS.RhsCurrent(gcgs, vgs);
            ChargeGD.Integrate();
            double gcgd = ChargeGD.Jacobian(capgd);
            double ceqgd = ChargeGD.RhsCurrent(gcgd, vgd);
            ChargeGB.Integrate();
            double gcgb = ChargeGB.Jacobian(capgb);
            double ceqgb = ChargeGB.RhsCurrent(gcgb, vgb);

            /* 
			 * load current vector
			 */
            double ceqbs = mbp.MosfetType * (Cbs - Gbs * vbs);
            double ceqbd = mbp.MosfetType * (Cbd - Gbd * vbd);
            state.Rhs[gateNode] -= mbp.MosfetType * (ceqgs + ceqgb + ceqgd);
            state.Rhs[bulkNode] -= ceqbs + ceqbd - mbp.MosfetType * ceqgb;
            state.Rhs[drainNodePrime] += ceqbd + mbp.MosfetType * ceqgd;
            state.Rhs[sourceNodePrime] += ceqbs + mbp.MosfetType * ceqgs;

            /* 
			 * load y matrix
			 */
            GateGatePtr.Add(gcgd + gcgs + gcgb);
            BulkBulkPtr.Add((Gbd + Gbs + gcgb));
            DrainPrimeDrainPrimePtr.Add(Gbd + gcgd);
            SourcePrimeSourcePrimePtr.Add(Gbs + gcgs);
            GateBulkPtr.Sub(gcgb);
            GateDrainPrimePtr.Sub(gcgd);
            GateSourcePrimePtr.Sub(gcgs);
            BulkGatePtr.Sub(gcgb);
            BulkDrainPrimePtr.Sub(Gbd);
            BulkSourcePrimePtr.Sub(Gbs);
            DrainPrimeGatePtr.Sub(gcgd);
            DrainPrimeBulkPtr.Sub(Gbd);
            SourcePrimeGatePtr.Sub(gcgs);
            SourcePrimeBulkPtr.Sub(Gbs);
        }

        /// <summary>
        /// Truncate timestep
        /// </summary>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(ref double timestep)
        {
            ChargeGS.LocalTruncationError(ref timestep);
            ChargeGD.LocalTruncationError(ref timestep);
            ChargeGB.LocalTruncationError(ref timestep);
        }
    }
}
