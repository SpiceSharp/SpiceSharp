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
        protected IDerivative QCap { get; private set; }

        private int _posNode, _negNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TimeBehavior(string name, ComponentBindingContext context) : base(name, context)
        {
            context.Nodes.CheckNodes(2);

            _posNode = BiasingState.Map[context.Nodes[0]];
            _negNode = BiasingState.Map[context.Nodes[1]];
            Elements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(_posNode, _posNode),
                new MatrixLocation(_posNode, _negNode),
                new MatrixLocation(_negNode, _posNode),
                new MatrixLocation(_negNode, _negNode)
            }, new[] { _posNode, _negNode });

            var method = context.GetState<IIntegrationMethod>();
            QCap = method.CreateDerivative();
        }

        /// <summary>
        /// Calculate the state for DC
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
            // Calculate the state for DC
            var sol = BiasingState.Solution;
            if (BiasingState.UseIc)
                QCap.Value = Capacitance * BaseParameters.InitialCondition;
            else
                QCap.Value = Capacitance * (sol[_posNode] - sol[_negNode]);
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            // Don't matter for DC analysis
            if (BiasingState.UseDc)
                return;
            var vcap = BiasingState.Solution[_posNode] - BiasingState.Solution[_negNode];

            // Integrate
            QCap.Value = Capacitance * vcap;
            QCap.Integrate();
            var info = QCap.GetContributions(Capacitance);
            var geq = info.Jacobian;
            var ceq = info.Rhs;

            // Load matrix and rhs vector
            Elements.Add(geq, -geq, -geq, geq, -ceq, ceq);
        }
    }
}
