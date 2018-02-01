using System;
using System.Numerics;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// AC behavior for circuit objects
    /// </summary>
    public abstract class FrequencyBehavior : Behavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        protected FrequencyBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create an export method for AC analysis
        /// </summary>
        /// <param name="propertyName">Property name</param>
        /// <returns></returns>
        public virtual Func<ComplexState, Complex> CreateACExport(string propertyName)
        {
            return CreateExport<ComplexState, Complex>(propertyName);
        }

        /// <summary>
        /// Initialize parameters for AC analysis
        /// </summary>
        public virtual void InitializeParameters(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            // Nothing to initialize by default
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public virtual void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            // No matrix pointers by default
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public abstract void Load(FrequencySimulation simulation);
    }
}
