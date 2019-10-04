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
    /// <seealso cref="SpiceSharp.Components.CapacitorBehaviors.TemperatureBehavior" />
    /// <seealso cref="SpiceSharp.Behaviors.ITimeBehavior" />
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
            return QCap.Derivative * (BiasingState.Solution[PosNode] - BiasingState.Solution[NegNode]);
        }

        /// <summary>
        /// Gets the voltage across the capacitor.
        /// </summary>
        /// <returns></returns>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double GetVoltage() => BiasingState.ThrowIfNotBound(this).Solution[PosNode] - BiasingState.Solution[NegNode];

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

        /// <summary>
        /// Gets the biasing simulation state.
        /// </summary>
        /// <value>
        /// The biasing simulation state.
        /// </value>
        protected IBiasingSimulationState BiasingState { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">Name of the behavior</param>
        public TransientBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            BiasingState = context.States.GetValue<IBiasingSimulationState>();
            Elements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(PosNode, PosNode),
                new MatrixLocation(PosNode, NegNode),
                new MatrixLocation(NegNode, PosNode),
                new MatrixLocation(NegNode, NegNode)
            }, new[] { PosNode, NegNode });

            var method = context.States.GetValue<ITimeSimulationState>().Method;
            QCap = method.CreateDerivative();
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            BiasingState = null;
            Elements?.Destroy();
            Elements = null;
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
                QCap.Current = Capacitance * (sol[PosNode] - sol[NegNode]);
        }
        
        /// <summary>
        /// Execute behavior for DC and Transient analysis
        /// </summary>
        void ITimeBehavior.Load()
        {
            var vcap = BiasingState.Solution[PosNode] - BiasingState.Solution[NegNode];

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
