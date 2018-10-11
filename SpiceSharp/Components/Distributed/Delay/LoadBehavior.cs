using System;
using System.Collections.Generic;
using System.Text;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Loading behavior for a <see cref="Delay" />
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseLoadBehavior" />
    public class LoadBehavior : BaseLoadBehavior, IConnectedBehavior
    {
        // Necessary behaviors and parameters
        private int _input, _output;
        public int Branch { get; private set; }

        protected MatrixElement<double> OutBranchPtr { get; private set; }
        protected MatrixElement<double> BranchOutPtr { get; private set; }
        protected MatrixElement<double> BranchInPtr { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public LoadBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Connect the behavior in the circuit
        /// </summary>
        /// <param name="pins">Pin indices in order</param>
        public void Connect(params int[] pins)
        {
            _input = pins[0];
            _output = pins[1];
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading. Additional
        /// equations can also be allocated here.
        /// </summary>
        /// <param name="variables">The variable set.</param>
        /// <param name="solver">The solver.</param>
        public override void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            Branch = variables.Create(Name.Combine("branch"), VariableType.Current).Index;
            OutBranchPtr = solver.GetMatrixElement(_output, Branch);
            BranchOutPtr = solver.GetMatrixElement(Branch, _output);
            BranchInPtr = solver.GetMatrixElement(Branch, _input);
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="simulation">The base simulation.</param>
        public override void Load(BaseSimulation simulation)
        {
            var state = simulation.RealState;

            // In DC, the delay should act like a short-circuit without loading the input
            OutBranchPtr.Value += 1.0;
            BranchOutPtr.Value += 1.0;
            if (state.UseDc)
                BranchInPtr.Value -= 1.0;
        }
    }
}
