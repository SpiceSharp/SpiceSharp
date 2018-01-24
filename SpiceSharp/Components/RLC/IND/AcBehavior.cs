using System.Numerics;
using SpiceSharp.Components.IND;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.IND
{
    /// <summary>
    /// AC behavior for <see cref="Components.Inductor"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        LoadBehavior load;

        /// <summary>
        /// Nodes
        /// </summary>
        int INDposNode, INDnegNode, INDbrEq;
        protected MatrixElement INDposIbrptr { get; private set; }
        protected MatrixElement INDnegIbrptr { get; private set; }
        protected MatrixElement INDibrNegptr { get; private set; }
        protected MatrixElement INDibrPosptr { get; private set; }
        protected MatrixElement INDibrIbrptr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>();
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            INDposNode = pins[0];
            INDnegNode = pins[1];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            // Get current equation
            INDbrEq = load.INDbrEq;

            // Get matrix pointers
            INDposIbrptr = matrix.GetElement(INDposNode, INDbrEq);
            INDnegIbrptr = matrix.GetElement(INDnegNode, INDbrEq);
            INDibrNegptr = matrix.GetElement(INDbrEq, INDnegNode);
            INDibrPosptr = matrix.GetElement(INDbrEq, INDposNode);
            INDibrIbrptr = matrix.GetElement(INDbrEq, INDbrEq);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            INDposIbrptr = null;
            INDnegIbrptr = null;
            INDibrPosptr = null;
            INDibrNegptr = null;
            INDibrIbrptr = null;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
            var state = sim.State;
            Complex val = state.Laplace * bp.INDinduct.Value;

            INDposIbrptr.Add(1.0);
            INDnegIbrptr.Sub(1.0);
            INDibrNegptr.Sub(1.0);
            INDibrPosptr.Add(1.0);
            INDibrIbrptr.Sub(val);
        }
    }
}
