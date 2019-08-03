using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Event arguments for evaluating a solution by an integration method.
    /// </summary>
    /// <seealso cref="TruncateTimestepEventArgs" />
    public class TruncateEvaluateEventArgs : TruncateTimestepEventArgs
    {
        /// <summary>
        /// Gets or sets the order to continue integration. The order is capped between 1 and the maximum integration order.
        /// </summary>
        public int Order
        {
            get => _order;
            set
            {
                if (value < 1)
                    _order = 1;
                else if (value > MaxOrder)
                    _order = MaxOrder;
                else
                    _order = value;
            }
        }
        private int _order = 1;

        /// <summary>
        /// Gets or sets whether or not the solution should be accepted. If flagged false, other events cannot reset this value.
        /// </summary>
        public bool Accepted { get => _accepted; set => _accepted &= value; }
        private bool _accepted = true;

        /// <summary>
        /// Gets the maximum integration order.
        /// </summary>
        public int MaxOrder { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TruncateEvaluateEventArgs"/> class.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        /// <param name="maxOrder">The maximum integration order.</param>
        public TruncateEvaluateEventArgs(TimeSimulation simulation, int maxOrder)
            : this(simulation, maxOrder, double.PositiveInfinity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TruncateEvaluateEventArgs"/> class.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        /// <param name="maxOrder">The maximum integration order.</param>
        /// <param name="delta">The initial timestep.</param>
        public TruncateEvaluateEventArgs(TimeSimulation simulation, int maxOrder, double delta)
            : base(simulation, delta)
        {
            MaxOrder = maxOrder;
        }
    }
}
