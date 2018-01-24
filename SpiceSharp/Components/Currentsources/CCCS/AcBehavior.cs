using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Components.CCCS;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Behaviors.CCCS
{
    /// <summary>
    /// AC behavior for <see cref="Components.CurrentControlledCurrentsource"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary parameters and behaviors
        /// </summary>
        BaseParameters bp;
        VSRC.LoadBehavior vsrcload;

        /// <summary>
        /// Nodes
        /// </summary>
        int CCCSposNode, CCCSnegNode, CCCScontBranch;
        protected MatrixElement CCCSposContBrptr { get; private set; }
        protected MatrixElement CCCSnegContBrptr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcBehavior(Identifier name) : base(name) { }
        
        /// <summary>
        /// Create an export method
        /// </summary>
        /// <param name="property">Parameters</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(string property)
        {
            switch (property)
            {
                case "vr": return (State state) => state.Solution[CCCSposNode] - state.Solution[CCCSnegNode];
                case "vi": return (State state) => state.iSolution[CCCSposNode] - state.iSolution[CCCSnegNode];
                case "ir": return (State state) => state.Solution[CCCScontBranch] * bp.CCCScoeff.Value;
                case "ii": return (State state) => state.iSolution[CCCScontBranch] * bp.CCCScoeff.Value;
                case "pr": return (State state) =>
                    {
                        double vr = state.Solution[CCCSposNode] - state.Solution[CCCSnegNode];
                        double vi = state.iSolution[CCCSposNode] - state.iSolution[CCCSnegNode];
                        double ir = state.Solution[CCCScontBranch] * bp.CCCScoeff.Value;
                        double ii = state.iSolution[CCCScontBranch] * bp.CCCScoeff.Value;
                        return vr * ir - vi * ii;
                    };
                case "pi": return (State state) =>
                    {
                        double vr = state.Solution[CCCSposNode] - state.Solution[CCCSnegNode];
                        double vi = state.iSolution[CCCSposNode] - state.iSolution[CCCSnegNode];
                        double ir = state.Solution[CCCScontBranch] * bp.CCCScoeff.Value;
                        double ii = state.iSolution[CCCScontBranch] * bp.CCCScoeff.Value;
                        return vr * ii + vi * ir;
                    };
                default: return null;
            }
        }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();

            // Get behaviors
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
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            CCCScontBranch = vsrcload.VSRCbranch;
            CCCSposContBrptr = matrix.GetElement(CCCSposNode, CCCScontBranch);
            CCCSnegContBrptr = matrix.GetElement(CCCSnegNode, CCCScontBranch);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            CCCSposContBrptr = null;
            CCCSnegContBrptr = null;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
            CCCSposContBrptr.Add(bp.CCCScoeff);
            CCCSnegContBrptr.Sub(bp.CCCScoeff);
        }
    }
}
