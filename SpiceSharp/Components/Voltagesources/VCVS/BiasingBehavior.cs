using SpiceSharp.Entities;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.VoltageControlledVoltageSourceBehaviors
{
    /// <summary>
    /// General behavior for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    public class BiasingBehavior : Behavior, IBiasingBehavior
    {
        /// <summary>
        /// Gets the base parameters.
        /// </summary>
        protected BaseParameters BaseParameters { get; private set; }

        /// <summary>
        /// Gets the current through the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("i"), ParameterName("i_r"), ParameterInfo("Output current")]
        public double GetCurrent() => BiasingState.ThrowIfNotBound(this).Solution[_branchEq];

        /// <summary>
        /// Gets the voltage applied by the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("v"), ParameterName("v_r"), ParameterInfo("Output current")]
        public double GetVoltage() => BiasingState.ThrowIfNotBound(this).Solution[_posNode] - BiasingState.Solution[_negNode];

        /// <summary>
        /// Gets the power dissipated by the source.
        /// </summary>
        /// <returns></returns>
        [ParameterName("p"), ParameterName("p_r"), ParameterInfo("Power")]
        public double GetPower() => BiasingState.ThrowIfNotBound(this).Solution[_branchEq] * (BiasingState.Solution[_posNode] - BiasingState.Solution[_negNode]);

        /// <summary>
        /// Gets the branch equation.
        /// </summary>
        public Variable Branch { get; private set; }

        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected ElementSet<double> Elements { get; private set; }

        /// <summary>
        /// Gets the biasing simulation state.
        /// </summary>
        /// <value>
        /// The biasing simulation state.
        /// </value>
        protected IBiasingSimulationState BiasingState { get; private set; }

        private int _posNode, _negNode, _contPosNode, _contNegNode, _branchEq;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, ComponentBindingContext context) : base(name)
        {
            context.ThrowIfNull(nameof(context));
            context.Nodes.ThrowIfNot("nodes", 4);

            BaseParameters = context.Behaviors.Parameters.GetValue<BaseParameters>();
            BiasingState = context.States.GetValue<IBiasingSimulationState>();
            _posNode = BiasingState.Map[context.Nodes[0]];
            _negNode = BiasingState.Map[context.Nodes[1]];
            _contPosNode = BiasingState.Map[context.Nodes[2]];
            _contNegNode = BiasingState.Map[context.Nodes[3]];
            Branch = context.Variables.Create(Name.Combine("branch"), VariableType.Current);
            _branchEq = BiasingState.Map[Branch];
            Elements = new ElementSet<double>(BiasingState.Solver,
                new MatrixLocation(_posNode, _branchEq),
                new MatrixLocation(_negNode, _branchEq),
                new MatrixLocation(_branchEq, _posNode),
                new MatrixLocation(_branchEq, _negNode),
                new MatrixLocation(_branchEq, _contPosNode),
                new MatrixLocation(_branchEq, _contNegNode));
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        void IBiasingBehavior.Load()
        {
            var val = BaseParameters.Coefficient;
            Elements.Add(1, -1, 1, -1, -val, val);
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
