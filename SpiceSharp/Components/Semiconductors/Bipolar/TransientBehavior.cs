using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class TransientBehavior : BiasingBehavior, ITimeBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private IntegrationMethod _method;

        /// <summary>
        /// Nodes
        /// </summary>
        protected VectorElement<double> BasePtr { get; private set; }
        protected VectorElement<double> SubstratePtr { get; private set; }

        /// <summary>
        /// Device methods and properties
        /// </summary>
        [ParameterName("qbe"), ParameterInfo("Charge storage B-E junction")]
        public double ChargeBe => StateChargeBe.Current;
        // TODO: Merge this with biasing behavior properties?
        [ParameterName("cqbe"), ParameterInfo("Capacitance current due to charges in the B-E junction")]
        public new double CurrentBe => StateChargeBe.Derivative;
        [ParameterName("qbc"), ParameterInfo("Charge storage B-C junction")]
        public double ChargeBc => StateChargeBc.Current;
        [ParameterName("cqbc"), ParameterInfo("Capacitance current due to charges in the B-C junction")]
        public new double CurrentBc => StateChargeBc.Derivative;
        [ParameterName("qcs"), ParameterInfo("Charge storage C-S junction")]
        public double ChargeCs => StateChargeCs.Current;
        [ParameterName("cqcs"), ParameterInfo("Capacitance current due to charges in the C-S junction")]
        public double CurrentCs => StateChargeCs.Derivative;
        [ParameterName("qbx"), ParameterInfo("Charge storage B-X junction")]
        public double ChargeBx => StateChargeBx.Current;
        [ParameterName("cqbx"), ParameterInfo("Capacitance current due to charges in the B-X junction")]
        public double CurrentBx => StateChargeBx.Derivative;
        [ParameterName("cexbc"), ParameterInfo("Total capacitance in B-X junction")]
        public double CurrentExBc => StateExcessPhaseCurrentBc.Current;
        [ParameterName("cpi"), ParameterInfo("Internal base to emitter capactance")]
        public double CapBe { get; protected set; }
        [ParameterName("cmu"), ParameterInfo("Internal base to collector capactiance")]
        public double CapBc { get; protected set; }
        [ParameterName("cbx"), ParameterInfo("Base to collector capacitance")]
        public double CapBx { get; protected set; }
        [ParameterName("ccs"), ParameterInfo("Collector to substrate capacitance")]
        public double CapCs { get; protected set; }

        /// <summary>
        /// States
        /// </summary>
        protected StateDerivative StateChargeBe { get; private set; }
        protected StateDerivative StateChargeBc { get; private set; }
        protected StateDerivative StateChargeCs { get; private set; }
        protected StateDerivative StateChargeBx { get; private set; }
        protected StateHistory StateExcessPhaseCurrentBc { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(string name) : base(name) { }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Matrix</param>
        public void GetEquationPointers(Solver<double> solver)
        {
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));

            // Get rhs pointers
            BasePtr = solver.GetRhsElement(BaseNode);
            SubstratePtr = solver.GetRhsElement(SubstrateNode);
        }
        
        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="method"></param>
        public void CreateStates(IntegrationMethod method)
        {
            _method = method ?? throw new ArgumentNullException(nameof(method));

            // We just need a history without integration here
            StateChargeBe = method.CreateDerivative();
            StateChargeBc = method.CreateDerivative();
            StateChargeCs = method.CreateDerivative();

            // Spice 3f5 does not include this state for LTE calculations
            StateChargeBx = method.CreateDerivative(false);

            StateExcessPhaseCurrentBc = method.CreateHistory();
        }

        /// <summary>
        /// Calculate state variables
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public void GetDcState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double arg;
            double sarg, f1, f2, f3;

            var cbe = base.CurrentBe;
            var cbc = base.CurrentBc;
            var qb = BaseCharge;

            var vbe = VoltageBe;
            var vbc = VoltageBc;
            var vbx = ModelParameters.BipolarType * (state.Solution[BaseNode] - state.Solution[CollectorPrimeNode]);
            var vcs = ModelParameters.BipolarType * (state.Solution[SubstrateNode] - state.Solution[CollectorPrimeNode]);

            StateExcessPhaseCurrentBc.Current = CurrentBe / BaseCharge;

            // Charge storage elements
            double tf = ModelParameters.TransitTimeForward;
            double tr = ModelParameters.TransitTimeReverse;
            var czbe = TempBeCap * BaseParameters.Area;
            var pe = TempBePotential;
            double xme = ModelParameters.JunctionExpBe;
            double cdis = ModelParameters.BaseFractionBcCap;
            var ctot = TempBcCap * BaseParameters.Area;
            var czbc = ctot * cdis;
            var czbx = ctot - czbc;
            var pc = TempBcPotential;
            double xmc = ModelParameters.JunctionExpBc;
            var fcpe = TempDepletionCap;
            var czcs = ModelParameters.CapCs * BaseParameters.Area;
            double ps = ModelParameters.PotentialSubstrate;
            double xms = ModelParameters.ExponentialSubstrate;
            double xtf = ModelParameters.TransitTimeBiasCoefficientForward;
            var ovtf = ModelTemperature.TransitTimeVoltageBcFactor;
            var xjtf = ModelParameters.TransitTimeHighCurrentForward * BaseParameters.Area;
            if (!tf.Equals(0) && vbe > 0) // Avoid computations
            {
                double argtf = 0;
                if (!xtf.Equals(0)) // Avoid computations
                {
                    argtf = xtf;
                    if (!ovtf.Equals(0)) // Avoid expensive Exp()
                    {
                        argtf = argtf * Math.Exp(vbc * ovtf);
                    }

                    if (!xjtf.Equals(0)) // Avoid computations
                    {
                        var tmp = cbe / (cbe + xjtf);
                        argtf = argtf * tmp * tmp;
                    }
                }
                cbe = cbe * (1 + argtf) / qb;
            }
            if (vbe < fcpe)
            {
                arg = 1 - vbe / pe;
                sarg = Math.Exp(-xme * Math.Log(arg));
                StateChargeBe.Current = tf * cbe + pe * czbe * (1 - arg * sarg) / (1 - xme);
            }
            else
            {
                f1 = TempFactor1;
                f2 = ModelTemperature.F2;
                f3 = ModelTemperature.F3;
                var czbef2 = czbe / f2;
                StateChargeBe.Current = tf * cbe + czbe * f1 + czbef2 * (f3 * (vbe - fcpe) + xme / (pe + pe) * (vbe * vbe -
                     fcpe * fcpe));
            }
            var fcpc = TempFactor4;
            f1 = TempFactor5;
            f2 = ModelTemperature.F6;
            f3 = ModelTemperature.F7;
            if (vbc < fcpc)
            {
                arg = 1 - vbc / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                StateChargeBc.Current = tr * cbc + pc * czbc * (1 - arg * sarg) / (1 - xmc);
            }
            else
            {
                var czbcf2 = czbc / f2;
                StateChargeBc.Current = tr * cbc + czbc * f1 + czbcf2 * (f3 * (vbc - fcpc) + xmc / (pc + pc) * (vbc * vbc -
                     fcpc * fcpc));
            }
            if (vbx < fcpc)
            {
                arg = 1 - vbx / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                StateChargeBx.Current = pc * czbx * (1 - arg * sarg) / (1 - xmc);
            }
            else
            {
                var czbxf2 = czbx / f2;
                StateChargeBx.Current = czbx * f1 + czbxf2 * (f3 * (vbx - fcpc) + xmc / (pc + pc) * (vbx * vbx - fcpc * fcpc));
            }
            if (vcs < 0)
            {
                arg = 1 - vcs / ps;
                sarg = Math.Exp(-xms * Math.Log(arg));
                StateChargeCs.Current = ps * czcs * (1 - arg * sarg) / (1 - xms);
            }
            else
            {
                StateChargeCs.Current = vcs * czcs * (1 + xms * vcs / (2 * ps));
            }
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public void Transient(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double arg;
            double sarg, f1, f2, f3;

            var cbe = base.CurrentBe;
            var cbc = base.CurrentBc;
            var gbe = CondBe;
            var gbc = CondBc;
            var qb = BaseCharge;
            double geqcb = 0;

            var gpi = 0.0;
            var gmu = 0.0;
            var cb = 0.0;
            var cc = 0.0;

            var vbe = VoltageBe;
            var vbc = VoltageBc;
            var vbx = ModelParameters.BipolarType * (state.Solution[BaseNode] - state.Solution[CollectorPrimeNode]);
            var vcs = ModelParameters.BipolarType * (state.Solution[SubstrateNode] - state.Solution[CollectorPrimeNode]);

            // Charge storage elements
            double tf = ModelParameters.TransitTimeForward;
            double tr = ModelParameters.TransitTimeReverse;
            var czbe = TempBeCap * BaseParameters.Area;
            var pe = TempBePotential;
            double xme = ModelParameters.JunctionExpBe;
            double cdis = ModelParameters.BaseFractionBcCap;
            var ctot = TempBcCap * BaseParameters.Area;
            var czbc = ctot * cdis;
            var czbx = ctot - czbc;
            var pc = TempBcPotential;
            double xmc = ModelParameters.JunctionExpBc;
            var fcpe = TempDepletionCap;
            var czcs = ModelParameters.CapCs * BaseParameters.Area;
            double ps = ModelParameters.PotentialSubstrate;
            double xms = ModelParameters.ExponentialSubstrate;
            double xtf = ModelParameters.TransitTimeBiasCoefficientForward;
            var ovtf = ModelTemperature.TransitTimeVoltageBcFactor;
            var xjtf = ModelParameters.TransitTimeHighCurrentForward * BaseParameters.Area;
            if (!tf.Equals(0) && vbe > 0) // Avoid computations
            {
                double argtf = 0;
                double arg2 = 0;
                double arg3 = 0;
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
                        var tmp = cbe / (cbe + xjtf);
                        argtf = argtf * tmp * tmp;
                        arg2 = argtf * (3 - tmp - tmp);
                    }
                    arg3 = cbe * argtf * ovtf;
                }
                cbe = cbe * (1 + argtf) / qb;
                gbe = (gbe * (1 + arg2) - cbe * Dqbdve) / qb;
                geqcb = tf * (arg3 - cbe * Dqbdvc) / qb;
            }
            if (vbe < fcpe)
            {
                arg = 1 - vbe / pe;
                sarg = Math.Exp(-xme * Math.Log(arg));
                StateChargeBe.Current = tf * cbe + pe * czbe * (1 - arg * sarg) / (1 - xme);
                CapBe = tf * gbe + czbe * sarg;
            }
            else
            {
                f1 = TempFactor1;
                f2 = ModelTemperature.F2;
                f3 = ModelTemperature.F3;
                var czbef2 = czbe / f2;
                StateChargeBe.Current = tf * cbe + czbe * f1 + czbef2 * (f3 * (vbe - fcpe) + xme / (pe + pe) * (vbe * vbe -
                     fcpe * fcpe));
                CapBe = tf * gbe + czbef2 * (f3 + xme * vbe / pe);
            }
            var fcpc = TempFactor4;
            f1 = TempFactor5;
            f2 = ModelTemperature.F6;
            f3 = ModelTemperature.F7;
            if (vbc < fcpc)
            {
                arg = 1 - vbc / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                StateChargeBc.Current = tr * cbc + pc * czbc * (1 - arg * sarg) / (1 - xmc);
                CapBc = tr * gbc + czbc * sarg;
            }
            else
            {
                var czbcf2 = czbc / f2;
                StateChargeBc.Current = tr * cbc + czbc * f1 + czbcf2 * (f3 * (vbc - fcpc) + xmc / (pc + pc) * (vbc * vbc -
                     fcpc * fcpc));
                CapBc = tr * gbc + czbcf2 * (f3 + xmc * vbc / pc);
            }
            if (vbx < fcpc)
            {
                arg = 1 - vbx / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                StateChargeBx.Current = pc * czbx * (1 - arg * sarg) / (1 - xmc);
                CapBx = czbx * sarg;
            }
            else
            {
                var czbxf2 = czbx / f2;
                StateChargeBx.Current = czbx * f1 + czbxf2 * (f3 * (vbx - fcpc) + xmc / (pc + pc) * (vbx * vbx - fcpc * fcpc));
                CapBx = czbxf2 * (f3 + xmc * vbx / pc);
            }
            if (vcs < 0)
            {
                arg = 1 - vcs / ps;
                sarg = Math.Exp(-xms * Math.Log(arg));
                StateChargeCs.Current = ps * czcs * (1 - arg * sarg) / (1 - xms);
                CapCs = czcs * sarg;
            }
            else
            {
                StateChargeCs.Current = vcs * czcs * (1 + xms * vcs / (2 * ps));
                CapCs = czcs * (1 + xms * vcs / ps);
            }

            StateChargeBe.Integrate();
            geqcb = StateChargeBe.Jacobian(geqcb); // Multiplies geqcb with method.Slope (ag[0])
            gpi += StateChargeBe.Jacobian(CapBe);
            cb += StateChargeBe.Derivative;
            StateChargeBc.Integrate();
            gmu += StateChargeBc.Jacobian(CapBc);
            cb += StateChargeBc.Derivative;
            cc -= StateChargeBc.Derivative;

            // Charge storage for c-s and b-x junctions
            StateChargeCs.Integrate();
            var gccs = StateChargeCs.Jacobian(CapCs);
            StateChargeBx.Integrate();
            var geqbx = StateChargeBx.Jacobian(CapBx);

            // Load current excitation vector
            var ceqcs = ModelParameters.BipolarType * (StateChargeCs.Derivative - vcs * gccs);
            var ceqbx = ModelParameters.BipolarType * (StateChargeBx.Derivative - vbx * geqbx);
            var ceqbe = ModelParameters.BipolarType * (cc + cb - vbe * gpi + vbc * -geqcb);
            var ceqbc = ModelParameters.BipolarType * (-cc + - vbc * gmu);

            // Load Rhs-vector
            BasePtr.Value += -ceqbx;
            CollectorPrimePtr.Value += ceqcs + ceqbx + ceqbc;
            BasePrimePtr.Value += -ceqbe - ceqbc;
            EmitterPrimePtr.Value += ceqbe;
            SubstratePtr.Value += -ceqcs;

            // Load Y-matrix
            BaseBasePtr.Value += geqbx;
            CollectorPrimeCollectorPrimePtr.Value += gmu + gccs + geqbx;
            BasePrimeBasePrimePtr.Value += gpi + gmu + geqcb;
            EmitterPrimeEmitterPrimePtr.Value += gpi;
            CollectorPrimeBasePrimePtr.Value += -gmu;
            BasePrimeCollectorPrimePtr.Value += -gmu - geqcb;
            BasePrimeEmitterPrimePtr.Value += -gpi;
            EmitterPrimeCollectorPrimePtr.Value += geqcb;
            EmitterPrimeBasePrimePtr.Value += -gpi - geqcb;
            SubstrateSubstratePtr.Value += gccs;
            CollectorPrimeSubstratePtr.Value += -gccs;
            SubstrateCollectorPrimePtr.Value += -gccs;
            BaseCollectorPrimePtr.Value += -geqbx;
            CollectorPrimeBasePtr.Value += -geqbx;
        }

        /// <summary>
        /// Excess phase calculation.
        /// </summary>
        /// <param name="cc">The collector current.</param>
        /// <param name="cex">The excess phase current.</param>
        /// <param name="gex">The excess phase conductance.</param>
        protected override void ExcessPhaseCalculation(ref double cc, ref double cex, ref double gex)
        {
            var td = ModelTemperature.ExcessPhaseFactor;
            if (td.Equals(0))
            {
                StateExcessPhaseCurrentBc.Current = cex;
                return;
            }
            
            /* 
             * weil's approx. for excess phase applied with backward - 
             * euler integration
             */
            var cbe = cex;
            var gbe = gex;

            var delta = _method.GetTimestep(0);
            var prevdelta = _method.GetTimestep(1);
            var arg1 = delta / td;
            var arg2 = 3 * arg1;
            arg1 = arg2 * arg1;
            var denom = 1 + arg1 + arg2;
            var arg3 = arg1 / denom;
            /* Still need a place for this...
            if (state.Init == State.InitFlags.InitTransient)
            {
                state.States[1][State + Cexbc] = cbe / qb;
                state.States[2][State + Cexbc] = state.States[1][State + Cexbc];
            } */
            cc = (StateExcessPhaseCurrentBc[1] * (1 + delta / prevdelta + arg2) 
                - StateExcessPhaseCurrentBc[2] * delta / prevdelta) / denom;
            cex = cbe * arg3;
            gex = gbe * arg3;
            StateExcessPhaseCurrentBc.Current = cc + cex / BaseCharge;
        }
    }
}
