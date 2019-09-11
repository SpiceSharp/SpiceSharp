using SpiceSharp.Algebra;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.VoltageSourceBehaviors
{
    /// <summary>
    /// General behavior for <see cref="VoltageSource"/>
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        protected CommonBehaviors.IndependentSourceParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("i"), ParameterName("i_r"), ParameterInfo("Voltage source current")]
        public double GetCurrent() => BiasingState.ThrowIfNotBound(this).Solution[BranchEq];

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Instantaneous power")]
        public double GetPower() => (BiasingState.ThrowIfNotBound(this).Solution[PosNode] - BiasingState.Solution[NegNode]) * -BiasingState.Solution[BranchEq];

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Instantaneous voltage")]
        public double Voltage { get; private set; }

        /// <summary>
        /// Gets the positive node.
        /// </summary>
        protected int PosNode { get; private set; }

        /// <summary>
        /// Gets the negative node.
        /// </summary>
        protected int NegNode { get; private set; }

        /// <summary>
        /// Gets the branch equation.
        /// </summary>
        public int BranchEq { get; private set; }

        /// <summary>
        /// Gets the (positive, branch) element.
        /// </summary>
        protected MatrixElement<double> PosBranchPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, branch) element.
        /// </summary>
        protected MatrixElement<double> NegBranchPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, positive) element.
        /// </summary>
        protected MatrixElement<double> BranchPosPtr { get; private set; }

        /// <summary>
        /// Gets the (branch, negative) element.
        /// </summary>
        protected MatrixElement<double> BranchNegPtr { get; private set; }

        /// <summary>
        /// Gets the branch RHS element.
        /// </summary>
        protected VectorElement<double> BranchPtr { get; private set; }

        /// <summary>
        /// Gets the biasing simulation state.
        /// </summary>
        /// <value>
        /// The biasing simulation state.
        /// </value>
        protected BiasingSimulationState BiasingState { get; private set; }

        private TimeSimulationState _timeState;

        /// <summary>
        /// Creates a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public BiasingBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);
            var c = (ComponentBindingContext)context;
            PosNode = c.Pins[0];
            NegNode = c.Pins[1];
            BaseParameters = context.Behaviors.Parameters.GetValue<CommonBehaviors.IndependentSourceParameters>();
            BaseParameters.Waveform?.Bind(context);

            if (!BaseParameters.DcValue.Given)
            {
                // No DC value: either have a transient value or none
                if (BaseParameters.Waveform != null)
                {
                    CircuitWarning.Warning(this, "{0}: No DC value, transient time 0 value used".FormatString(Name));
                    BaseParameters.DcValue.RawValue = BaseParameters.Waveform.Value;
                }
                else
                {
                    CircuitWarning.Warning(this, "{0}: No value, DC 0 assumed".FormatString(Name));
                }
            }

            // Get matrix elements
            BranchEq = context.Variables.Create(Name.Combine("branch"), VariableType.Current).Index;
            BiasingState = context.States.GetValue<BiasingSimulationState>();
            var solver = BiasingState.Solver;
            context.States.TryGetValue(out _timeState);
            PosBranchPtr = solver.GetMatrixElement(PosNode, BranchEq);
            BranchPosPtr = solver.GetMatrixElement(BranchEq, PosNode);
            NegBranchPtr = solver.GetMatrixElement(NegNode, BranchEq);
            BranchNegPtr = solver.GetMatrixElement(BranchEq, NegNode);
            BranchPtr = solver.GetRhsElement(BranchEq);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            BiasingState = null;
            PosBranchPtr = null;
            BranchPosPtr = null;
            NegBranchPtr = null;
            BranchNegPtr = null;
            BranchPtr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        void IBiasingBehavior.Load()
        {
            var state = BiasingState.ThrowIfNotBound(this);
            double value;

            PosBranchPtr.Value += 1;
            BranchPosPtr.Value += 1;
            NegBranchPtr.Value -= 1;
            BranchNegPtr.Value -= 1;

            if (_timeState != null)
            {
                // Use the waveform if possible
                if (BaseParameters.Waveform != null)
                    value = BaseParameters.Waveform.Value;
                else
                    value = BaseParameters.DcValue * state.SourceFactor;
            }
            else
            {
                value = BaseParameters.DcValue * state.SourceFactor;
            }

            Voltage = value;
            BranchPtr.Value += value;
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
