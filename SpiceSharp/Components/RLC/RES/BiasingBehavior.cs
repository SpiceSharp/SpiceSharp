using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// General behavior for <see cref="Resistor"/>
    /// </summary>
    public class BiasingBehavior : TemperatureBehavior, IBiasingBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Gets the voltage across the resistor.
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
        /// Gets the current through the resistor.
        /// </summary>
        /// <param name="state">The simulation state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("i"), ParameterInfo("Current")]
        public double GetCurrent(BaseSimulationState state)
        {
            state.ThrowIfNull(nameof(state));
            return (state.Solution[PosNode] - state.Solution[NegNode]) * Conductance;
        }

        /// <summary>
        /// Gets the power dissipated by the resistor.
        /// </summary>
        /// <param name="state">The simulation state.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">state</exception>
        [ParameterName("p"), ParameterInfo("Power")]
        public double GetPower(BaseSimulationState state)
        {
			state.ThrowIfNull(nameof(state));
            var v = state.Solution[PosNode] - state.Solution[NegNode];
            return v * v * Conductance;
        }

        /// <summary>
        /// Nodes
        /// </summary>
        protected int PosNode { get; private set; }
        protected int NegNode { get; private set; }
        protected MatrixElement<double> PosPosPtr { get; private set; }
        protected MatrixElement<double> NegNegPtr { get; private set; }
        protected MatrixElement<double> PosNegPtr { get; private set; }
        protected MatrixElement<double> NegPosPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public BiasingBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Connect the behavior to nodes
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            pins.ThrowIfNot(nameof(pins), 2);
            PosNode = pins[0];
            NegNode = pins[1];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="variables">Nodes</param>
        /// <param name="solver">Solver</param>
        public void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            solver.ThrowIfNull(nameof(solver));

            // Get matrix elements
            PosPosPtr = solver.GetMatrixElement(PosNode, PosNode);
            NegNegPtr = solver.GetMatrixElement(NegNode, NegNode);
            PosNegPtr = solver.GetMatrixElement(PosNode, NegNode);
            NegPosPtr = solver.GetMatrixElement(NegNode, PosNode);
        }
        
        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public void Load(BaseSimulation simulation)
        {
            var conductance = Conductance;
            PosPosPtr.Value += conductance;
            NegNegPtr.Value += conductance;
            PosNegPtr.Value -= conductance;
            NegPosPtr.Value -= conductance;
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <param name="simulation">The base simulation.</param>
        /// <returns>
        /// <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvergent(BaseSimulation simulation) => true;
    }
}
