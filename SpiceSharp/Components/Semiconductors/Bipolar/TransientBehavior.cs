using System;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="BipolarJunctionTransistor"/>
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
        int collectorNode, baseNode, emitterNode, substrateNode, colPrimeNode, basePrimeNode, emitPrimeNode;
        protected Element<double> CollectorCollectorPrimePtr { get; private set; }
        protected Element<double> BaseBasePrimePtr { get; private set; }
        protected Element<double> EmitterEmitterPrimePtr { get; private set; }
        protected Element<double> CollectorPrimeCollectorPtr { get; private set; }
        protected Element<double> CollectorPrimeBasePrimePtr { get; private set; }
        protected Element<double> CollectorPrimeEmitterPrimePtr { get; private set; }
        protected Element<double> BasePrimeBasePtr { get; private set; }
        protected Element<double> BasePrimeCollectorPrimePtr { get; private set; }
        protected Element<double> BasePrimeEmitterPrimePtr { get; private set; }
        protected Element<double> EmitterPrimeEmitterPtr { get; private set; }
        protected Element<double> EmitterPrimeCollectorPrimePtr { get; private set; }
        protected Element<double> EmitterPrimeBasePrimePtr { get; private set; }
        protected Element<double> CollectorCollectorPtr { get; private set; }
        protected Element<double> BaseBasePtr { get; private set; }
        protected Element<double> EmitterEmitterPtr { get; private set; }
        protected Element<double> CollectorPrimeCollectorPrimePtr { get; private set; }
        protected Element<double> BasePrimeBasePrimePtr { get; private set; }
        protected Element<double> EmitterPrimeEmitterPrimePtr { get; private set; }
        protected Element<double> SubstrateSubstratePtr { get; private set; }
        protected Element<double> CollectorPrimeSubstratePtr { get; private set; }
        protected Element<double> SubstrateCollectorPrimePtr { get; private set; }
        protected Element<double> BaseCollectorPrimePtr { get; private set; }
        protected Element<double> CollectorPrimeBasePtr { get; private set; }

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [PropertyName("qbe"), PropertyInfo("Charge storage B-E junction")]
        public double ChargeBE => StateChargeBE.Current;
        [PropertyName("cqbe"), PropertyInfo("Capacitance current due to charges in the B-E junction")]
        public double CurrentBE => StateChargeBE.Derivative;
        [PropertyName("qbc"), PropertyInfo("Charge storage B-C junction")]
        public double ChargeBC => StateChargeBC.Current;
        [PropertyName("cqbc"), PropertyInfo("Capacitance current due to charges in the B-C junction")]
        public double CurrentBC => StateChargeBC.Derivative;
        [PropertyName("qcs"), PropertyInfo("Charge storage C-S junction")]
        public double ChargeCS => StateChargeCS.Current;
        [PropertyName("cqcs"), PropertyInfo("Capacitance current due to charges in the C-S junction")]
        public double CurrentCS => StateChargeCS.Derivative;
        [PropertyName("qbx"), PropertyInfo("Charge storage B-X junction")]
        public double ChargeBX => StateChargeBX.Current;
        [PropertyName("cqbx"), PropertyInfo("Capacitance current due to charges in the B-X junction")]
        public double CurrentBX => StateChargeBX.Derivative;
        [PropertyName("cexbc"), PropertyInfo("Total capacitance in B-X junction")]
        public double CurrentExBC => StateExcessPhaseCurrentBC.Current;
        [PropertyName("cpi"), PropertyInfo("Internal base to emitter capactance")]
        public double CapBE { get; internal set; }
        [PropertyName("cmu"), PropertyInfo("Internal base to collector capactiance")]
        public double CapBC { get; internal set; }
        [PropertyName("cbx"), PropertyInfo("Base to collector capacitance")]
        public double CapBX { get; internal set; }
        [PropertyName("ccs"), PropertyInfo("Collector to substrate capacitance")]
        public double CapCS { get; internal set; }

        /// <summary>
        /// States
        /// </summary>
        protected StateDerivative StateChargeBE { get; private set; }
        protected StateDerivative StateChargeBC { get; private set; }
        protected StateDerivative StateChargeCS { get; private set; }
        protected StateDerivative StateChargeBX { get; private set; }
        protected StateHistory StateExcessPhaseCurrentBC { get; private set; }

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
            collectorNode = pins[0];
            baseNode = pins[1];
            emitterNode = pins[2];
            substrateNode = pins[3];
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
            colPrimeNode = load.CollectorPrimeNode;
            basePrimeNode = load.BasePrimeNode;
            emitPrimeNode = load.EmitterPrimeNode;

            // Get matrix pointers
            CollectorCollectorPrimePtr = matrix.GetElement(collectorNode, colPrimeNode);
            BaseBasePrimePtr = matrix.GetElement(baseNode, basePrimeNode);
            EmitterEmitterPrimePtr = matrix.GetElement(emitterNode, emitPrimeNode);
            CollectorPrimeCollectorPtr = matrix.GetElement(colPrimeNode, collectorNode);
            CollectorPrimeBasePrimePtr = matrix.GetElement(colPrimeNode, basePrimeNode);
            CollectorPrimeEmitterPrimePtr = matrix.GetElement(colPrimeNode, emitPrimeNode);
            BasePrimeBasePtr = matrix.GetElement(basePrimeNode, baseNode);
            BasePrimeCollectorPrimePtr = matrix.GetElement(basePrimeNode, colPrimeNode);
            BasePrimeEmitterPrimePtr = matrix.GetElement(basePrimeNode, emitPrimeNode);
            EmitterPrimeEmitterPtr = matrix.GetElement(emitPrimeNode, emitterNode);
            EmitterPrimeCollectorPrimePtr = matrix.GetElement(emitPrimeNode, colPrimeNode);
            EmitterPrimeBasePrimePtr = matrix.GetElement(emitPrimeNode, basePrimeNode);
            CollectorCollectorPtr = matrix.GetElement(collectorNode, collectorNode);
            BaseBasePtr = matrix.GetElement(baseNode, baseNode);
            EmitterEmitterPtr = matrix.GetElement(emitterNode, emitterNode);
            CollectorPrimeCollectorPrimePtr = matrix.GetElement(colPrimeNode, colPrimeNode);
            BasePrimeBasePrimePtr = matrix.GetElement(basePrimeNode, basePrimeNode);
            EmitterPrimeEmitterPrimePtr = matrix.GetElement(emitPrimeNode, emitPrimeNode);
            SubstrateSubstratePtr = matrix.GetElement(substrateNode, substrateNode);
            CollectorPrimeSubstratePtr = matrix.GetElement(colPrimeNode, substrateNode);
            SubstrateCollectorPrimePtr = matrix.GetElement(substrateNode, colPrimeNode);
            BaseCollectorPrimePtr = matrix.GetElement(baseNode, colPrimeNode);
            CollectorPrimeBasePtr = matrix.GetElement(colPrimeNode, baseNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            CollectorCollectorPrimePtr = null;
            BaseBasePrimePtr = null;
            EmitterEmitterPrimePtr = null;
            CollectorPrimeCollectorPtr = null;
            CollectorPrimeBasePrimePtr = null;
            CollectorPrimeEmitterPrimePtr = null;
            BasePrimeBasePtr = null;
            BasePrimeCollectorPrimePtr = null;
            BasePrimeEmitterPrimePtr = null;
            EmitterPrimeEmitterPtr = null;
            EmitterPrimeCollectorPrimePtr = null;
            EmitterPrimeBasePrimePtr = null;
            CollectorCollectorPtr = null;
            BaseBasePtr = null;
            EmitterEmitterPtr = null;
            CollectorPrimeCollectorPrimePtr = null;
            BasePrimeBasePrimePtr = null;
            EmitterPrimeEmitterPrimePtr = null;
            SubstrateSubstratePtr = null;
            CollectorPrimeSubstratePtr = null;
            SubstrateCollectorPrimePtr = null;
            BaseCollectorPrimePtr = null;
            CollectorPrimeBasePtr = null;
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="states">Pool of all states</param>
        public override void CreateStates(StatePool states)
        {
			if (states == null)
				throw new ArgumentNullException(nameof(states));

            // We just need a history without integration here
            StateChargeBE = states.CreateDerivative();
            StateChargeBC = states.CreateDerivative();
            StateChargeCS = states.CreateDerivative();
            StateChargeBX = states.CreateDerivative();
            StateExcessPhaseCurrentBC = states.CreateHistory();
        }

        /// <summary>
        /// Calculate state variables
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void GetDCState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double tf, tr, czbe, pe, xme, cdis, ctot, czbc, czbx, pc, xmc, fcpe, czcs, ps, arg, arg2,
                xms, xtf, ovtf, xjtf, argtf, tmp, sarg, f1, f2, f3, czbef2, fcpc, czbcf2, czbxf2;

            double cbe = load.CurrentBE;
            double cbc = load.CurrentBC;
            double gbe = load.CondBE;
            double qb = load.BaseCharge;
            double dqbdve = load.Dqbdve;

            double vbe = load.VoltageBE;
            double vbc = load.VoltageBC;
            double vbx = mbp.BipolarType * (state.Solution[baseNode] - state.Solution[colPrimeNode]);
            double vcs = mbp.BipolarType * (state.Solution[substrateNode] - state.Solution[colPrimeNode]);

            StateExcessPhaseCurrentBC.Current = load.CurrentBE / load.BaseCharge;

            // Charge storage elements
            tf = mbp.TransitTimeForward;
            tr = mbp.TransitTimeReverse;
            czbe = temp.TempBECap * bp.Area;
            pe = temp.TempBEPotential;
            xme = mbp.JunctionExpBE;
            cdis = mbp.BaseFractionBCCap;
            ctot = temp.TempBCCap * bp.Area;
            czbc = ctot * cdis;
            czbx = ctot - czbc;
            pc = temp.TempBCPotential;
            xmc = mbp.JunctionExpBC;
            fcpe = temp.TempDepletionCap;
            czcs = mbp.CapCS * bp.Area;
            ps = mbp.PotentialSubstrate;
            xms = mbp.ExponentialSubstrate;
            xtf = mbp.TransitTimeBiasCoefficientForward;
            ovtf = modeltemp.TransitTimeVoltageBCFactor;
            xjtf = mbp.TransitTimeHighCurrentForward * bp.Area;
            if (!tf.Equals(0) && vbe > 0) // Avoid computations
            {
                argtf = 0;
                arg2 = 0;
                if (!xtf.Equals(0)) // Avoid computations
                {
                    argtf = xtf;
                    if (!ovtf.Equals(0)) // Avoid expensive Exp()
                    {
                        argtf = argtf * Math.Exp(vbc * ovtf);
                    }
                    arg2 = argtf;
                    if (!xjtf.Equals(0)) // Avoid computations
                    {
                        tmp = cbe / (cbe + xjtf);
                        argtf = argtf * tmp * tmp;
                        arg2 = argtf * (3 - tmp - tmp);
                    }
                }
                cbe = cbe * (1 + argtf) / qb;
                gbe = (gbe * (1 + arg2) - cbe * dqbdve) / qb;
            }
            if (vbe < fcpe)
            {
                arg = 1 - vbe / pe;
                sarg = Math.Exp(-xme * Math.Log(arg));
                StateChargeBE.Current = tf * cbe + pe * czbe * (1 - arg * sarg) / (1 - xme);
            }
            else
            {
                f1 = temp.TempFactor1;
                f2 = modeltemp.F2;
                f3 = modeltemp.F3;
                czbef2 = czbe / f2;
                StateChargeBE.Current = tf * cbe + czbe * f1 + czbef2 * (f3 * (vbe - fcpe) + (xme / (pe + pe)) * (vbe * vbe -
                     fcpe * fcpe));
            }
            fcpc = temp.TempFactor4;
            f1 = temp.TempFactor5;
            f2 = modeltemp.F6;
            f3 = modeltemp.F7;
            if (vbc < fcpc)
            {
                arg = 1 - vbc / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                StateChargeBC.Current = tr * cbc + pc * czbc * (1 - arg * sarg) / (1 - xmc);
            }
            else
            {
                czbcf2 = czbc / f2;
                StateChargeBC.Current = tr * cbc + czbc * f1 + czbcf2 * (f3 * (vbc - fcpc) + (xmc / (pc + pc)) * (vbc * vbc -
                     fcpc * fcpc));
            }
            if (vbx < fcpc)
            {
                arg = 1 - vbx / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                StateChargeBX.Current = pc * czbx * (1 - arg * sarg) / (1 - xmc);
            }
            else
            {
                czbxf2 = czbx / f2;
                StateChargeBX.Current = czbx * f1 + czbxf2 * (f3 * (vbx - fcpc) + (xmc / (pc + pc)) * (vbx * vbx - fcpc * fcpc));
            }
            if (vcs < 0)
            {
                arg = 1 - vcs / ps;
                sarg = Math.Exp(-xms * Math.Log(arg));
                StateChargeCS.Current = ps * czcs * (1 - arg * sarg) / (1 - xms);
            }
            else
            {
                StateChargeCS.Current = vcs * czcs * (1 + xms * vcs / (2 * ps));
            }

            // Register for excess phase calculations
            if (modeltemp.ExcessPhaseFactor > 0.0)
            {
                load.ExcessPhaseCalculation += CalculateExcessPhase;
            }
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
            double tf, tr, czbe, pe, xme, cdis, ctot, czbc, czbx, pc, xmc, fcpe, czcs, ps, arg, arg2, arg3,
                xms, xtf, ovtf, xjtf, argtf, tmp, sarg, f1, f2, f3, czbef2, fcpc, czbcf2, czbxf2;

            double cbe = load.CurrentBE;
            double cbc = load.CurrentBC;
            double gbe = load.CondBE;
            double gbc = load.CondBC;
            double qb = load.BaseCharge;
            double geqcb = 0;

            double gpi = 0.0;
            double gmu = 0.0;
            double cb = 0.0;
            double cc = 0.0;

            double vbe = load.VoltageBE;
            double vbc = load.VoltageBC;
            double vbx = mbp.BipolarType * (state.Solution[baseNode] - state.Solution[colPrimeNode]);
            double vcs = mbp.BipolarType * (state.Solution[substrateNode] - state.Solution[colPrimeNode]);

            // Charge storage elements
            tf = mbp.TransitTimeForward;
            tr = mbp.TransitTimeReverse;
            czbe = temp.TempBECap * bp.Area;
            pe = temp.TempBEPotential;
            xme = mbp.JunctionExpBE;
            cdis = mbp.BaseFractionBCCap;
            ctot = temp.TempBCCap * bp.Area;
            czbc = ctot * cdis;
            czbx = ctot - czbc;
            pc = temp.TempBCPotential;
            xmc = mbp.JunctionExpBC;
            fcpe = temp.TempDepletionCap;
            czcs = mbp.CapCS * bp.Area;
            ps = mbp.PotentialSubstrate;
            xms = mbp.ExponentialSubstrate;
            xtf = mbp.TransitTimeBiasCoefficientForward;
            ovtf = modeltemp.TransitTimeVoltageBCFactor;
            xjtf = mbp.TransitTimeHighCurrentForward * bp.Area;
            if (!tf.Equals(0) && vbe > 0) // Avoid computations
            {
                argtf = 0;
                arg2 = 0;
                arg3 = 0;
                if (!xtf.Equals(0)) // Avoid computations
                {
                    argtf = xtf;
                    if (!ovtf.Equals(0)) // Avoid expensive Exp()
                    {
                        argtf = argtf * Math.Exp(vbc * ovtf);
                    }
                    arg2 = argtf;
                    if (!xjtf.Equals(0)) // Avoid computations
                    {
                        tmp = cbe / (cbe + xjtf);
                        argtf = argtf * tmp * tmp;
                        arg2 = argtf * (3 - tmp - tmp);
                    }
                    arg3 = cbe * argtf * ovtf;
                }
                cbe = cbe * (1 + argtf) / qb;
                gbe = (gbe * (1 + arg2) - cbe * load.Dqbdve) / qb;
                geqcb = tf * (arg3 - cbe * load.Dqbdvc) / qb;
            }
            if (vbe < fcpe)
            {
                arg = 1 - vbe / pe;
                sarg = Math.Exp(-xme * Math.Log(arg));
                StateChargeBE.Current = tf * cbe + pe * czbe * (1 - arg * sarg) / (1 - xme);
                CapBE = tf * gbe + czbe * sarg;
            }
            else
            {
                f1 = temp.TempFactor1;
                f2 = modeltemp.F2;
                f3 = modeltemp.F3;
                czbef2 = czbe / f2;
                StateChargeBE.Current = tf * cbe + czbe * f1 + czbef2 * (f3 * (vbe - fcpe) + (xme / (pe + pe)) * (vbe * vbe -
                     fcpe * fcpe));
                CapBE = tf * gbe + czbef2 * (f3 + xme * vbe / pe);
            }
            fcpc = temp.TempFactor4;
            f1 = temp.TempFactor5;
            f2 = modeltemp.F6;
            f3 = modeltemp.F7;
            if (vbc < fcpc)
            {
                arg = 1 - vbc / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                StateChargeBC.Current = tr * cbc + pc * czbc * (1 - arg * sarg) / (1 - xmc);
                CapBC = tr * gbc + czbc * sarg;
            }
            else
            {
                czbcf2 = czbc / f2;
                StateChargeBC.Current = tr * cbc + czbc * f1 + czbcf2 * (f3 * (vbc - fcpc) + (xmc / (pc + pc)) * (vbc * vbc -
                     fcpc * fcpc));
                CapBC = tr * gbc + czbcf2 * (f3 + xmc * vbc / pc);
            }
            if (vbx < fcpc)
            {
                arg = 1 - vbx / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                StateChargeBX.Current = pc * czbx * (1 - arg * sarg) / (1 - xmc);
                CapBX = czbx * sarg;
            }
            else
            {
                czbxf2 = czbx / f2;
                StateChargeBX.Current = czbx * f1 + czbxf2 * (f3 * (vbx - fcpc) + (xmc / (pc + pc)) * (vbx * vbx - fcpc * fcpc));
                CapBX = czbxf2 * (f3 + xmc * vbx / pc);
            }
            if (vcs < 0)
            {
                arg = 1 - vcs / ps;
                sarg = Math.Exp(-xms * Math.Log(arg));
                StateChargeCS.Current = ps * czcs * (1 - arg * sarg) / (1 - xms);
                CapCS = czcs * sarg;
            }
            else
            {
                StateChargeCS.Current = vcs * czcs * (1 + xms * vcs / (2 * ps));
                CapCS = czcs * (1 + xms * vcs / ps);
            }

            StateChargeBE.Integrate();
            geqcb = StateChargeBE.Jacobian(geqcb); // Multiplies geqcb with method.Slope (ag[0])
            gpi += StateChargeBE.Jacobian(CapBE);
            cb += StateChargeBE.Derivative;
            StateChargeBC.Integrate();
            gmu += StateChargeBC.Jacobian(CapBC);
            cb += StateChargeBC.Derivative;
            cc -= StateChargeBC.Derivative;

            // Charge storage for c-s and b-x junctions
            StateChargeCS.Integrate();
            double gccs = StateChargeCS.Jacobian(CapCS);
            StateChargeBX.Integrate();
            double geqbx = StateChargeBX.Jacobian(CapBX);

            // Load current excitation vector
            double ceqcs = mbp.BipolarType * (StateChargeCS.Derivative - vcs * gccs);
            double ceqbx = mbp.BipolarType * (StateChargeBX.Derivative - vbx * geqbx);
            double ceqbe = mbp.BipolarType * (cc + cb - vbe * gpi + vbc * (-geqcb));
            double ceqbc = mbp.BipolarType * (-cc + - vbc * gmu);
            
            state.Rhs[baseNode] += (-ceqbx);
            state.Rhs[colPrimeNode] += (ceqcs + ceqbx + ceqbc);
            state.Rhs[basePrimeNode] += -ceqbe - ceqbc;
            state.Rhs[emitPrimeNode] += (ceqbe);
            state.Rhs[substrateNode] += (-ceqcs);

            // Load y matrix
            BaseBasePtr.Add(geqbx);
            CollectorPrimeCollectorPrimePtr.Add(gmu + gccs + geqbx);
            BasePrimeBasePrimePtr.Add(gpi + gmu + geqcb);
            EmitterPrimeEmitterPrimePtr.Add(gpi);
            CollectorPrimeBasePrimePtr.Add(-gmu);
            BasePrimeCollectorPrimePtr.Add(-gmu - geqcb);
            BasePrimeEmitterPrimePtr.Add(-gpi);
            EmitterPrimeCollectorPrimePtr.Add(geqcb);
            EmitterPrimeBasePrimePtr.Add(-gpi - geqcb);
            SubstrateSubstratePtr.Add(gccs);
            CollectorPrimeSubstratePtr.Add(-gccs);
            SubstrateCollectorPrimePtr.Add(-gccs);
            BaseCollectorPrimePtr.Add(-geqbx);
            CollectorPrimeBasePtr.Add(-geqbx);
        }

        /// <summary>
        /// Truncate the timestep
        /// </summary>
        /// <param name="timestep">Current timestep</param>
        public override void Truncate(ref double timestep)
        {
            StateChargeBE.LocalTruncationError(ref timestep);
            StateChargeBC.LocalTruncationError(ref timestep);
            StateChargeCS.LocalTruncationError(ref timestep);
        }

        /// <summary>
        /// Calculate excess phase
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        public void CalculateExcessPhase(object sender, ExcessPhaseEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            
            double arg1, arg2, denom, arg3;
            double td = modeltemp.ExcessPhaseFactor;
            if (td.Equals(0))
            {
                StateExcessPhaseCurrentBC.Current = args.ExcessPhaseCurrent;
                return;
            }
            
            /* 
             * weil's approx. for excess phase applied with backward - 
             * euler integration
             */
            double cbe = args.ExcessPhaseCurrent;
            double gbe = args.ExcessPhaseConduct;

            double delta = StateExcessPhaseCurrentBC.Timesteps[0];
            double prevdelta = StateExcessPhaseCurrentBC.Timesteps[1];
            arg1 = delta / td;
            arg2 = 3 * arg1;
            arg1 = arg2 * arg1;
            denom = 1 + arg1 + arg2;
            arg3 = arg1 / denom;
            /* Still need a place for this...
            if (state.Init == State.InitFlags.InitTransient)
            {
                state.States[1][State + Cexbc] = cbe / qb;
                state.States[2][State + Cexbc] = state.States[1][State + Cexbc];
            } */
            args.CollectorCurrent = (StateExcessPhaseCurrentBC[1] * (1 + delta / prevdelta + arg2) 
                - StateExcessPhaseCurrentBC[2] * delta / prevdelta) / denom;
            args.ExcessPhaseCurrent = cbe * arg3;
            args.ExcessPhaseConduct = gbe * arg3;
            StateExcessPhaseCurrentBC.Current = args.CollectorCurrent + args.ExcessPhaseCurrent / args.BaseCharge;
        }
    }
}
