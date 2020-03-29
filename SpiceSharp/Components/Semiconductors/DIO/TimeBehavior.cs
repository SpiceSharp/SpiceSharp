using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="Diode"/>
    /// </summary>
    public class TimeBehavior : DynamicParameterBehavior, ITimeBehavior
    {
        private readonly IDerivative _capCharge;
        private readonly int _negNode, _posPrimeNode;
        private readonly ITimeSimulationState _time;

        /// <summary>
        /// Gets the capacitor current.
        /// </summary>
        [ParameterName("capcur"), ParameterInfo("Diode capacitor current")]
        public double CapCurrent => _capCharge.Derivative;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TimeBehavior(string name, IComponentBindingContext context) : base(name, context)
        {
            _time = context.GetState<ITimeSimulationState>();
            var method = context.GetState<IIntegrationMethod>();
            _capCharge = method.CreateDerivative();
        }

        /// <summary>
        /// Calculate the state values
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
            double vd = (Variables.PosPrime.Value - Variables.Negative.Value) / Parameters.SeriesMultiplier;
            CalculateCapacitance(vd);
            _capCharge.Value = LocalCapCharge;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        protected override void Load()
        {
            base.Load();
            if (_time.UseDc)
                return;

            // Calculate the capacitance
            var n = Parameters.SeriesMultiplier;
            double vd = (Variables.PosPrime.Value - Variables.Negative.Value) / n;
            CalculateCapacitance(vd);

            // Integrate
            _capCharge.Value = LocalCapCharge;
            _capCharge.Integrate();
            var info = _capCharge.GetContributions(LocalCapacitance, vd);
            var geq = info.Jacobian;
            var ceq = info.Rhs;

            // Store the current
            LocalCurrent += _capCharge.Derivative;

            var m = Parameters.ParallelMultiplier;
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
