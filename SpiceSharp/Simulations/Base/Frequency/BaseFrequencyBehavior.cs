using System;
using System.Collections.Generic;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A template that describes frequency-dependent behavior.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.Behavior" />
    public abstract class BaseFrequencyBehavior : Behavior
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        protected BaseFrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Creates a getter for a complex parameter.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        /// A function get return the value of the specified parameter, or <c>null</c> if no parameter was found.
        /// </returns>
        public virtual Func<Complex> CreateAcExport(Simulation simulation, string propertyName, IEqualityComparer<string> comparer = null)
        {
            return CreateGetter<Complex>(simulation, propertyName, comparer);
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        /// <exception cref="ArgumentNullException">simulation</exception>
        public virtual void InitializeParameters(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            // Nothing to initialize by default
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public abstract void GetEquationPointers(Solver<Complex> solver);

        /// <summary>
        /// Load the Y-matrix and right-hand side vector for frequency domain analysis.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        public abstract void Load(FrequencySimulation simulation);
    }
}
