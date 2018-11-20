using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Nodes
        /// </summary>
        protected MatrixElement<Complex> CCollectorCollectorPrimePtr { get; private set; }
        protected MatrixElement<Complex> CBaseBasePrimePtr { get; private set; }
        protected MatrixElement<Complex> CEmitterEmitterPrimePtr { get; private set; }
        protected MatrixElement<Complex> CCollectorPrimeCollectorPtr { get; private set; }
        protected MatrixElement<Complex> CCollectorPrimeBasePrimePtr { get; private set; }
        protected MatrixElement<Complex> CCollectorPrimeEmitterPrimePtr { get; private set; }
        protected MatrixElement<Complex> CBasePrimeBasePtr { get; private set; }
        protected MatrixElement<Complex> CBasePrimeCollectorPrimePtr { get; private set; }
        protected MatrixElement<Complex> CBasePrimeEmitterPrimePtr { get; private set; }
        protected MatrixElement<Complex> CEmitterPrimeEmitterPtr { get; private set; }
        protected MatrixElement<Complex> CEmitterPrimeCollectorPrimePtr { get; private set; }
        protected MatrixElement<Complex> CEmitterPrimeBasePrimePtr { get; private set; }
        protected MatrixElement<Complex> CCollectorCollectorPtr { get; private set; }
        protected MatrixElement<Complex> CBaseBasePtr { get; private set; }
        protected MatrixElement<Complex> CEmitterEmitterPtr { get; private set; }
        protected MatrixElement<Complex> CCollectorPrimeCollectorPrimePtr { get; private set; }
        protected MatrixElement<Complex> CBasePrimeBasePrimePtr { get; private set; }
        protected MatrixElement<Complex> CEmitterPrimeEmitterPrimePtr { get; private set; }
        protected MatrixElement<Complex> CSubstrateSubstratePtr { get; private set; }
        protected MatrixElement<Complex> CCollectorPrimeSubstratePtr { get; private set; }
        protected MatrixElement<Complex> CSubstrateCollectorPrimePtr { get; private set; }
        protected MatrixElement<Complex> CBaseCollectorPrimePtr { get; private set; }
        protected MatrixElement<Complex> CCollectorPrimeBasePtr { get; private set; }
        
        /// <summary>
        /// Device methods and properties
        /// </summary>
        [ParameterName("cpi"), ParameterInfo("Internal base to emitter capactance")]
        public double CapBe { get; protected set; }
        [ParameterName("cmu"), ParameterInfo("Internal base to collector capactiance")]
        public double CapBc { get; protected set; }
        [ParameterName("cbx"), ParameterInfo("Base to collector capacitance")]
        public double CapBx { get; protected set; }
        [ParameterName("ccs"), ParameterInfo("Collector to substrate capacitance")]
        public double CapCs { get; protected set; }
        [ParameterName("gcb"), ParameterInfo("Conductance of the C-B junction")]
        public double CondCb { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public void GetEquationPointers(Solver<Complex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // CGet matrix pointers
            CCollectorCollectorPrimePtr = solver.GetMatrixElement(CollectorNode, CollectorPrimeNode);
            CBaseBasePrimePtr = solver.GetMatrixElement(BaseNode, BasePrimeNode);
            CEmitterEmitterPrimePtr = solver.GetMatrixElement(EmitterNode, EmitterPrimeNode);
            CCollectorPrimeCollectorPtr = solver.GetMatrixElement(CollectorPrimeNode, CollectorNode);
            CCollectorPrimeBasePrimePtr = solver.GetMatrixElement(CollectorPrimeNode, BasePrimeNode);
            CCollectorPrimeEmitterPrimePtr = solver.GetMatrixElement(CollectorPrimeNode, EmitterPrimeNode);
            CBasePrimeBasePtr = solver.GetMatrixElement(BasePrimeNode, BaseNode);
            CBasePrimeCollectorPrimePtr = solver.GetMatrixElement(BasePrimeNode, CollectorPrimeNode);
            CBasePrimeEmitterPrimePtr = solver.GetMatrixElement(BasePrimeNode, EmitterPrimeNode);
            CEmitterPrimeEmitterPtr = solver.GetMatrixElement(EmitterPrimeNode, EmitterNode);
            CEmitterPrimeCollectorPrimePtr = solver.GetMatrixElement(EmitterPrimeNode, CollectorPrimeNode);
            CEmitterPrimeBasePrimePtr = solver.GetMatrixElement(EmitterPrimeNode, BasePrimeNode);
            CCollectorCollectorPtr = solver.GetMatrixElement(CollectorNode, CollectorNode);
            CBaseBasePtr = solver.GetMatrixElement(BaseNode, BaseNode);
            CEmitterEmitterPtr = solver.GetMatrixElement(EmitterNode, EmitterNode);
            CCollectorPrimeCollectorPrimePtr = solver.GetMatrixElement(CollectorPrimeNode, CollectorPrimeNode);
            CBasePrimeBasePrimePtr = solver.GetMatrixElement(BasePrimeNode, BasePrimeNode);
            CEmitterPrimeEmitterPrimePtr = solver.GetMatrixElement(EmitterPrimeNode, EmitterPrimeNode);
            CSubstrateSubstratePtr = solver.GetMatrixElement(SubstrateNode, SubstrateNode);
            CCollectorPrimeSubstratePtr = solver.GetMatrixElement(CollectorPrimeNode, SubstrateNode);
            CSubstrateCollectorPrimePtr = solver.GetMatrixElement(SubstrateNode, CollectorPrimeNode);
            CBaseCollectorPrimePtr = solver.GetMatrixElement(BaseNode, CollectorPrimeNode);
            CCollectorPrimeBasePtr = solver.GetMatrixElement(CollectorPrimeNode, BaseNode);
        }

        /// <summary>
        /// Initialize AC parameters
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public void InitializeParameters(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double arg, sarg;
            double f2, f3;

            // Get voltages
            var state = simulation.RealState;
            var vbe = VoltageBe;
            var vbc = VoltageBc;
            var vbx = ModelParameters.BipolarType * (state.Solution[BaseNode] - state.Solution[CollectorPrimeNode]);
            var vcs = ModelParameters.BipolarType * (state.Solution[SubstrateNode] - state.Solution[CollectorPrimeNode]);

            // Get shared parameters
            var cbe = CurrentBe;
            var gbe = CondBe;
            var gbc = CondBc;
            var qb = BaseCharge;
            var dqbdve = Dqbdve;
            var dqbdvc = Dqbdvc;

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
                gbe = (gbe * (1 + arg2) - cbe * dqbdve) / qb;
                CondCb = tf * (arg3 - cbe * dqbdvc) / qb;
            }
            if (vbe < fcpe)
            {
                arg = 1 - vbe / pe;
                sarg = Math.Exp(-xme * Math.Log(arg));
                CapBe = tf * gbe + czbe * sarg;
            }
            else
            {
                f2 = ModelTemperature.F2;
                f3 = ModelTemperature.F3;
                var czbef2 = czbe / f2;
                CapBe = tf * gbe + czbef2 * (f3 + xme * vbe / pe);
            }

            var fcpc = TempFactor4;
            f2 = ModelTemperature.F6;
            f3 = ModelTemperature.F7;
            if (vbc < fcpc)
            {
                arg = 1 - vbc / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                CapBc = tr * gbc + czbc * sarg;
            }
            else
            {
                var czbcf2 = czbc / f2;
                CapBc = tr * gbc + czbcf2 * (f3 + xmc * vbc / pc);
            }
            if (vbx < fcpc)
            {
                arg = 1 - vbx / pc;
                sarg = Math.Exp(-xmc * Math.Log(arg));
                CapBx = czbx * sarg;
            }
            else
            {
                var czbxf2 = czbx / f2;
                CapBx = czbxf2 * (f3 + xmc * vbx / pc);
            }
            if (vcs < 0)
            {
                arg = 1 - vcs / ps;
                sarg = Math.Exp(-xms * Math.Log(arg));
                CapCs = czcs * sarg;
            }
            else
            {
                CapCs = czcs * (1 + xms * vcs / ps);
            }
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var cstate = simulation.ComplexState;
            var gcpr = ModelTemperature.CollectorConduct * BaseParameters.Area;
            var gepr = ModelTemperature.EmitterConduct * BaseParameters.Area;
            var gpi = ConductancePi;
            var gmu = ConductanceMu;
            Complex gm = Transconductance;
            var go = OutputConductance;
            var td = ModelTemperature.ExcessPhaseFactor;
            if (!td.Equals(0)) // Avoid computations
            {
                var arg = td * cstate.Laplace;

                gm = gm + go;
                gm = gm * Complex.Exp(-arg);
                gm = gm - go;
            }
            var gx = ConductanceX;
            var xcpi = CapBe * cstate.Laplace;
            var xcmu = CapBc * cstate.Laplace;
            var xcbx = CapBx * cstate.Laplace;
            var xccs = CapCs * cstate.Laplace;
            var xcmcb = CondCb * cstate.Laplace;

            CCollectorCollectorPtr.Value += gcpr;
            CBaseBasePtr.Value += gx + xcbx;
            CEmitterEmitterPtr.Value += gepr;
            CCollectorPrimeCollectorPrimePtr.Value += gmu + go + gcpr + xcmu + xccs + xcbx;
            CBasePrimeBasePrimePtr.Value += gx + gpi + gmu + xcpi + xcmu + xcmcb;
            CEmitterPrimeEmitterPrimePtr.Value += gpi + gepr + gm + go + xcpi;
            CCollectorCollectorPrimePtr.Value += -gcpr;
            CBaseBasePrimePtr.Value += -gx;
            CEmitterEmitterPrimePtr.Value += -gepr;
            CCollectorPrimeCollectorPtr.Value += -gcpr;
            CCollectorPrimeBasePrimePtr.Value += -gmu + gm - xcmu;
            CCollectorPrimeEmitterPrimePtr.Value += -gm - go;
            CBasePrimeBasePtr.Value += -gx;
            CBasePrimeCollectorPrimePtr.Value += -gmu - xcmu - xcmcb;
            CBasePrimeEmitterPrimePtr.Value += -gpi - xcpi;
            CEmitterPrimeEmitterPtr.Value += -gepr;
            CEmitterPrimeCollectorPrimePtr.Value += -go + xcmcb;
            CEmitterPrimeBasePrimePtr.Value += -gpi - gm - xcpi - xcmcb;
            CSubstrateSubstratePtr.Value += xccs;
            CCollectorPrimeSubstratePtr.Value += -xccs;
            CSubstrateCollectorPrimePtr.Value += -xccs;
            CBaseCollectorPrimePtr.Value += -xcbx;
            CCollectorPrimeBasePtr.Value += -xcbx;
        }
    }
}
