using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Components.CCCS;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;
using System;
using System.Numerics;

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
        /// Properties
        /// </summary>
        [PropertyName("v"), PropertyInfo("Complex voltage")]
        public Complex GetVoltage(State state)
        {
            return new Complex(
                state.Solution[CCCSposNode] - state.Solution[CCCSnegNode],
                state.iSolution[CCCSposNode] - state.iSolution[CCCSnegNode]);
        }
        [PropertyName("i"), PropertyInfo("Complex current")]
        public Complex GetCurrent(State state)
        {
            return new Complex(
                state.Solution[CCCScontBranch],
                state.iSolution[CCCScontBranch]
                ) * bp.CCCScoeff.Value;
        }
        [PropertyName("p"), PropertyInfo("Complex power")]
        public Complex GetPower(State state)
        {
            Complex v = new Complex(state.Solution[CCCSposNode], state.iSolution[CCCSnegNode]);
            Complex i = new Complex(state.Solution[CCCScontBranch], state.iSolution[CCCScontBranch]) * bp.CCCScoeff.Value;
            return -v * Complex.Conjugate(i);
        }

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
