using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
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
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior for usage with a matrix
        /// </summary>
        /// <param name="matrix">The matrix</param>
        public virtual void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            // No pointers needed by default
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public abstract void Load(BaseSimulation sim);

        /// <summary>
        /// Test convergence on device-level
        /// </summary>
        /// <param name="sim">Base simulation</param>
        /// <returns></returns>
        public virtual bool IsConvergent(BaseSimulation sim)
        {
            return true;
        }
    }
}
