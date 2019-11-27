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
        protected IVariableMap Variables { get; private set; }

        /// <summary>
        /// Gets the solver of the system of equations.
        /// </summary>
        protected ISparseSolver<double> Solver { get; private set; }

        /// <summary>
        /// Gets the diagonal element.
        /// </summary>
        protected Element<double> Diagonal { get; private set; }

        /// <summary>
        /// Gets the right-hand side element.
        /// </summary>
        protected Element<double> Rhs { get; private set; }

        /// <summary>
        /// Gets the variable index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        protected int Index { get; private set; }

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
        public virtual void Initialize(IBiasingSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));
            var state = simulation.State;
            Variables = state.Map;
            Solver = state.Solver;

            // Get the node
            if (!simulation.Variables.TryGetNode(Name, out var node) || !Variables.Contains(node))
            {
                SpiceSharpWarning.Warning(this, "Could not set convergence aid: variable {0} not found.".FormatString(Name));
                Index = 0;
                Diagonal = null;
                Rhs = null;
                return;
            }

            // Get the necessary elements
            Index = state.Map[node];
            Diagonal = Solver.GetElement(Index, Index);
            Rhs = !Value.Equals(0.0) ? Solver.GetElement(Index) : Solver.FindElement(Index);

            // Update the current solution to reflect our convergence aid value
            state.Solution[Index] = Value;
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
                if (v.Key.UnknownType == VariableType.Current)
                    hasOtherTypes = true;
                else
                {
                    var elt = Solver.FindElement(Index, v.Value);
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
    }
}
