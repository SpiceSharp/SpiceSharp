using System.Numerics;
using SpiceSharp.Entities;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.LosslessTransmissionLineBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="LosslessTransmissionLine" />.
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
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

        private int _pos1, _neg1, _pos2, _neg2, _int1, _int2, _br1, _br2;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior" /> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, ComponentBindingContext context)
            : base(name, context)
        {
            ComplexState = context.States.GetValue<IComplexSimulationState>();
            _pos1 = ComplexState.Map[context.Nodes[0]];
            _neg1 = ComplexState.Map[context.Nodes[1]];
            _pos2 = ComplexState.Map[context.Nodes[2]];
            _neg2 = ComplexState.Map[context.Nodes[3]];
            _int1 = ComplexState.Map[Internal1];
            _int2 = ComplexState.Map[Internal2];
            _br1 = ComplexState.Map[Branch1];
            _br2 = ComplexState.Map[Branch2];
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver,
                new MatrixLocation(_pos1, _pos1),
                new MatrixLocation(_pos1, _int1),
                new MatrixLocation(_neg1, _br1),
                new MatrixLocation(_pos2, _pos2),
                new MatrixLocation(_neg2, _br2),
                new MatrixLocation(_int1, _pos1),
                new MatrixLocation(_int1, _int1),
                new MatrixLocation(_int1, _br1),
                new MatrixLocation(_int2, _int2),
                new MatrixLocation(_int2, _br2),
                new MatrixLocation(_br1, _neg1),
                new MatrixLocation(_br1, _pos2),
                new MatrixLocation(_br1, _neg2),
                new MatrixLocation(_br1, _int1),
                new MatrixLocation(_br1, _br2),
                new MatrixLocation(_br2, _pos1),
                new MatrixLocation(_br2, _neg1),
                new MatrixLocation(_br2, _neg2),
                new MatrixLocation(_br2, _int2),
                new MatrixLocation(_br2, _br1),
                new MatrixLocation(_pos2, _int2),
                new MatrixLocation(_int2, _pos2));
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// Load the Y-matrix and right-hand side vector for frequency domain analysis.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var laplace = ComplexState.Laplace;
            var factor = Complex.Exp(-laplace * BaseParameters.Delay.Value);
            var y = BaseParameters.Admittance;
            ComplexElements.Add(
                y, -y, -1, y, -1,
                -y, y, 1, y, 1, -1, -factor,
                factor, 1, -factor * BaseParameters.Impedance,
                -factor, factor, -1, 1, -factor * BaseParameters.Impedance,
                -y, -y);
        }
    }
}
