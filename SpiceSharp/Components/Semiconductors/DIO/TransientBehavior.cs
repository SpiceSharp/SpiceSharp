using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="Diode"/>
    /// </summary>
    public class TransientBehavior : DynamicParameterBehavior, ITimeBehavior
    {
        /// <summary>
        /// Gets the capacitor current.
        /// </summary>
        [ParameterName("capcur"), ParameterInfo("Diode capacitor current")]
        public double CapCurrent => _capCharge.Derivative;

        /// <summary>
        /// The charge on the junction capacitance
        /// </summary>
        private StateDerivative _capCharge;
        private int _negNode, _posPrimeNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TransientBehavior(string name, ComponentBindingContext context) : base(name, context)
        {
            _negNode = BiasingState.Map[context.Nodes[1]];
            _posPrimeNode = BiasingState.Map[PosPrime];

            var method = context.GetState<ITimeSimulationState>().Method;
            _capCharge = method.CreateDerivative();
        }

        /// <summary>
        /// Calculate the state values
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
            double vd = (BiasingState.Solution[_posPrimeNode] - BiasingState.Solution[_negNode]) / BaseParameters.SeriesMultiplier;
            CalculateCapacitance(vd);
            _capCharge.Current = LocalCapCharge;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        void ITimeBehavior.Load()
        {
            // Calculate the capacitance
            var state = BiasingState;
            var n = BaseParameters.SeriesMultiplier;
            double vd = (state.Solution[_posPrimeNode] - state.Solution[_negNode]) / n;
            CalculateCapacitance(vd);

            // Integrate
            _capCharge.Current = LocalCapCharge;
            _capCharge.Integrate();
            var geq = _capCharge.Jacobian(LocalCapacitance);
            var ceq = _capCharge.RhsCurrent(geq, vd);

            // Store the current
            LocalCurrent += _capCharge.Derivative;

            var m = BaseParameters.ParallelMultiplier;
            geq *= m / n;
            ceq *= m;
            Elements.Add(
                // Y-matrix
                0, geq, geq, -geq, -geq, 0, 0,
                // RHS vector
                ceq, -ceq);
        }
    }
}
