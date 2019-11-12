using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System.Threading.Tasks;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// Time behavior for a <see cref="Subcircuit"/>.
    /// </summary>
    /// <seealso cref="SubcircuitBehavior{T}" />
    /// <seealso cref="ITimeBehavior" />
    public class TimeBehavior : SubcircuitBehavior<ITimeBehavior>, ITimeBehavior
    {
        private BiasingBehavior _biasing;
        private TimeParameters _tp;
        private IBiasingSimulationState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TimeBehavior(string name, SubcircuitBindingContext context) : base(name, context)
        {
            context.ThrowIfNull(nameof(context));
            _biasing = context.Behaviors.GetValue<BiasingBehavior>();
            _biasing.AfterLoad += LoadTransientSubcircuitBehaviors;
            context.Behaviors.Parameters.TryGetValue(out _tp);
            _state = context.States.GetValue<IBiasingSimulationState>();
        }

        /// <summary>
        /// Initialize the state values from the current DC solution.
        /// </summary>
        /// <remarks>
        /// In this method, the initial value is calculated based on the operating point solution,
        /// and the result is stored in each respective <see cref="SpiceSharp.IntegrationMethods.StateDerivative" /> or <see cref="SpiceSharp.IntegrationMethods.StateHistory" />.
        /// </remarks>
        public void InitializeStates()
        {
            if (_tp != null && _tp.ParallelInitialize && Behaviors.Length > 1)
            {
                var tasks = new Task[Behaviors.Length];
                for (var t = 0; t < tasks.Length; t++)
                {
                    var task = t;
                    tasks[t] = Task.Run(() =>
                    {
                        foreach (var behavior in Behaviors[task])
                            behavior.InitializeStates();
                    });
                }
                Task.WaitAll(tasks);
            }
            else 
            {
                foreach (var bs in Behaviors)
                {
                    foreach (var behavior in bs)
                        behavior.InitializeStates();
                }
            }
        }

        /// <summary>
        /// Loads the transient subcircuit behaviors.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="BiasingEventArgs"/> instance containing the event data.</param>
        public void LoadTransientSubcircuitBehaviors(object sender, BiasingEventArgs args)
        {
            if (_state.UseDc)
                return;
            foreach (var behavior in Behaviors[args.Task])
                behavior.Load();
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        public void Load()
        {
            // We already hooked up loading elsewhere
        }
    }
}
