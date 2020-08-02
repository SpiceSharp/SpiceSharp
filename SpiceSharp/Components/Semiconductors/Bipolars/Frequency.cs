using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Numerics;
using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Bipolars
{
    /// <summary>
    /// Small-signal behavior for <see cref="BipolarJunctionTransistor"/>.
    /// </summary>
    /// <seealso cref="Dynamic"/>
    /// <seealso cref="IFrequencyBehavior"/>
    [BehaviorFor(typeof(BipolarJunctionTransistor), typeof(IFrequencyBehavior), 2)]
    public class Frequency : Dynamic,
        IFrequencyBehavior
    {
        private readonly ElementSet<Complex> _elements;
        private readonly IComplexSimulationState _complex;
        private readonly int _collectorNode, _baseNode, _emitterNode, _collectorPrimeNode, _basePrimeNode, _emitterPrimeNode, _substrateNode;

        /// <summary>
        /// Gets the internal collector node.
        /// </summary>
        /// <value>
        /// The internal collector node.
        /// </value>
        protected new IVariable<Complex> CollectorPrime { get; private set; }

        /// <summary>
        /// Gets the internal base node.
        /// </summary>
        /// <value>
        /// The internal base node.
        /// </value>
        protected new IVariable<Complex> BasePrime { get; private set; }

        /// <summary>
        /// Gets the internal emitter node.
        /// </summary>
        /// <value>
        /// The internal emitter node.
        /// </value>
        protected new IVariable<Complex> EmitterPrime { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Frequency(IComponentBindingContext context)
            : base(context)
        {
            _complex = context.GetState<IComplexSimulationState>();

            CollectorPrime = _complex.GetSharedVariable(context.Nodes[0]);
            BasePrime = _complex.GetSharedVariable(context.Nodes[1]);
            EmitterPrime = _complex.GetSharedVariable(context.Nodes[2]);
            _collectorNode = _complex.Map[CollectorPrime];
            _baseNode = _complex.Map[BasePrime];
            _emitterNode = _complex.Map[EmitterPrime];
            _substrateNode = _complex.Map[_complex.GetSharedVariable(context.Nodes[3])];

            // Add a series collector node if necessary
            if (ModelParameters.CollectorResistance > 0)
                CollectorPrime = _complex.CreatePrivateVariable(Name.Combine("col"), Units.Volt);
            _collectorPrimeNode = _complex.Map[CollectorPrime];

            // Add a series base node if necessary
            if (ModelParameters.BaseResist > 0)
                BasePrime = _complex.CreatePrivateVariable(Name.Combine("base"), Units.Volt);
            _basePrimeNode = _complex.Map[BasePrime];

            // Add a series emitter node if necessary
            if (ModelParameters.EmitterResistance > 0)
                EmitterPrime = _complex.CreatePrivateVariable(Name.Combine("emit"), Units.Volt);
            _emitterPrimeNode = _complex.Map[EmitterPrime];

            _elements = new ElementSet<Complex>(_complex.Solver,
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

        /// <inheritdoc/>
        void IFrequencyBehavior.InitializeParameters()
        {
            var vbe = VoltageBe;
            var vbc = VoltageBc;
            var vbx = ModelParameters.BipolarType * (BiasingState.Solution[_baseNode] - BiasingState.Solution[_collectorPrimeNode]);
            var vcs = ModelParameters.BipolarType * (BiasingState.Solution[_substrateNode] - BiasingState.Solution[_collectorPrimeNode]);
            CalculateCapacitances(vbe, vbc, vbx, vcs);
        }

        /// <inheritdoc/>
        void IFrequencyBehavior.Load()
        {
            var cstate = _complex;
            var gcpr = ModelTemperature.CollectorConduct * Parameters.Area;
            var gepr = ModelTemperature.EmitterConduct * Parameters.Area;
            var gpi = ConductancePi;
            var gmu = ConductanceMu;
            Complex gm = Transconductance;
            var go = OutputConductance;
            var td = ModelTemperature.ExcessPhaseFactor;
            if (!td.Equals(0)) // Avoid computations
            {
                var arg = td * cstate.Laplace;

                gm += go;
                gm *= Complex.Exp(-arg);
                gm -= go;
            }
            var gx = ConductanceX;
            var xcpi = CapBe * cstate.Laplace;
            var xcmu = CapBc * cstate.Laplace;
            var xcbx = CapBx * cstate.Laplace;
            var xccs = CapCs * cstate.Laplace;
            var xcmcb = Geqcb * cstate.Laplace;

            _elements.Add(
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
