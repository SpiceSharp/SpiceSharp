using SpiceSharp.Sparse;
using SpiceSharp.Components.CCCS;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;
using System;
using System.Numerics;

namespace SpiceSharp.Behaviors.CCCS
{
    /// <summary>
    /// Frequency behavior for <see cref="Components.CurrentControlledCurrentsource"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
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
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return new Complex(
                state.Solution[CCCSposNode] - state.Solution[CCCSnegNode],
                state.iSolution[CCCSposNode] - state.iSolution[CCCSnegNode]);
        }
        [PropertyName("i"), PropertyInfo("Complex current")]
        public Complex GetCurrent(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

            return new Complex(
                state.Solution[CCCScontBranch],
                state.iSolution[CCCScontBranch]
                ) * bp.CCCScoeff.Value;
        }
        [PropertyName("p"), PropertyInfo("Complex power")]
        public Complex GetPower(State state)
        {
			if (state == null)
				throw new ArgumentNullException(nameof(state));

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
        public FrequencyBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);

            // Get behaviors
            vsrcload = provider.GetBehavior<VSRC.LoadBehavior>(1);
        }

        /// <summary>
        /// Connect the behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new CircuitException($"Pin count mismatch: 2 pins expected, {pins.Length} given");
            CCCSposNode = pins[0];
            CCCSnegNode = pins[1];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

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
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            CCCSposContBrptr.Add(bp.CCCScoeff);
            CCCSnegContBrptr.Sub(bp.CCCScoeff);
        }
    }
}
