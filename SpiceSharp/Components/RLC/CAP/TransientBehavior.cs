using SpiceSharp.Entities;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="Capacitor" />.
    /// </summary>
    /// <seealso cref="TemperatureBehavior" />
    /// <seealso cref="ITimeBehavior" />
    public class TransientBehavior : TemperatureBehavior, ITimeBehavior
    {
        /// <summary>
        /// Gets the current through the capacitor.
        /// </summary>
        [ParameterName("i"), ParameterInfo("Device current")]
        public double Current => QCap.Derivative;

        /// <summary>
        /// Gets the instantaneous power dissipated by the capacitor.
        /// </summary>
        /// <returns></returns>
        [ParameterName("p"), ParameterInfo("Instantaneous device power")]
        public double GetPower()
        {
            BiasingState.ThrowIfNotBound(this);
            return QCap.Derivative * (BiasingState.Solution[_posNode] - BiasingState.Solution[_negNode]);
        }

        /// <summary>
        /// Gets the voltage across the capacitor.
        /// </summary>
        /// <returns></returns>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double GetVoltage() => BiasingState.ThrowIfNotBound(this).Solution[_posNode] - BiasingState.Solution[_negNode];

        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected ElementSet<double> Elements { get; private set; }

        /// <summary>
        /// Gets the state tracking the charge.
        /// </summary>
        protected StateDerivative QCap { get; private set; }

        private int _posNode, _negNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TransientBehavior(string name, ComponentBindingContext context) : base(name, context)
        {
            context.Nodes.ThrowIfNot("nodes", 2);

            _posNode = BiasingState.Map[context.Nodes[0]];
            _negNode = BiasingState.Map[context.Nodes[1]];
            Elements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(_posNode, _posNode),
                new MatrixLocation(_posNode, _negNode),
                new MatrixLocation(_negNode, _posNode),
                new MatrixLocation(_negNode, _negNode)
            }, new[] { _posNode, _negNode });

            var method = context.GetState<ITimeSimulationState>().Method;
            QCap = method.CreateDerivative();
        }

        /// <summary>
        /// Calculate the state for DC
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
            // Calculate the state for DC
            var sol = BiasingState.Solution;
            if (BaseParameters.InitialCondition.Given)
                QCap.Current = BaseParameters.InitialCondition;
            else
                QCap.Current = Capacitance * (sol[_posNode] - sol[_negNode]);
        }
        
        /// <summary>
        /// Execute behavior for DC and Transient analysis
        /// </summary>
        void ITimeBehavior.Load()
        {
            var vcap = BiasingState.Solution[_posNode] - BiasingState.Solution[_negNode];

            // Integrate
            QCap.Current = Capacitance * vcap;
            QCap.Integrate();
            var geq = QCap.Jacobian(Capacitance);
            var ceq = QCap.RhsCurrent();

            // Load matrix and rhs vector
            Elements.Add(geq, -geq, -geq, geq, -ceq, ceq);
        }
    }
}
