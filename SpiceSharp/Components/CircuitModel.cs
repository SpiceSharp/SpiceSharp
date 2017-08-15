namespace SpiceSharp.Components
{
    /// <summary>
    /// This model represents a component model
    /// </summary>
    public abstract class CircuitModel<T> : CircuitComponent<T>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the model</param>
        public CircuitModel(string name) : base(name)
        {
            // Make sure the models are evaluated before the actual components
            Priority = 1;
            Model = null;
        }

        /// <summary>
        /// Setup the model
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt) { }

        /// <summary>
        /// Temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt) { }

        /// <summary>
        /// Load the circuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt) { }
    }
}
