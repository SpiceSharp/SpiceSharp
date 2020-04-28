using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components
{
    public partial class Capacitor
    {
        /// <summary>
        /// Transient behavior for a <see cref="Capacitor" />.
        /// </summary>
        /// <seealso cref="TemperatureBehavior" />
        /// <seealso cref="ITimeBehavior" />
        protected class TimeBehavior : TemperatureBehavior, 
            ITimeBehavior
        {
            private readonly IBiasingSimulationState _biasing;
            private readonly ElementSet<double> _elements;
            private readonly IDerivative _qcap;
            private readonly ITimeSimulationState _time;
            private readonly OnePort<double> _variables;

            /// <summary>
            /// Gets the instantaneous current through the capacitor.
            /// </summary>
            /// <value>
            /// The instantaneous current.
            /// </value>
            [ParameterName("i"), ParameterInfo("Device current")]
            public double Current => _qcap.Derivative;

            /// <summary>
            /// Gets the instantaneous power dissipated by the capacitor.
            /// </summary>
            /// <value>
            /// The instantaneous power dissipation.
            /// </value>
            [ParameterName("p"), ParameterInfo("Instantaneous device power")]
            public double Power => -Current * Voltage;

            /// <summary>
            /// Gets the voltage across the capacitor.
            /// </summary>
            /// <value>
            /// The instantaneous voltage.
            /// </value>
            [ParameterName("v"), ParameterInfo("Voltage")]
            public double Voltage => _variables.Positive.Value - _variables.Negative.Value;

            /// <summary>
            /// Initializes a new instance of the <see cref="TimeBehavior"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            /// <param name="context">The context.</param>
            public TimeBehavior(string name, IComponentBindingContext context) : base(name, context)
            {
                context.Nodes.CheckNodes(2);
                _biasing = context.GetState<IBiasingSimulationState>();
                _time = context.GetState<ITimeSimulationState>();
                _variables = new OnePort<double>(_biasing, context);
                _elements = new ElementSet<double>(_biasing.Solver,
                    _variables.GetMatrixLocations(_biasing.Map),
                    _variables.GetRhsIndices(_biasing.Map));
                var method = context.GetState<IIntegrationMethod>();
                _qcap = method.CreateDerivative();
            }

            void ITimeBehavior.InitializeStates()
            {
                // Calculate the state for DC
                if (_time.UseIc)
                    _qcap.Value = Capacitance * Parameters.InitialCondition;
                else
                    _qcap.Value = Capacitance * (_variables.Positive.Value - _variables.Negative.Value);
            }

            void IBiasingBehavior.Load()
            {
                // Don't matter for DC analysis
                if (_time.UseDc)
                    return;
                var vcap = _variables.Positive.Value - _variables.Negative.Value;

                // Integrate
                _qcap.Value = Capacitance * vcap;
                _qcap.Integrate();
                var info = _qcap.GetContributions(Capacitance);
                var geq = info.Jacobian;
                var ceq = info.Rhs;

                // Load matrix and rhs vector
                _elements.Add(geq, -geq, -geq, geq, -ceq, ceq);
            }
        }
    }
}