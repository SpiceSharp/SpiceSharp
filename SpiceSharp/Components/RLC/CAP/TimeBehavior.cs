using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="Capacitor" />.
    /// </summary>
    /// <seealso cref="TemperatureBehavior" />
    /// <seealso cref="ITimeBehavior" />
    public class TimeBehavior : TemperatureBehavior, ITimeBehavior
    {
        private readonly IBiasingSimulationState _biasing;
        private readonly ElementSet<double> _elements;
        private readonly IDerivative _qcap;
        private readonly int _posNode, _negNode;
        private readonly ITimeSimulationState _time;

        /// <summary>
        /// Gets the current through the capacitor.
        /// </summary>
        [ParameterName("i"), ParameterInfo("Device current")]
        public double Current => _qcap.Derivative;

        /// <summary>
        /// Gets the instantaneous power dissipated by the capacitor.
        /// </summary>
        /// <returns></returns>
        [ParameterName("p"), ParameterInfo("Instantaneous device power")]
        public double Power => _qcap.Derivative * (_biasing.Solution[_posNode] - _biasing.Solution[_negNode]);

        /// <summary>
        /// Gets the voltage across the capacitor.
        /// </summary>
        /// <returns></returns>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double Voltage => _biasing.Solution[_posNode] - _biasing.Solution[_negNode];
        
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
            _posNode = _biasing.Map[_biasing.MapNode(context.Nodes[0])];
            _negNode = _biasing.Map[_biasing.MapNode(context.Nodes[1])];
            _elements = new ElementSet<double>(_biasing.Solver, new[] {
                new MatrixLocation(_posNode, _posNode),
                new MatrixLocation(_posNode, _negNode),
                new MatrixLocation(_negNode, _posNode),
                new MatrixLocation(_negNode, _negNode)
            }, new[] { _posNode, _negNode });

            var method = context.GetState<IIntegrationMethod>();
            _qcap = method.CreateDerivative();
        }

        /// <summary>
        /// Calculate the state for DC
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
            // Calculate the state for DC
            var sol = _biasing.Solution;
            if (_time.UseIc)
                _qcap.Value = Capacitance * Parameters.InitialCondition;
            else
                _qcap.Value = Capacitance * (sol[_posNode] - sol[_negNode]);
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            // Don't matter for DC analysis
            if (_time.UseDc)
                return;
            var vcap = _biasing.Solution[_posNode] - _biasing.Solution[_negNode];

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
