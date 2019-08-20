using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.BipolarBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class FrequencyBehavior : DynamicParameterBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the (external collector, collector) element.
        /// </summary>
        protected MatrixElement<Complex> CCollectorCollectorPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (external base, base) element.
        /// </summary>
        protected MatrixElement<Complex> CBaseBasePrimePtr { get; private set; }

        /// <summary>
        /// Gets the (external emitter, emitter) element.
        /// </summary>
        protected MatrixElement<Complex> CEmitterEmitterPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (collector, external collector) element.
        /// </summary>
        protected MatrixElement<Complex> CCollectorPrimeCollectorPtr { get; private set; }

        /// <summary>
        /// Gets the (collector, base) element.
        /// </summary>
        protected MatrixElement<Complex> CCollectorPrimeBasePrimePtr { get; private set; }

        /// <summary>
        /// Gets the (collector, emitter) element.
        /// </summary>
        protected MatrixElement<Complex> CCollectorPrimeEmitterPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (base, external base) element.
        /// </summary>
        protected MatrixElement<Complex> CBasePrimeBasePtr { get; private set; }

        /// <summary>
        /// Gets the (base, collector) element.
        /// </summary>
        protected MatrixElement<Complex> CBasePrimeCollectorPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (base, emitter) element.
        /// </summary>
        protected MatrixElement<Complex> CBasePrimeEmitterPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (emitter, external emitter) element.
        /// </summary>
        protected MatrixElement<Complex> CEmitterPrimeEmitterPtr { get; private set; }

        /// <summary>
        /// Gets the (emitter, collector) element.
        /// </summary>
        protected MatrixElement<Complex> CEmitterPrimeCollectorPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (emitter, base) element.
        /// </summary>
        protected MatrixElement<Complex> CEmitterPrimeBasePrimePtr { get; private set; }

        /// <summary>
        /// Gets the external (collector, collector) element.
        /// </summary>
        protected MatrixElement<Complex> CCollectorCollectorPtr { get; private set; }

        /// <summary>
        /// Gets the external (base, base) element.
        /// </summary>
        protected MatrixElement<Complex> CBaseBasePtr { get; private set; }

        /// <summary>
        /// Gets the external (emitter, emitter) element.
        /// </summary>
        protected MatrixElement<Complex> CEmitterEmitterPtr { get; private set; }

        /// <summary>
        /// Gets the (collector, collector) element.
        /// </summary>
        protected MatrixElement<Complex> CCollectorPrimeCollectorPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (base, base) element.
        /// </summary>
        protected MatrixElement<Complex> CBasePrimeBasePrimePtr { get; private set; }

        /// <summary>
        /// Gets the (emitter, emitter) element.
        /// </summary>
        protected MatrixElement<Complex> CEmitterPrimeEmitterPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (substrate, substrate) element.
        /// </summary>
        protected MatrixElement<Complex> CSubstrateSubstratePtr { get; private set; }

        /// <summary>
        /// Gets the (collector, substrate) element.
        /// </summary>
        protected MatrixElement<Complex> CCollectorPrimeSubstratePtr { get; private set; }

        /// <summary>
        /// Gets the (substrate, collector) element.
        /// </summary>
        protected MatrixElement<Complex> CSubstrateCollectorPrimePtr { get; private set; }

        /// <summary>
        /// TODO: Check if this is right.
        /// Gets the (external base, collector) element.
        /// </summary>
        protected MatrixElement<Complex> CBaseCollectorPrimePtr { get; private set; }

        /// <summary>
        /// TODO: Check if this is right.
        /// Gets the (collector, external base) element.
        /// </summary>
        protected MatrixElement<Complex> CCollectorPrimeBasePtr { get; private set; }

        private ComplexSimulationState _state;

        /// <summary>
        /// Creates a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            _state = ((FrequencySimulation)simulation).ComplexState;
            var solver = _state.Solver;
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
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            _state = null;
            CCollectorCollectorPrimePtr = null;
            CBaseBasePrimePtr = null;
            CEmitterEmitterPrimePtr = null;
            CCollectorPrimeCollectorPtr = null;
            CCollectorPrimeBasePrimePtr = null;
            CCollectorPrimeEmitterPrimePtr = null;
            CBasePrimeBasePtr = null;
            CBasePrimeCollectorPrimePtr = null;
            CBasePrimeEmitterPrimePtr = null;
            CEmitterPrimeEmitterPtr = null;
            CEmitterPrimeCollectorPrimePtr = null;
            CEmitterPrimeBasePrimePtr = null;
            CCollectorCollectorPtr = null;
            CBaseBasePtr = null;
            CEmitterEmitterPtr = null;
            CCollectorPrimeCollectorPrimePtr = null;
            CBasePrimeBasePrimePtr = null;
            CEmitterPrimeEmitterPrimePtr = null;
            CSubstrateSubstratePtr = null;
            CCollectorPrimeSubstratePtr = null;
            CSubstrateCollectorPrimePtr = null;
            CBaseCollectorPrimePtr = null;
            CCollectorPrimeBasePtr = null;
        }

        /// <summary>
        /// Initialize AC parameters
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
            var vbe = VoltageBe;
            var vbc = VoltageBc;
            var vbx = ModelParameters.BipolarType * (State.Solution[BaseNode] - State.Solution[CollectorPrimeNode]);
            var vcs = ModelParameters.BipolarType * (State.Solution[SubstrateNode] - State.Solution[CollectorPrimeNode]);
            CalculateCapacitances(vbe, vbc, vbx, vcs);
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var cstate = _state;
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
            var xcmcb = Geqcb * cstate.Laplace;

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
