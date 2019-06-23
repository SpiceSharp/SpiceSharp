using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A template that describes the loading behavior.
    /// </summary>
    public abstract class BaseLoadBehavior : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseLoadBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected BaseLoadBehavior(string name) : base(name) { }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading. Additional
        /// equations can also be allocated here.
        /// </summary>
        /// <param name="variables">The variable set.</param>
        /// <param name="solver">The solver.</param>
        public virtual void GetEquationPointers(VariableSet variables, Solver<double> solver)
        {
            // No equation pointers needed by default
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        /// <param name="simulation">The base simulation.</param>
        public abstract void Load(BaseSimulation simulation);

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <param name="simulation">The base simulation.</param>
        /// <returns>
        ///   <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsConvergent(BaseSimulation simulation) => true;
    }
}
