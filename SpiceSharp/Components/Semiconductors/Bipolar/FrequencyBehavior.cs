using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

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
        protected ElementSet<Complex> ComplexElements { get; private set; }

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
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver,
                new MatrixLocation(CollectorNode, CollectorNode),
                new MatrixLocation(BaseNode, BaseNode),
                new MatrixLocation(EmitterNode, EmitterNode),
                new MatrixLocation(CollectorPrimeNode, CollectorPrimeNode),
                new MatrixLocation(BasePrimeNode, BasePrimeNode),
                new MatrixLocation(EmitterPrimeNode, EmitterPrimeNode),
                new MatrixLocation(CollectorNode, CollectorPrimeNode),
                new MatrixLocation(BaseNode, BasePrimeNode),
                new MatrixLocation(EmitterNode, EmitterPrimeNode),
                new MatrixLocation(CollectorPrimeNode, CollectorNode),
                new MatrixLocation(CollectorPrimeNode, BasePrimeNode),
                new MatrixLocation(CollectorPrimeNode, EmitterPrimeNode),
                new MatrixLocation(BasePrimeNode, BaseNode),
                new MatrixLocation(BasePrimeNode, CollectorPrimeNode),
                new MatrixLocation(BasePrimeNode, EmitterPrimeNode),
                new MatrixLocation(EmitterPrimeNode, EmitterNode),
                new MatrixLocation(EmitterPrimeNode, CollectorPrimeNode),
                new MatrixLocation(EmitterPrimeNode, BasePrimeNode),
                new MatrixLocation(SubstrateNode, SubstrateNode),
                new MatrixLocation(CollectorPrimeNode, SubstrateNode),
                new MatrixLocation(SubstrateNode, CollectorPrimeNode),
                new MatrixLocation(BaseNode, CollectorPrimeNode),
                new MatrixLocation(CollectorPrimeNode, BaseNode));
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            ComplexState = null;
            ComplexElements?.Destroy();
            ComplexElements = null;
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

            ComplexElements.Add(
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
