using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System.Linq;
using System;
using System.Numerics;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// An <see cref="INoiseBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="INoiseBehavior" />
    [BehaviorFor(typeof(Subcircuit))]
    public class Noise : SubcircuitBehavior<INoiseBehavior>,
        INoiseBehavior
    {
        private readonly LocalSolverState<Complex, IComplexSimulationState> _state;

        /// <inheritdoc/>
        public double OutputNoiseDensity => Behaviors.Sum(nb => nb.OutputNoiseDensity);

        /// <inheritdoc/>
        public double TotalOutputNoise => Behaviors.Sum(nb => nb.TotalOutputNoise);

        /// <inheritdoc/>
        public double TotalInputNoise => Behaviors.Sum(nb => nb.TotalInputNoise);

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Noise(SubcircuitBindingContext context)
            : base(context)
        {
            var parameters = context.GetParameterSet<Parameters>();
            if (parameters.LocalSolver)
            {
                // Use the state created by the Frequency behavior
                _state = (LocalSolverState<Complex, IComplexSimulationState>)context.GetState<IComplexSimulationState>();
            }
            else
                _state = null;
        }

        /// <inheritdoc/>
        void INoiseSource.Initialize()
        {
            foreach (var behavior in Behaviors)
                behavior.Initialize();
        }

        void INoiseBehavior.Load()
        {
            if (_state == null)
            {
                // Regular operation, no local solver
                foreach (var behavior in Behaviors)
                    behavior.Load();
                return;
            }

            // If we reach this point, we have a local solver and need
            // to reset the right-hand side vector, then load all subcircuit
            // behaviors, and finally do forward substitution.
            _state.Solver.Precondition((matrix, rhs) =>
            {
                rhs.Reset();
            });

            // Load the noise behaviors inside
            foreach (var behavior in Behaviors)
                behavior.Load();

            // Forward substitute and apply
            _state.ApplyTransposed();
        }

        /// <inheritdoc/>
        void INoiseBehavior.Compute()
        {
            _state?.UpdateTransposed();
            foreach (var behavior in Behaviors)
                behavior.Compute();
        }
    }
}
