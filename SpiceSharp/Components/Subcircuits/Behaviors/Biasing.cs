using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// An <see cref="IBiasingBehavior"/> for a <see cref="SubcircuitDefinition"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="IBiasingBehavior" />
    /// <seealso cref="IConvergenceBehavior"/>
    [BehaviorFor(typeof(Subcircuit))]
    public partial class Biasing : SubcircuitBehavior<IBiasingBehavior>,
        IBiasingBehavior,
        IBiasingUpdateBehavior,
        IConvergenceBehavior
    {
        private BehaviorList<IConvergenceBehavior> _convergenceBehaviors;
        private readonly LocalSimulationState _state;
        private readonly BiasingParameters _biasingParameters;

        /// <summary>
        /// Gets the update behaviors.
        /// </summary>
        protected BehaviorList<IBiasingUpdateBehavior> UpdateBehaviors { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing" /> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Biasing(SubcircuitBindingContext context)
            : base(context)
        {
            var parameters = context.GetParameterSet<Parameters>();
            UpdateBehaviors = context.GetBehaviors<IBiasingUpdateBehavior>();
            var parent = context.GetState<IBiasingSimulationState>();
            if (parameters.LocalSolver)
            {
                _state = new LocalSimulationState(Name, parent, new SparseRealSolver());
                context.AddLocalState<IBiasingSimulationState>(_state);
            }
            else
                context.AddLocalState<IBiasingSimulationState>(new FlatSimulationState(Name, parent, context.Bridges));
            _biasingParameters = context.GetSimulationParameterSet<BiasingParameters>();
        }

        /// <inheritdoc/>
        public override void FetchBehaviors(SubcircuitBindingContext context)
        {
            base.FetchBehaviors(context);
            _convergenceBehaviors = context.GetBehaviors<IConvergenceBehavior>();
            _state?.Initialize(context.Bridges);
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            if (_state != null)
            {
                _state.Update();
                do
                {
                    _state.Solver.Reset();
                    LoadBehaviors();
                }
                while (!_state.Apply());
            }
            else
                LoadBehaviors();
        }

        /// <summary>
        /// Loads the behaviors.
        /// </summary>
        protected virtual void LoadBehaviors()
        {
            foreach (var behavior in Behaviors)
                behavior.Load();
        }

        /// <inheritdoc/>
        bool IConvergenceBehavior.IsConvergent()
        {
            _state?.Update();

            // Check convergence for each node
            if (_state is not null)
            {
                foreach (var v in _state.Map)
                {
                    var node = v.Key;
                    double n = _state.Solution[v.Value];
                    double o = _state.OldSolution[v.Value];

                    if (double.IsNaN(n))
                        throw new SpiceSharpException(Properties.Resources.Simulation_VariableNotANumber.FormatString(v));

                    if (node.Unit == Units.Volt)
                    {
                        double tol = _biasingParameters.RelativeTolerance * Math.Max(Math.Abs(n), Math.Abs(o)) + _biasingParameters.VoltageTolerance;
                        if (Math.Abs(n - o) > tol)
                            return false;
                    }
                    else
                    {
                        double tol = _biasingParameters.RelativeTolerance * Math.Max(Math.Abs(n), Math.Abs(o)) + _biasingParameters.AbsoluteTolerance;
                        if (Math.Abs(n - o) > tol)
                            return false;
                    }
                }
            }

            // Run the convergence behaviors
            bool result = true;
            foreach (var behavior in _convergenceBehaviors)
                result &= behavior.IsConvergent();
            return result;
        }

        /// <inheritdoc />
        public void Update()
        {
            _state?.Update();
            foreach (var behavior in UpdateBehaviors)
                behavior.Update();
        }
    }
}
