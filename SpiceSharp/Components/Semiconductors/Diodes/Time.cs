using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Diodes
{
    /// <summary>
    /// Transient behavior for a <see cref="Diode"/>
    /// </summary>
    /// <seealso cref="Dynamic"/>
    /// <seealso cref="ITimeBehavior"/>
    [BehaviorFor(typeof(Diode)), AddBehaviorIfNo(typeof(ITimeBehavior))]
    [GeneratedParameters]
    public partial class Time : Dynamic,
        ITimeBehavior
    {
        private readonly IDerivative _capCharge;
        private readonly ITimeSimulationState _time;

        /// <summary>
        /// Gets the diode capacitor current.
        /// </summary>
        /// <value>
        /// The diode capacitor current.
        /// </value>
        [ParameterName("capcur"), ParameterInfo("Diode capacitor current")]
        public double CapCurrent => _capCharge.Derivative;

        /// <summary>
        /// Initializes a new instance of the <see cref="Time"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Time(IComponentBindingContext context)
            : base(context)
        {
            _time = context.GetState<ITimeSimulationState>();
            var method = context.GetState<IIntegrationMethod>();
            _capCharge = method.CreateDerivative();
        }

        /// <inheritdoc/>
        void ITimeBehavior.InitializeStates()
        {
            double vd = _time.UseIc && Parameters.InitCond.Given ?
                Parameters.InitCond.Value / Parameters.SeriesMultiplier :
                (Variables.PosPrime.Value - Variables.Negative.Value) / Parameters.SeriesMultiplier;
            CalculateCapacitance(vd);
            _capCharge.Value = LocalCapCharge;
        }

        /// <inheritdoc/>
        protected override void Load()
        {
            base.Load();
            if (_time.UseDc)
                return;

            // Calculate the capacitance
            double n = Parameters.SeriesMultiplier;
            double m = Parameters.ParallelMultiplier;
            double vd = (Variables.PosPrime.Value - Variables.Negative.Value) / n;
            CalculateCapacitance(vd);

            // Integrate
            _capCharge.Value = LocalCapCharge;
            _capCharge.Derive();
            var info = _capCharge.GetContributions(LocalCapacitance, vd);
            double geq = info.Jacobian * m / n;
            double ceq = info.Rhs * m;

            // Store the current
            LocalCurrent += _capCharge.Derivative;

            Elements.Add(
                // Y-matrix
                0, geq, geq, -geq, -geq, 0, 0,
                // RHS vector
                ceq, -ceq);
        }
    }
}
