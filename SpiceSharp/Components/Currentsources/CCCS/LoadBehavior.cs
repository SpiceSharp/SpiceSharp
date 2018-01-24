using System;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Components.CCCS;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.CCCS
{
    /// <summary>
    /// Behavior for a <see cref="Components.CurrentControlledCurrentsource"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary parameters and behaviors
        /// </summary>
        BaseParameters bp;
        VSRC.LoadBehavior vsrcload;

        /// <summary>
        /// Nodes
        /// </summary>
        public int CCCScontBranch { get; protected set; }
        int CCCSposNode, CCCSnegNode;
        protected MatrixElement CCCSposContBrptr { get; private set; }
        protected MatrixElement CCCSnegContBrptr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create an export method
        /// </summary>
        /// <param name="property">Parameter name</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(string property)
        {
            switch (property)
            {
                case "i": return (State state) => state.Solution[CCCScontBranch] * bp.CCCScoeff;
                case "v": return (State state) => state.Solution[CCCSposNode] - state.Solution[CCCSnegNode];
                case "p": return (State state) =>
                    {
                        double v = state.Solution[CCCSposNode] - state.Solution[CCCSnegNode];
                        return state.Solution[CCCScontBranch] * bp.CCCScoeff * v;
                    };
                default: return null;
            }
        }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();

            // Get behaviors (0 = CCCS behaviors, 1 = VSRC behaviors)
            vsrcload = provider.GetBehavior<VSRC.LoadBehavior>(1);
        }

        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            CCCSposNode = pins[0];
            CCCSnegNode = pins[1];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="nodes">Nodes</param>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            CCCScontBranch = vsrcload.VSRCbranch;
            CCCSposContBrptr = matrix.GetElement(CCCSposNode, CCCScontBranch);
            CCCSnegContBrptr = matrix.GetElement(CCCSnegNode, CCCScontBranch);
        }
        
        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            CCCSposContBrptr.Add(bp.CCCScoeff.Value);
            CCCSnegContBrptr.Sub(bp.CCCScoeff.Value);
        }
    }
}
