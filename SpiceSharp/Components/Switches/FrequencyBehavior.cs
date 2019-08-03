using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SwitchBehaviors
{
    /// <summary>
    /// Frequency behavior for switches.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.SwitchBehaviors.BiasingBehavior" />
    /// <seealso cref="SpiceSharp.Behaviors.IFrequencyBehavior" />
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the (positive, positive) element.
        /// </summary>
        protected MatrixElement<Complex> CPosPosPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, positive) element.
        /// </summary>
        protected MatrixElement<Complex> CNegPosPtr { get; private set; }

        /// <summary>
        /// Gets the (positive, negative) element.
        /// </summary>
        protected MatrixElement<Complex> CPosNegPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, negative) element.
        /// </summary>
        protected MatrixElement<Complex> CNegNegPtr { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="method">The method.</param>
        public FrequencyBehavior(string name, Controller method) : base(name, method)
        {
        }

        /// <summary>
        /// Initialize small-signal parameters.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        public void InitializeParameters(FrequencySimulation simulation)
        {
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public void GetEquationPointers(Solver<Complex> solver)
        {
            solver.ThrowIfNull(nameof(solver));

            CPosPosPtr = solver.GetMatrixElement(PosNode, PosNode);
            CPosNegPtr = solver.GetMatrixElement(PosNode, NegNode);
            CNegPosPtr = solver.GetMatrixElement(NegNode, PosNode);
            CNegNegPtr = solver.GetMatrixElement(NegNode, NegNode);
        }

        /// <summary>
        /// Load the Y-matrix and right-hand side vector for frequency domain analysis.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        public void Load(FrequencySimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            // Get the current state
            var currentState = CurrentState;
            var gNow = currentState ? ModelParameters.OnConductance : ModelParameters.OffConductance;

            // Load the Y-matrix
            PosPosPtr.Value += gNow;
            PosNegPtr.Value -= gNow;
            NegPosPtr.Value -= gNow;
            NegNegPtr.Value += gNow;
        }
    }
}
