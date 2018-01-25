using System;
using System.Numerics;
using SpiceSharp.Sparse;
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
        public AcBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create an export method for AC analysis
        /// </summary>
        /// <param name="property">Property name</param>
        /// <returns></returns>
        public virtual Func<State, Complex> CreateAcExport(string property)
        {
            return CreateExport<State, Complex>(property);
        }

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
        /// <param name="sim">Frequency-based simulation</param>
        public abstract void Load(FrequencySimulation sim);
    }
}
