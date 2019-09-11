using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Circuits;
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
        protected IMatrixElement<Complex> CCollectorCollectorPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (external base, base) element.
        /// </summary>
        protected IMatrixElement<Complex> CBaseBasePrimePtr { get; private set; }

        /// <summary>
        /// Gets the (external emitter, emitter) element.
        /// </summary>
        protected IMatrixElement<Complex> CEmitterEmitterPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (collector, external collector) element.
        /// </summary>
        protected IMatrixElement<Complex> CCollectorPrimeCollectorPtr { get; private set; }

        /// <summary>
        /// Gets the (collector, base) element.
        /// </summary>
        protected IMatrixElement<Complex> CCollectorPrimeBasePrimePtr { get; private set; }

        /// <summary>
        /// Gets the (collector, emitter) element.
        /// </summary>
        protected IMatrixElement<Complex> CCollectorPrimeEmitterPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (base, external base) element.
        /// </summary>
        protected IMatrixElement<Complex> CBasePrimeBasePtr { get; private set; }

        /// <summary>
        /// Gets the (base, collector) element.
        /// </summary>
        protected IMatrixElement<Complex> CBasePrimeCollectorPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (base, emitter) element.
        /// </summary>
        protected IMatrixElement<Complex> CBasePrimeEmitterPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (emitter, external emitter) element.
        /// </summary>
        protected IMatrixElement<Complex> CEmitterPrimeEmitterPtr { get; private set; }

        /// <summary>
        /// Gets the (emitter, collector) element.
        /// </summary>
        protected IMatrixElement<Complex> CEmitterPrimeCollectorPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (emitter, base) element.
        /// </summary>
        protected IMatrixElement<Complex> CEmitterPrimeBasePrimePtr { get; private set; }

        /// <summary>
        /// Gets the external (collector, collector) element.
        /// </summary>
        protected IMatrixElement<Complex> CCollectorCollectorPtr { get; private set; }

        /// <summary>
        /// Gets the external (base, base) element.
        /// </summary>
        protected IMatrixElement<Complex> CBaseBasePtr { get; private set; }

        /// <summary>
        /// Gets the external (emitter, emitter) element.
        /// </summary>
        protected IMatrixElement<Complex> CEmitterEmitterPtr { get; private set; }

        /// <summary>
        /// Gets the (collector, collector) element.
        /// </summary>
        protected IMatrixElement<Complex> CCollectorPrimeCollectorPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (base, base) element.
        /// </summary>
        protected IMatrixElement<Complex> CBasePrimeBasePrimePtr { get; private set; }

        /// <summary>
        /// Gets the (emitter, emitter) element.
        /// </summary>
        protected IMatrixElement<Complex> CEmitterPrimeEmitterPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (substrate, substrate) element.
        /// </summary>
        protected IMatrixElement<Complex> CSubstrateSubstratePtr { get; private set; }

        /// <summary>
        /// Gets the (collector, substrate) element.
        /// </summary>
        protected IMatrixElement<Complex> CCollectorPrimeSubstratePtr { get; private set; }

        /// <summary>
        /// Gets the (substrate, collector) element.
        /// </summary>
        protected IMatrixElement<Complex> CSubstrateCollectorPrimePtr { get; private set; }

        /// <summary>
        /// TODO: Check if this is right.
        /// Gets the (external base, collector) element.
        /// </summary>
        protected IMatrixElement<Complex> CBaseCollectorPrimePtr { get; private set; }

        /// <summary>
        /// TODO: Check if this is right.
        /// Gets the (collector, external base) element.
        /// </summary>
        protected IMatrixElement<Complex> CCollectorPrimeBasePtr { get; private set; }

        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The complex simulation state.
        /// </value>
        protected ComplexSimulationState ComplexState { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            ComplexState = context.States.GetValue<ComplexSimulationState>();
            var solver = ComplexState.Solver;
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
            ComplexState = null;
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
            var vbx = ModelParameters.BipolarType * (BiasingState.Solution[BaseNode] - BiasingState.Solution[CollectorPrimeNode]);
            var vcs = ModelParameters.BipolarType * (BiasingState.Solution[SubstrateNode] - BiasingState.Solution[CollectorPrimeNode]);
            CalculateCapacitances(vbe, vbc, vbx, vcs);
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var cstate = ComplexState;
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
