using System;
using SpiceSharp.Sparse;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
{
    /// <summary>
    /// Transient behavior for a <see cref="Mosfet2"/>
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
        int drainNode, gateNode, sourceNode, bulkNode, drainNodePrime, sourceNodePrime;
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
        /// Shared variables
        /// </summary>
        public double CapBS { get; protected set; }
        public double CapBD { get; protected set; }

        /// <summary>
        /// Constants
        /// </summary>
        protected StateHistory VoltageBS { get; private set; }
        protected StateHistory VoltageGS { get; private set; }
        protected StateHistory VoltageDS { get; private set; }
        protected StateHistory CapGS { get; private set; }
        protected StateHistory CapGD { get; private set; }
        protected StateHistory CapGB { get; private set; }
        protected StateDerivative ChargeGS { get; private set; }
        protected StateDerivative ChargeGD { get; private set; }
        protected StateDerivative ChargeGB { get; private set; }
        protected StateDerivative ChargeBD { get; private set; }
        protected StateDerivative ChargeBS { get; private set; }

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
        /// Gets matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix<double> matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            // Get extra equations
            sourceNodePrime = load.SourceNodePrime;
            drainNodePrime = load.DrainNodePrime;

            // Get matrix elements
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
        /// Unsetup the behavior
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

            VoltageBS = states.CreateHistory();
            VoltageGS = states.CreateHistory();
            VoltageDS = states.CreateHistory();
            CapGS = states.CreateHistory();
            CapGD = states.CreateHistory();
            CapGB = states.CreateHistory();
            ChargeGS = states.CreateDerivative();
            ChargeGD = states.CreateDerivative();
            ChargeGB = states.CreateDerivative();
            ChargeBD = states.CreateDerivative();
            ChargeBS = states.CreateDerivative();
        }

        /// <summary>
        /// Calculate initial states
        /// </summary>
        /// <param name="simulation">Simulation</param>
        public override void GetDCState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap,
                OxideCap, vgs, vbs, vbd, vgb, vgd, von,
                vdsat, sargsw, capgs = 0.0, capgd = 0.0, capgb = 0.0;

            vbs = load.VoltageBS;
            vbd = load.VoltageBD;
            vgs = load.VoltageGS;
            vgd = load.VoltageGS - load.VoltageDS;
            vgb = load.VoltageGS - load.VoltageBS;
            von = mbp.MosfetType * load.Von;
            vdsat = mbp.MosfetType * load.SaturationVoltageDS;

            VoltageDS.Current = load.VoltageDS;
            VoltageBS.Current = vbs;
            VoltageGS.Current = vgs;

            EffectiveLength = bp.Length - 2 * mbp.LateralDiffusion;
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
                // NOSQRT
                ChargeBS.Current = temp.TempBulkPotential * (temp.CapBS * (1 - arg * sarg) / (1 - mbp.BulkJunctionBotGradingCoefficient) +
                    temp.CapBSSidewall * (1 - arg * sargsw) / (1 - mbp.BulkJunctionSideGradingCoefficient));
            }
            else
            {
                ChargeBS.Current = temp.F4S + vbs * (temp.F2S + vbs * (temp.F3S / 2));
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
                ChargeBD.Current = temp.TempBulkPotential * (temp.CapBD * (1 - arg * sarg) / (1 - mbp.BulkJunctionBotGradingCoefficient) +
                    temp.CapBDSidewall * (1 - arg * sargsw) / (1 - mbp.BulkJunctionSideGradingCoefficient));
            }
            else
            {
                ChargeBD.Current = temp.F4D + vbd * (temp.F2D + vbd * temp.F3D / 2);
            }

            /* 
             * calculate meyer's capacitors
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
            CapGS.Current = icapgs;
            CapGD.Current = icapgd;
            CapGB.Current = icapgb;

            capgs = 2 * CapGS.Current + GateSourceOverlapCap;
            capgd = 2 * CapGD.Current + GateDrainOverlapCap;
            capgb = 2 * CapGB.Current + GateBulkOverlapCap;

            /* TRANOP */
            ChargeGS.Current = capgs * vgs;
            ChargeGD.Current = capgd * vgd;
            ChargeGB.Current = capgb * vgb;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="simulation"></param>
        public override void Transient(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double EffectiveLength, GateSourceOverlapCap, GateDrainOverlapCap, GateBulkOverlapCap,
                OxideCap, vgs, vbs, vbd, vgb, vgd, von,
                vdsat, sargsw, vgs1, vgd1, vgb1, capgs = 0.0, capgd = 0.0, capgb = 0.0, gcgs, ceqgs, gcgd, ceqgd, gcgb, ceqgb, ceqbs,
                ceqbd;

            vbs = load.VoltageBS;
            vbd = load.VoltageBD;
            vgs = load.VoltageGS;
            vgd = load.VoltageGS - load.VoltageDS;
            vgb = load.VoltageGS - load.VoltageBS;
            von = mbp.MosfetType * load.Von;
            vdsat = mbp.MosfetType * load.SaturationVoltageDS;

            VoltageDS.Current = load.VoltageDS;
            VoltageBS.Current = vbs;
            VoltageGS.Current = vgs;

            double Gbd = 0.0;
            double Cbd = 0.0;
            double Cd = 0.0;
            double Gbs = 0.0;
            double Cbs = 0.0;

            EffectiveLength = bp.Length - 2 * mbp.LateralDiffusion;
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
                // NOSQRT
                ChargeBS.Current = temp.TempBulkPotential * (temp.CapBS * (1 - arg * sarg) / (1 - mbp.BulkJunctionBotGradingCoefficient) +
                    temp.CapBSSidewall * (1 - arg * sargsw) / (1 - mbp.BulkJunctionSideGradingCoefficient));
                CapBS = temp.CapBS * sarg + temp.CapBSSidewall * sargsw;
            }
            else
            {
                ChargeBS.Current = temp.F4S + vbs * (temp.F2S + vbs * (temp.F3S / 2));
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
                ChargeBD.Current = temp.TempBulkPotential * (temp.CapBD * (1 - arg * sarg) / (1 - mbp.BulkJunctionBotGradingCoefficient) +
                    temp.CapBDSidewall * (1 - arg * sargsw) / (1 - mbp.BulkJunctionSideGradingCoefficient));
                CapBD = temp.CapBD * sarg + temp.CapBDSidewall * sargsw;
            }
            else
            {
                ChargeBD.Current = temp.F4D + vbd * (temp.F2D + vbd * temp.F3D / 2);
                CapBD = temp.F2D + vbd * temp.F3D;
            }

            /* (above only excludes tranop, since we're only at this
            * point if tran or tranop)
            */
            /* 
            * calculate equivalent conductances and currents for
            * depletion capacitors
            */

            // integrate the capacitors and save results
            ChargeBD.Integrate();
            Gbd += ChargeBD.Jacobian(CapBD);
            Cbd += ChargeBD.Derivative;
            Cd -= ChargeBD.Derivative;
            ChargeBS.Integrate();
            Gbs += ChargeBS.Jacobian(CapBS);
            Cbs += ChargeBS.Derivative;

            /* 
             * calculate meyer's capacitors
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
            CapGS.Current = icapgs;
            CapGD.Current = icapgd;
            CapGB.Current = icapgb;

            vgs1 = VoltageGS[1];
            vgd1 = vgs1 - VoltageDS[1];
            vgb1 = vgs1 - VoltageBS[1];
            capgs = CapGS.Current + CapGS[1] + GateSourceOverlapCap;
            capgd = CapGD.Current + CapGD[1] + GateDrainOverlapCap;
            capgb = CapGB.Current + CapGB[1] + GateBulkOverlapCap;

            /* 
            * store small - signal parameters (for meyer's model)
            * all parameters already stored, so done...
            */

            ChargeGS.Current = (vgs - vgs1) * capgs + ChargeGS[1];
            ChargeGD.Current = (vgd - vgd1) * capgd + ChargeGD[1];
            ChargeGB.Current = (vgb - vgb1) * capgb + ChargeGB[1];

            /* 
             * calculate equivalent conductances and currents for
             * meyer"s capacitors
             */
            ChargeGS.Integrate();
            gcgs = ChargeGS.Jacobian(capgs);
            ceqgs = ChargeGS.RhsCurrent(gcgs, vgs);
            ChargeGD.Integrate();
            gcgd = ChargeGD.Jacobian(capgd);
            ceqgd = ChargeGD.RhsCurrent(gcgd, vgd);
            ChargeGB.Integrate();
            gcgb = ChargeGB.Jacobian(capgb);
            ceqgb = ChargeGB.RhsCurrent(gcgb, vgb);

            /* 
			* store charge storage info for meyer's cap in lx table
			*/

            /* 
            * load current vector
            */
            ceqbs = mbp.MosfetType * (Cbs - (Gbs - state.Gmin) * vbs);
            ceqbd = mbp.MosfetType * (Cbd - (Gbd - state.Gmin) * vbd);
            state.Rhs[gateNode] -= (mbp.MosfetType * (ceqgs + ceqgb + ceqgd));
            state.Rhs[bulkNode] -= (ceqbs + ceqbd - mbp.MosfetType * ceqgb);
            state.Rhs[drainNodePrime] += (ceqbd + mbp.MosfetType * ceqgd);
            state.Rhs[sourceNodePrime] += ceqbs + mbp.MosfetType * ceqgs;

            /* 
			 * load y matrix
			 */
            GateGatePtr.Add(gcgd + gcgs + gcgb);
            BulkBulkPtr.Add(Gbd + Gbs + gcgb);
            DrainPrimeDrainPrimePtr.Add(Gbd + gcgd);
            SourcePrimeSourcePrimePtr.Add(Gbs + gcgs);
            GateBulkPtr.Sub(gcgb);
            GateDrainPrimePtr.Sub(gcgd);
            GateSourcePrimePtr.Sub(gcgs);
            BulkGatePtr.Sub(gcgb);
            BulkDrainPrimePtr.Sub(Gbd);
            BulkSourcePrimePtr.Sub(Gbs);
            DrainPrimeGatePtr.Add(-gcgd);
            DrainPrimeBulkPtr.Add(-Gbd);
            SourcePrimeGatePtr.Add(-gcgs);
            SourcePrimeBulkPtr.Add(-Gbs);
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
