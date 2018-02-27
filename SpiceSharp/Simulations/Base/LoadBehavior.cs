using SpiceSharp.Circuits;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// General behavior for a circuit object
    /// </summary>
    public abstract class LoadBehavior : Behavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        protected LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior for usage with a solver
        /// </summary>
        /// <param name="solver">Solver</param>
        public abstract void GetEquationPointers(Nodes nodes, Solver<double> solver);

        /// <summary>
        /// Load the Y-matrix and Rhs-vector
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public abstract void Load(BaseSimulation simulation);

        /// <summary>
        /// Test convergence on device-level
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        /// <returns></returns>
        public virtual bool IsConvergent(BaseSimulation simulation)
        {
            return true;
        }
    }
}
