using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="Capacitor" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.CapacitorBehaviors.TemperatureBehavior" />
    /// <seealso cref="SpiceSharp.Behaviors.ITimeBehavior" />
    public class TransientBehavior : TemperatureBehavior, ITimeBehavior
    {
        /// <summary>
        /// Gets the current through the capacitor.
        /// </summary>
        [ParameterName("i"), ParameterInfo("Device current")]
        public double Current => QCap.Derivative;

        /// <summary>
        /// Gets the instantaneous power dissipated by the capacitor.
        /// </summary>
        /// <param name="state">The simulation state.</param>
        /// <returns></returns>
        [ParameterName("p"), ParameterInfo("Instantaneous device power")]
        public double GetPower(BaseSimulationState state)
        {
			state.ThrowIfNull(nameof(state));

            return QCap.Derivative * (state.Solution[PosNode] - state.Solution[NegNode]);
        }

        /// <summary>
        /// Gets the voltage across the capacitor.
        /// </summary>
        /// <param name="state">The simulation state.</param>
        /// <returns></returns>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double GetVoltage(BaseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return state.Solution[PosNode] - state.Solution[NegNode];
        }

        /// <summary>
        /// Gets the (positive, positive) element.
        /// </summary>
        protected MatrixElement<double> PosPosPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, negative) element.
        /// </summary>
        protected MatrixElement<double> NegNegPtr { get; private set; }

        /// <summary>
        /// Gets the (positive, negative) element.
        /// </summary>
        protected MatrixElement<double> PosNegPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, positive) element.
        /// </summary>
        protected MatrixElement<double> NegPosPtr { get; private set; }

        /// <summary>
        /// Gets the positive RHS element.
        /// </summary>
        protected VectorElement<double> PosPtr { get; private set; }

        /// <summary>
        /// Gets the negative RHS element.
        /// </summary>
        protected VectorElement<double> NegPtr { get; private set; }

        /// <summary>
        /// Gets the state tracking the charge.
        /// </summary>
        protected StateDerivative QCap { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">Name of the behavior</param>
        public TransientBehavior(string name) : base(name) { }
        
        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="method"></param>
        public void CreateStates(IntegrationMethod method)
        {
			method.ThrowIfNull(nameof(method));
            QCap = method.CreateDerivative();
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public void GetEquationPointers(Solver<double> solver)
        {
            solver.ThrowIfNull(nameof(solver));

            // Get matrix elements
            PosPosPtr = solver.GetMatrixElement(PosNode, PosNode);
            NegNegPtr = solver.GetMatrixElement(NegNode, NegNode);
            NegPosPtr = solver.GetMatrixElement(NegNode, PosNode);
            PosNegPtr = solver.GetMatrixElement(PosNode, NegNode);

            // Get rhs elements
            PosPtr = solver.GetRhsElement(PosNode);
            NegPtr = solver.GetRhsElement(NegNode);
        }

        /// <summary>
        /// Calculate the state for DC
        /// </summary>
        /// <param name="simulation"></param>
        public void GetDcState(TimeSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            // Calculate the state for DC
            var sol = simulation.RealState.Solution;
            if (BaseParameters.InitialCondition.Given)
                QCap.Current = BaseParameters.InitialCondition;
            else
                QCap.Current = Capacitance * (sol[PosNode] - sol[NegNode]);
        }
        
        /// <summary>
        /// Execute behavior for DC and Transient analysis
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public void Transient(TimeSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            var state = simulation.RealState;
            var vcap = state.Solution[PosNode] - state.Solution[NegNode];

            // Integrate
            QCap.Current = Capacitance * vcap;
            QCap.Integrate();
            var geq = QCap.Jacobian(Capacitance);
            var ceq = QCap.RhsCurrent();

            // Load matrix
            PosPosPtr.Value += geq;
            NegNegPtr.Value += geq;
            PosNegPtr.Value -= geq;
            NegPosPtr.Value -= geq;

            // Load Rhs vector
            PosPtr.Value -= ceq;
            NegPtr.Value += ceq;
        }
    }
}
