using System;
using SpiceSharp.Circuits;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// General behavior for <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        TemperatureBehavior temp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyName("vbe"), PropertyInfo("B-E voltage")]
        public double VoltageBE { get; protected set; }
        [PropertyName("vbc"), PropertyInfo("B-C voltage")]
        public double VoltageBC { get; protected set; }
        [PropertyName("cc"), PropertyInfo("Current at collector node")]
        public double CollectorCurrent { get; protected set; }
        [PropertyName("cb"), PropertyInfo("Current at base node")]
        public double BaseCurrent { get; protected set; }
        [PropertyName("gpi"), PropertyInfo("Small signal input conductance - pi")]
        public double ConductancePi { get; protected set; }
        [PropertyName("gmu"), PropertyInfo("Small signal conductance - mu")]
        public double ConductanceMu { get; protected set; }
        [PropertyName("gm"), PropertyInfo("Small signal transconductance")]
        public double Transconductance { get; protected set; }
        [PropertyName("go"), PropertyInfo("Small signal output conductance")]
        public double OutputConductance { get; protected set; }
        public double ConductanceX { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int collectorNode, baseNode, emitterNode, substrateNode;
        public int CollectorPrimeNode { get; private set; }
        public int BasePrimeNode { get; private set; }
        public int EmitterPrimeNode { get; private set; }
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
        /// Shared parameters
        /// </summary>
        public double CurrentBE { get; protected set; }
        public double CondBE { get; protected set; }
        public double CurrentBC { get; protected set; }
        public double CondBC { get; protected set; }
        public double BaseCharge { get; protected set; }
        public double Dqbdvc { get; protected set; }
        public double Dqbdve { get; protected set; }

        /// <summary>
        /// Event called when excess phase calculation is needed
        /// </summary>
        public event EventHandler<ExcessPhaseEventArgs> ExcessPhaseCalculation;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

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
        /// <param name="nodes">Nodes</param>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix<double> matrix)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            // Add a series collector node if necessary
            if (mbp.CollectorResistance.Value > 0)
                CollectorPrimeNode = nodes.Create(Name.Grow("#col")).Index;
            else
                CollectorPrimeNode = collectorNode;

            // Add a series base node if necessary
            if (mbp.BaseResist.Value > 0)
                BasePrimeNode = nodes.Create(Name.Grow("#base")).Index;
            else
                BasePrimeNode = baseNode;

            // Add a series emitter node if necessary
            if (mbp.EmitterResistance.Value > 0)
                EmitterPrimeNode = nodes.Create(Name.Grow("#emit")).Index;
            else
                EmitterPrimeNode = emitterNode;

            // Get matrix pointers
            CollectorCollectorPrimePtr = matrix.GetElement(collectorNode, CollectorPrimeNode);
            BaseBasePrimePtr = matrix.GetElement(baseNode, BasePrimeNode);
            EmitterEmitterPrimePtr = matrix.GetElement(emitterNode, EmitterPrimeNode);
            CollectorPrimeCollectorPtr = matrix.GetElement(CollectorPrimeNode, collectorNode);
            CollectorPrimeBasePrimePtr = matrix.GetElement(CollectorPrimeNode, BasePrimeNode);
            CollectorPrimeEmitterPrimePtr = matrix.GetElement(CollectorPrimeNode, EmitterPrimeNode);
            BasePrimeBasePtr = matrix.GetElement(BasePrimeNode, baseNode);
            BasePrimeCollectorPrimePtr = matrix.GetElement(BasePrimeNode, CollectorPrimeNode);
            BasePrimeEmitterPrimePtr = matrix.GetElement(BasePrimeNode, EmitterPrimeNode);
            EmitterPrimeEmitterPtr = matrix.GetElement(EmitterPrimeNode, emitterNode);
            EmitterPrimeCollectorPrimePtr = matrix.GetElement(EmitterPrimeNode, CollectorPrimeNode);
            EmitterPrimeBasePrimePtr = matrix.GetElement(EmitterPrimeNode, BasePrimeNode);
            CollectorCollectorPtr = matrix.GetElement(collectorNode, collectorNode);
            BaseBasePtr = matrix.GetElement(baseNode, baseNode);
            EmitterEmitterPtr = matrix.GetElement(emitterNode, emitterNode);
            CollectorPrimeCollectorPrimePtr = matrix.GetElement(CollectorPrimeNode, CollectorPrimeNode);
            BasePrimeBasePrimePtr = matrix.GetElement(BasePrimeNode, BasePrimeNode);
            EmitterPrimeEmitterPrimePtr = matrix.GetElement(EmitterPrimeNode, EmitterPrimeNode);
            SubstrateSubstratePtr = matrix.GetElement(substrateNode, substrateNode);
            CollectorPrimeSubstratePtr = matrix.GetElement(CollectorPrimeNode, substrateNode);
            SubstrateCollectorPrimePtr = matrix.GetElement(substrateNode, CollectorPrimeNode);
            BaseCollectorPrimePtr = matrix.GetElement(baseNode, CollectorPrimeNode);
            CollectorPrimeBasePtr = matrix.GetElement(CollectorPrimeNode, baseNode);
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
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Load(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double vt;
            double csat, rbpr, rbpi, gcpr, gepr, oik, c2, vte, oikr, c4, vtc, xjrb, vbe, vbc;
            double vce;
            double vtn, evbe, gben, evben, cben, evbc, gbcn, evbcn, cbcn, q1, q2, arg, sqarg, cc, cex,
                gex, arg1, arg2, cb, gx, gpi, gmu, go, gm;
            double ceqbe, ceqbc;

            vt = bp.Temperature * Circuit.KOverQ;

            // DC model parameters
            csat = temp.TempSaturationCurrent * bp.Area;
            rbpr = mbp.MinimumBaseResistance / bp.Area;
            rbpi = mbp.BaseResist / bp.Area - rbpr;
            gcpr = modeltemp.CollectorConduct * bp.Area;
            gepr = modeltemp.EmitterConduct * bp.Area;
            oik = modeltemp.InverseRollOffForward / bp.Area;
            c2 = temp.TempBELeakageCurrent * bp.Area;
            vte = mbp.LeakBEEmissionCoefficient * vt;
            oikr = modeltemp.InverseRollOffReverse / bp.Area;
            c4 = temp.TempBCLeakageCurrent * bp.Area;
            vtc = mbp.LeakBCEmissionCoefficient * vt;
            xjrb = mbp.BaseCurrentHalfResist * bp.Area;

            // Initialization
            if (state.Init == RealState.InitializationStates.InitJunction && state.Domain == RealState.DomainType.Time && state.UseDC && state.UseIC)
            {
                vbe = mbp.BipolarType * bp.InitialVoltageBE;
                vce = mbp.BipolarType * bp.InitialVoltageCE;
                vbc = vbe - vce;
            }
            else if (state.Init == RealState.InitializationStates.InitJunction && !bp.Off)
            {
                vbe = temp.TempVCritical;
                vbc = 0;
            }
            else if (state.Init == RealState.InitializationStates.InitJunction || (state.Init == RealState.InitializationStates.InitFix && bp.Off))
            {
                vbe = 0;
                vbc = 0;
            }
            else
            {
                // Compute new nonlinear branch voltages
                vbe = mbp.BipolarType * (state.Solution[BasePrimeNode] - state.Solution[EmitterPrimeNode]);
                vbc = mbp.BipolarType * (state.Solution[BasePrimeNode] - state.Solution[CollectorPrimeNode]);

                // Limit nonlinear branch voltages
                bool limited = false;
                vbe = Semiconductor.LimitJunction(vbe, VoltageBE, vt, temp.TempVCritical, ref limited);
                vbc = Semiconductor.LimitJunction(vbc, VoltageBC, vt, temp.TempVCritical, ref limited);
                if (limited)
                    state.IsConvergent = false;
            }

            // Determine dc current and derivitives
            vtn = vt * mbp.EmissionCoefficientForward;
            if (vbe > -5 * vtn)
            {
                evbe = Math.Exp(vbe / vtn);
                CurrentBE = csat * (evbe - 1) + state.Gmin * vbe;
                CondBE = csat * evbe / vtn + state.Gmin;
                if (c2.Equals(0)) // Avoid Exp()
                {
                    cben = 0;
                    gben = 0;
                }
                else
                {
                    evben = Math.Exp(vbe / vte);
                    cben = c2 * (evben - 1);
                    gben = c2 * evben / vte;
                }
            }
            else
            {
                CondBE = -csat / vbe + state.Gmin;
                CurrentBE = CondBE * vbe;
                gben = -c2 / vbe;
                cben = gben * vbe;
            }

            vtn = vt * mbp.EmissionCoefficientReverse;
            if (vbc > -5 * vtn)
            {
                evbc = Math.Exp(vbc / vtn);
                CurrentBC = csat * (evbc - 1) + state.Gmin * vbc;
                CondBC = csat * evbc / vtn + state.Gmin;
                if (c4.Equals(0)) // Avoid Exp()
                {
                    cbcn = 0;
                    gbcn = 0;
                }
                else
                {
                    evbcn = Math.Exp(vbc / vtc);
                    cbcn = c4 * (evbcn - 1);
                    gbcn = c4 * evbcn / vtc;
                }
            }
            else
            {
                CondBC = -csat / vbc + state.Gmin;
                CurrentBC = CondBC * vbc;
                gbcn = -c4 / vbc;
                cbcn = gbcn * vbc;
            }

            // Determine base charge terms
            q1 = 1 / (1 - modeltemp.InverseEarlyVoltForward * vbc - modeltemp.InverseEarlyVoltReverse * vbe);
            if (oik.Equals(0) && oikr.Equals(0)) // Avoid computations
            {
                BaseCharge = q1;
                Dqbdve = q1 * BaseCharge * modeltemp.InverseEarlyVoltReverse;
                Dqbdvc = q1 * BaseCharge * modeltemp.InverseEarlyVoltForward;
            }
            else
            {
                q2 = oik * CurrentBE + oikr * CurrentBC;
                arg = Math.Max(0, 1 + 4 * q2);
                sqarg = 1;
                if (!arg.Equals(0)) // Avoid Sqrt()
                    sqarg = Math.Sqrt(arg);
                BaseCharge = q1 * (1 + sqarg) / 2;
                Dqbdve = q1 * (BaseCharge * modeltemp.InverseEarlyVoltReverse + oik * CondBE / sqarg);
                Dqbdvc = q1 * (BaseCharge * modeltemp.InverseEarlyVoltForward + oikr * CondBC / sqarg);
            }

            // Excess phase calculation
            ExcessPhaseEventArgs ep = new ExcessPhaseEventArgs()
            {
                CollectorCurrent = 0.0,
                ExcessPhaseCurrent = CurrentBE,
                ExcessPhaseConduct = CondBE,
                BaseCharge = BaseCharge
            };
            ExcessPhaseCalculation?.Invoke(this, ep);
            cc = ep.CollectorCurrent;
            cex = ep.ExcessPhaseCurrent;
            gex = ep.ExcessPhaseConduct;

            // Determine dc incremental conductances
            cc = cc + (cex - CurrentBC) / BaseCharge - CurrentBC / temp.TempBetaReverse - cbcn;
            cb = CurrentBE / temp.TempBetaForward + cben + CurrentBC / temp.TempBetaReverse + cbcn;
            gx = rbpr + rbpi / BaseCharge;
            if (!xjrb.Equals(0)) // Avoid calculations
            {
                arg1 = Math.Max(cb / xjrb, 1e-9);
                arg2 = (-1 + Math.Sqrt(1 + 14.59025 * arg1)) / 2.4317 / Math.Sqrt(arg1);
                arg1 = Math.Tan(arg2);
                gx = rbpr + 3 * rbpi * (arg1 - arg2) / arg2 / arg1 / arg1;
            }
            if (!gx.Equals(0)) // Do not divide by 0
                gx = 1 / gx;
            gpi = CondBE / temp.TempBetaForward + gben;
            gmu = CondBC / temp.TempBetaReverse + gbcn;
            go = (CondBC + (cex - CurrentBC) * Dqbdvc / BaseCharge) / BaseCharge;
            gm = (gex - (cex - CurrentBC) * Dqbdve / BaseCharge) / BaseCharge - go;

            VoltageBE = vbe;
            VoltageBC = vbc;
            CollectorCurrent = cc;
            BaseCurrent = cb;
            ConductancePi = gpi;
            ConductanceMu = gmu;
            Transconductance = gm;
            OutputConductance = go;
            ConductanceX = gx;

            // Load current excitation vector
            ceqbe = mbp.BipolarType * (cc + cb - vbe * (gm + go + gpi) + vbc * go);
            ceqbc = mbp.BipolarType * (-cc + vbe * (gm + go) - vbc * (gmu + go));
            state.Rhs[CollectorPrimeNode] += (ceqbc);
            state.Rhs[BasePrimeNode] += (-ceqbe - ceqbc);
            state.Rhs[EmitterPrimeNode] += (ceqbe);

            // Load y matrix
            CollectorCollectorPtr.Add(gcpr);
            BaseBasePtr.Add(gx);
            EmitterEmitterPtr.Add(gepr);
            CollectorPrimeCollectorPrimePtr.Add(gmu + go + gcpr);
            BasePrimeBasePrimePtr.Add(gx + gpi + gmu);
            EmitterPrimeEmitterPrimePtr.Add(gpi + gepr + gm + go);
            CollectorCollectorPrimePtr.Add(-gcpr);
            BaseBasePrimePtr.Add(-gx);
            EmitterEmitterPrimePtr.Add(-gepr);
            CollectorPrimeCollectorPtr.Add(-gcpr);
            CollectorPrimeBasePrimePtr.Add(-gmu + gm);
            CollectorPrimeEmitterPrimePtr.Add(-gm - go);
            BasePrimeBasePtr.Add(-gx);
            BasePrimeCollectorPrimePtr.Add(-gmu);
            BasePrimeEmitterPrimePtr.Add(-gpi);
            EmitterPrimeEmitterPtr.Add(-gepr);
            EmitterPrimeCollectorPrimePtr.Add(-go);
            EmitterPrimeBasePrimePtr.Add(-gpi - gm);
        }

        /// <summary>
        /// Check if the BJT is convergent
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        /// <returns></returns>
        public override bool IsConvergent(BaseSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            var config = simulation.BaseConfiguration;

            double vbe, vbc, delvbe, delvbc, cchat, cbhat, cc, cb;

            vbe = mbp.BipolarType * (state.Solution[BasePrimeNode] - state.Solution[EmitterPrimeNode]);
            vbc = mbp.BipolarType * (state.Solution[BasePrimeNode] - state.Solution[CollectorPrimeNode]);
            delvbe = vbe - VoltageBE;
            delvbc = vbc - VoltageBE;
            cchat = CollectorCurrent + (Transconductance + OutputConductance) * delvbe - (OutputConductance + ConductanceMu) * delvbc;
            cbhat = BaseCurrent + ConductancePi * delvbe + ConductanceMu * delvbc;
            cc = CollectorCurrent;
            cb = BaseCurrent;

            // Check convergence
            double tol = config.RelativeTolerance * Math.Max(Math.Abs(cchat), Math.Abs(cc)) + config.AbsoluteTolerance;
            if (Math.Abs(cchat - cc) > tol)
            {
                state.IsConvergent = false;
                return false;
            }

            tol = config.RelativeTolerance * Math.Max(Math.Abs(cbhat), Math.Abs(cb)) + config.AbsoluteTolerance;
            if (Math.Abs(cbhat - cb) > tol)
            {
                state.IsConvergent = false;
                return false;
            }
            return true;
        }
    }
}
