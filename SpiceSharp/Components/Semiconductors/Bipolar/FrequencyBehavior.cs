using System.Numerics;
using SpiceSharp.Entities;
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
        protected IComplexSimulationState ComplexState { get; private set; }

        private int _collectorNode, _baseNode, _emitterNode, _collectorPrimeNode, _basePrimeNode, _emitterPrimeNode, _substrateNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, ComponentBindingContext context) : base(name, context) 
        {
            ComplexState = context.States.GetValue<IComplexSimulationState>();
            _collectorNode = ComplexState.Map[context.Nodes[0]];
            _baseNode = ComplexState.Map[context.Nodes[1]];
            _emitterNode = ComplexState.Map[context.Nodes[2]];
            _substrateNode = ComplexState.Map[context.Nodes[3]];
            _collectorPrimeNode = ComplexState.Map[CollectorPrime];
            _basePrimeNode = ComplexState.Map[BasePrime];
            _emitterPrimeNode = ComplexState.Map[EmitterPrime];
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver,
                new MatrixLocation(_collectorNode, _collectorNode),
                new MatrixLocation(_baseNode, _baseNode),
                new MatrixLocation(_emitterNode, _emitterNode),
                new MatrixLocation(_collectorPrimeNode, _collectorPrimeNode),
                new MatrixLocation(_basePrimeNode, _basePrimeNode),
                new MatrixLocation(_emitterPrimeNode, _emitterPrimeNode),
                new MatrixLocation(_collectorNode, _collectorPrimeNode),
                new MatrixLocation(_baseNode, _basePrimeNode),
                new MatrixLocation(_emitterNode, _emitterPrimeNode),
                new MatrixLocation(_collectorPrimeNode, _collectorNode),
                new MatrixLocation(_collectorPrimeNode, _basePrimeNode),
                new MatrixLocation(_collectorPrimeNode, _emitterPrimeNode),
                new MatrixLocation(_basePrimeNode, _baseNode),
                new MatrixLocation(_basePrimeNode, _collectorPrimeNode),
                new MatrixLocation(_basePrimeNode, _emitterPrimeNode),
                new MatrixLocation(_emitterPrimeNode, _emitterNode),
                new MatrixLocation(_emitterPrimeNode, _collectorPrimeNode),
                new MatrixLocation(_emitterPrimeNode, _basePrimeNode),
                new MatrixLocation(_substrateNode, _substrateNode),
                new MatrixLocation(_collectorPrimeNode, _substrateNode),
                new MatrixLocation(_substrateNode, _collectorPrimeNode),
                new MatrixLocation(_baseNode, _collectorPrimeNode),
                new MatrixLocation(_collectorPrimeNode, _baseNode));
        }

        /// <summary>
        /// Initialize AC parameters
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
            var vbe = VoltageBe;
            var vbc = VoltageBc;
            var vbx = ModelParameters.BipolarType * (BiasingState.Solution[_baseNode] - BiasingState.Solution[_collectorPrimeNode]);
            var vcs = ModelParameters.BipolarType * (BiasingState.Solution[_substrateNode] - BiasingState.Solution[_collectorPrimeNode]);
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
