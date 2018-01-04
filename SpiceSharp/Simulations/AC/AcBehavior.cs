using SpiceSharp.Sparse;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// AC behavior for circuit objects
    /// </summary>
    public abstract class AcBehavior : Behavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcBehavior(Identifier name = null) : base(name) { }

        /// <summary>
        /// Initialize parameters for AC analysis
        /// </summary>
        public virtual void InitializeParameters(FrequencySimulation sim)
        {
            // Nothing to initialize by default
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public virtual void GetMatrixPointers(Matrix matrix)
        {
            // No matrix pointers by default
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector for AC analysis
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public abstract void Load(Circuit ckt);
    }
}
