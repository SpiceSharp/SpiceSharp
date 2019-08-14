using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for aiding convergence.
    /// </summary>
    /// <remarks>
    /// The convergence aid will try to bring the solution of a variable as close
    /// as possible to the specified value. If this value is close to the final
    /// solution, then convergence can be achieved much faster.
    /// </remarks>
    public class ConvergenceAid
    {
        /// <summary>
        /// The amount with which a value is forced to the convergence aid value.
        /// </summary>
        private const double Force = 1.0e10;

        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the value for the convergence aid.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Gets the unknown variables.
        /// </summary>
        protected VariableSet Variables { get; private set; }

        /// <summary>
        /// Gets the solver of the system of equations.
        /// </summary>
        protected Solver<double> Solver { get; private set; }

        /// <summary>
        /// Gets the diagonal element.
        /// </summary>
        protected MatrixElement<double> Diagonal { get; private set; }

        /// <summary>
        /// Gets the right-hand side element.
        /// </summary>
        protected VectorElement<double> Rhs { get; private set; }

        /// <summary>
        /// Gets the node for which the aid is meant.
        /// </summary>
        protected Variable Node { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvergenceAid"/> class.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The value.</param>
        public ConvergenceAid(string name, double value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Sets up the convergence aid for a specific simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public virtual void Initialize(BaseSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            // Get the unknown variables
            Variables = simulation.Variables;

            // Get the real solver
            var state = simulation.RealState;
            Solver = state.Solver;

            // Get the node
            if (!simulation.Variables.TryGetNode(Name, out var node))
            {
                CircuitWarning.Warning(this, "Could not set convergence aid: variable {0} not found.".FormatString(Name));
                Node = null;
                return;
            }
            Node = node;

            // Get the necessary elements
            Diagonal = Solver.GetMatrixElement(node.Index, node.Index);
            Rhs = !Value.Equals(0.0) ? Solver.GetRhsElement(node.Index) : Solver.FindRhsElement(node.Index);

            // Update the current solution to reflect our convergence aid value
            state.Solution[node.Index] = Value;
        }

        /// <summary>
        /// Aids the convergence.
        /// </summary>
        public virtual void Aid()
        {
            Solver.ThrowIfNull("solver");

            // Don't execute if the convergence aid wasn't initialized properly
            if (Diagonal == null)
                return;

            // Clear the row
            var hasOtherTypes = false;
            foreach (var v in Variables)
            {
                // If the variable is a current, then we can't just set it to 0... 
                if (v.UnknownType == VariableType.Current)
                    hasOtherTypes = true;
                else
                {
                    var elt = Solver.FindMatrixElement(Node.Index, v.Index);
                    if (elt != null)
                        elt.Value = 0.0;
                }
            }

            // If there are current contributions, then we can't just hard-set the value
            if (hasOtherTypes)
            {
                Diagonal.Value = Force;
                if (Rhs != null)
                    Rhs.Value = Force * Value;
            }
            else
            {
                Diagonal.Value = 1.0;
                if (Rhs != null)
                    Rhs.Value = Value;
            }
        }

        /// <summary>
        /// Destroys the convergence aid.
        /// </summary>
        public virtual void Unsetup()
        {
            // Clear all variables
            Variables = null;
            Solver = null;
            Node = null;
            Diagonal = null;
            Rhs = null;
        }
    }
}
