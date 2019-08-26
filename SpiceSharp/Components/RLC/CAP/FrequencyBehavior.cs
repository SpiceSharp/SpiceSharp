using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="Capacitor"/>.
    /// </summary>
    public class FrequencyBehavior : TemperatureBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the (positive, positive) element.
        /// </summary>
        protected MatrixElement<Complex> PosPosPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, negative) element.
        /// </summary>
        protected MatrixElement<Complex> NegNegPtr { get; private set; }

        /// <summary>
        /// Gets the (positive, negative) element.
        /// </summary>
        protected MatrixElement<Complex> PosNegPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, positive) element.
        /// </summary>
        protected MatrixElement<Complex> NegPosPtr { get; private set; }

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Capacitor voltage")]
        public Complex GetComplexVoltage() => ComplexState.ThrowIfNotBound(this).Solution[PosNode] - ComplexState.Solution[NegNode];

        /// <summary>
        /// Gets the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Capacitor current")]
        public Complex GetComplexCurrent()
        {
            ComplexState.ThrowIfNotBound(this);
            var conductance = ComplexState.Laplace * Capacitance;
            return (ComplexState.Solution[PosNode] - ComplexState.Solution[NegNode]) * conductance;
        }

        /// <summary>
        /// Gets the power.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Capacitor power")]
        public Complex GetComplexPower()
        {
            ComplexState.ThrowIfNotBound(this);
            var conductance = ComplexState.Laplace * Capacitance;
            var voltage = ComplexState.Solution[PosNode] - ComplexState.Solution[NegNode];
            return voltage * Complex.Conjugate(voltage * conductance);
        }

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

            ComplexState = context.States.Get<ComplexSimulationState>();
            var solver = ComplexState.Solver;
            PosPosPtr = solver.GetMatrixElement(PosNode, PosNode);
            NegNegPtr = solver.GetMatrixElement(NegNode, NegNode);
            NegPosPtr = solver.GetMatrixElement(NegNode, PosNode);
            PosNegPtr = solver.GetMatrixElement(PosNode, NegNode);
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
            // Not needed
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var val = ComplexState.Laplace * Capacitance;

            // Load the Y-matrix
            PosPosPtr.Value += val;
            NegNegPtr.Value += val;
            PosNegPtr.Value -= val;
            NegPosPtr.Value -= val;
        }
    }
}
