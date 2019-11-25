using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="Inductor"/>
    /// </summary>
    public class BiasingBehavior : TemperatureBehavior, IBiasingBehavior, IBranchedBehavior
    {
        /// <summary>
        /// Gets the branch equation index.
        /// </summary>
        public Variable Branch { get; private set; }

        /// <summary>
        /// Gets the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Current")]
        public double GetCurrent() => BiasingState.ThrowIfNotBound(this).Solution[_branchEq];

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double GetVoltage() => BiasingState.ThrowIfNotBound(this).Solution[_posNode] - BiasingState.Solution[_negNode];

        /// <summary>
        /// Gets the power dissipated by the inductor.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Power")]
        public double GetPower()
        {
            BiasingState.ThrowIfNotBound(this);
            var v = BiasingState.Solution[_posNode] - BiasingState.Solution[_negNode];
            return v * BiasingState.Solution[_branchEq];
        }

        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected ElementSet<double> Elements { get; private set; }

        /// <summary>
        /// Gets the state.
        /// </summary>
        protected IBiasingSimulationState BiasingState { get; private set; }

        private int _posNode, _negNode, _branchEq;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, ComponentBindingContext context) : base(name, context) 
        {
            context.Nodes.ThrowIfNot("nodes", 2);

            BiasingState = context.GetState<IBiasingSimulationState>();
            _posNode = BiasingState.Map[context.Nodes[0]];
            _negNode = BiasingState.Map[context.Nodes[1]];
            Branch = context.Variables.Create(Name.Combine("branch"), VariableType.Current);
            _branchEq = BiasingState.Map[Branch];

            Elements = new ElementSet<double>(BiasingState.Solver,
                new MatrixLocation(_posNode, _branchEq),
                new MatrixLocation(_negNode, _branchEq),
                new MatrixLocation(_branchEq, _negNode),
                new MatrixLocation(_branchEq, _posNode));
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        void IBiasingBehavior.Load()
        {
            Elements.Add(1, -1, -1, 1);
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
