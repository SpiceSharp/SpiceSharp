using SpiceSharp.Simulations;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Event arguments for evaluating a solution
    /// </summary>
    public class TruncateEvaluateEventArgs : TruncateTimestepEventArgs
    {

        /// <summary>
        /// Gets or sets the order to continue integration
        /// The order is capped between 1 and the maximum order
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
        /// Gets or sets whether or not the solution is accepted
        /// If flagged false, other events cannot reset this value
        /// </summary>
        public bool Accepted { get => _accepted; set => _accepted &= value; }
        private bool _accepted = true;

        /// <summary>
        /// The maximum order
        /// </summary>
        public int MaxOrder { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="simulation">Time simulation</param>
        /// <param name="maxOrder">Maximum integration order</param>
        public TruncateEvaluateEventArgs(TimeSimulation simulation, int maxOrder)
            : base(simulation, double.PositiveInfinity)
        {
            MaxOrder = maxOrder;
        }
    }
}
