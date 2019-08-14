using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.VoltageControlledCurrentSourceBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="VoltageControlledCurrentSource" />.
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Necessary parameters and behaviors
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// The positive node.
        /// </summary>
        protected int PosNode { get; private set; }

        /// <summary>
        /// The negative index.
        /// </summary>
        protected int NegNode { get; private set; }

        /// <summary>
        /// The controlling positive node.
        /// </summary>
        protected int ContPosNode { get; private set; }

        /// <summary>
        /// The controlling negative node.
        /// </summary>
        protected int ContNegNode { get; private set; }

        /// <summary>
        /// The (pos, ctrlpos) element.
        /// </summary>
        protected MatrixElement<double> PosControlPosPtr { get; private set; }

        /// <summary>
        /// The (neg, ctrlneg) element.
        /// </summary>
        protected MatrixElement<double> PosControlNegPtr { get; private set; }

        /// <summary>
        /// The (neg, ctrlpos) element.
        /// </summary>
        protected MatrixElement<double> NegControlPosPtr { get; private set; }

        /// <summary>
        /// The (neg, ctrlneg) element.
        /// </summary>
        protected MatrixElement<double> NegControlNegPtr { get; private set; }

        /// <summary>
        /// Get the voltage.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double GetVoltage() => _state.ThrowIfNotBound(this).Solution[PosNode] - _state.Solution[NegNode];

        /// <summary>
        /// Get the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Current")]
        public double GetCurrent() => (_state.ThrowIfNotBound(this).Solution[ContPosNode] - _state.Solution[ContNegNode]) * BaseParameters.Coefficient;

        /// <summary>
        /// Get the power dissipation.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Power")]
        public double GetPower()
        {
            _state.ThrowIfNotBound(this);
            var v = _state.Solution[PosNode] - _state.Solution[NegNode];
            var i = (_state.Solution[ContPosNode] - _state.Solution[ContNegNode]) * BaseParameters.Coefficient;
            return -v * i;
        }

        // Cache
        private BaseSimulationState _state;

        /// <summary>
        /// Creates a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public BiasingBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            // Get parameters
            BaseParameters = context.GetParameterSet<BaseParameters>();

            if (context is ComponentBindingContext cc)
            {
                PosNode = cc.Pins[0];
                NegNode = cc.Pins[1];
                ContPosNode = cc.Pins[2];
                ContNegNode = cc.Pins[3];
            }

            _state = ((BaseSimulation)simulation).RealState;
            var solver = _state.Solver;
            PosControlPosPtr = solver.GetMatrixElement(PosNode, ContPosNode);
            PosControlNegPtr = solver.GetMatrixElement(PosNode, ContNegNode);
            NegControlPosPtr = solver.GetMatrixElement(NegNode, ContPosNode);
            NegControlNegPtr = solver.GetMatrixElement(NegNode, ContNegNode);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            _state = null;
            PosControlPosPtr = null;
            PosControlNegPtr = null;
            NegControlPosPtr = null;
            NegControlNegPtr = null;
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            var value = BaseParameters.Coefficient.Value;
            PosControlPosPtr.Value += value;
            PosControlNegPtr.Value -= value;
            NegControlPosPtr.Value -= value;
            NegControlNegPtr.Value += value;
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        bool IBiasingBehavior.IsConvergent() => true;
    }
}
