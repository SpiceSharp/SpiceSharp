using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="Inductor"/>
    /// </summary>
    public class BiasingBehavior : TemperatureBehavior, IBiasingBehavior, IBranchedBehavior<double>
    {
        private readonly int _posNode, _negNode, _branchEq;
        private readonly ElementSet<double> _elements;

        /// <summary>
        /// Gets the branch equation index.
        /// </summary>
        public IVariable<double> Branch { get; }

        /// <summary>
        /// Gets the current.
        /// </summary>
        [ParameterName("i"), ParameterName("c"), ParameterInfo("Current")]
        public double Current => BiasingState.Solution[_branchEq];

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double Voltage => BiasingState.Solution[_posNode] - BiasingState.Solution[_negNode];

        /// <summary>
        /// Gets the power dissipated by the inductor.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Power")]
        public double Power
        {
            get
            {
                var v = BiasingState.Solution[_posNode] - BiasingState.Solution[_negNode];
                return v * BiasingState.Solution[_branchEq];
            }
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        protected IBiasingSimulationState BiasingState { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public BiasingBehavior(string name, IComponentBindingContext context) : base(name, context) 
        {
            context.Nodes.CheckNodes(2);

            BiasingState = context.GetState<IBiasingSimulationState>();
            _posNode = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[0])];
            _negNode = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[1])];
            Branch = BiasingState.CreatePrivateVariable(Name.Combine("branch"), Units.Ampere);
            _branchEq = BiasingState.Map[Branch];

            _elements = new ElementSet<double>(BiasingState.Solver,
                new MatrixLocation(_posNode, _branchEq),
                new MatrixLocation(_negNode, _branchEq),
                new MatrixLocation(_branchEq, _negNode),
                new MatrixLocation(_branchEq, _posNode));
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        protected virtual void Load()
        {
            _elements.Add(1, -1, -1, 1);
        }

        /// <summary>
        /// Loads the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load() => Load();
    }
}
