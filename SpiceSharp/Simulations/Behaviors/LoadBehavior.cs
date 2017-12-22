using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

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
        public LoadBehavior(Identifier name = null) : base(name) { }

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
        /// <param name="ckt">Circuit</param>
        public abstract void Load(Circuit ckt);

        /// <summary>
        /// Test convergence
        /// </summary>
        /// <returns></returns>
        public virtual bool IsConvergent(Circuit ckt)
        {
            return true;
        }
    }
}
