using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A template that describes frequency-dependent behavior.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.Behavior" />
    public abstract class BaseFrequencyBehavior : Behavior, IFrequencyBehavior
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
        /// Initializes the parameters.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        public virtual void InitializeParameters(FrequencySimulation simulation)
        {
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
