using System.Numerics;
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
        /// Gets the complex matrix elements.
        /// </summary>
        /// <value>
        /// The complex matrix elements.
        /// </value>
        protected ComplexMatrixElementSet ComplexMatrixElements { get; private set; }

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
            ComplexMatrixElements = new ComplexMatrixElementSet(ComplexState.Solver,
                new MatrixPin(CollectorNode, CollectorNode),
                new MatrixPin(BaseNode, BaseNode),
                new MatrixPin(EmitterNode, EmitterNode),
                new MatrixPin(CollectorPrimeNode, CollectorPrimeNode),
                new MatrixPin(BasePrimeNode, BasePrimeNode),
                new MatrixPin(EmitterPrimeNode, EmitterPrimeNode),
                new MatrixPin(CollectorNode, CollectorPrimeNode),
                new MatrixPin(BaseNode, BasePrimeNode),
                new MatrixPin(EmitterNode, EmitterPrimeNode),
                new MatrixPin(CollectorPrimeNode, CollectorNode),
                new MatrixPin(CollectorPrimeNode, BasePrimeNode),
                new MatrixPin(CollectorPrimeNode, EmitterPrimeNode),
                new MatrixPin(BasePrimeNode, BaseNode),
                new MatrixPin(BasePrimeNode, CollectorPrimeNode),
                new MatrixPin(BasePrimeNode, EmitterPrimeNode),
                new MatrixPin(EmitterPrimeNode, EmitterNode),
                new MatrixPin(EmitterPrimeNode, CollectorPrimeNode),
                new MatrixPin(EmitterPrimeNode, BasePrimeNode),
                new MatrixPin(SubstrateNode, SubstrateNode),
                new MatrixPin(CollectorPrimeNode, SubstrateNode),
                new MatrixPin(SubstrateNode, CollectorPrimeNode),
                new MatrixPin(BaseNode, CollectorPrimeNode),
                new MatrixPin(CollectorPrimeNode, BaseNode));
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            ComplexState = null;
            ComplexMatrixElements?.Destroy();
            ComplexMatrixElements = null;
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

            ComplexMatrixElements.Add(
                gcpr,
                gx + xcbx,
                gepr,
                gmu + go + gcpr + xcmu + xccs + xcbx,
                gx + gpi + gmu + xcpi + xcmu + xcmcb,
                gpi + gepr + gm + go + xcpi,
                -gcpr,
                -gx,
                -gepr,
                -gcpr,
                -gmu + gm - xcmu,
                -gm - go,
                -gx,
                -gmu - xcmu - xcmcb,
                -gpi - xcpi,
                -gepr,
                -go + xcmcb,
                -gpi - gm - xcpi - xcmcb,
                xccs,
                -xccs,
                -xccs,
                -xcbx,
                -xcbx);
        }
    }
}
