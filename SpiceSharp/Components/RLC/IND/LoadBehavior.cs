using SpiceSharp.Sparse;
using SpiceSharp.Components;
using SpiceSharp.Circuits;

namespace SpiceSharp.Behaviors.IND
{
    /// <summary>
    /// Load behavior for a <see cref="Components.Inductor"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior
    {
        /// <summary>
        /// Nodes
        /// </summary>
        public int INDbrEq { get; protected set; }
        public int INDposNode { get; protected set; }
        public int INDnegNode { get; protected set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement INDposIbrptr { get; private set; }
        protected MatrixElement INDnegIbrptr { get; private set; }
        protected MatrixElement INDibrNegptr { get; private set; }
        protected MatrixElement INDibrPosptr { get; private set; }

        /// <summary>
        /// Setup the load behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(Entity component, Circuit ckt)
        {
            var ind = component as Inductor;

            // Create branch equation
            INDbrEq = CreateNode(ckt, ind.Name.Grow("#branch"), Node.NodeType.Current).Index;

            // Get nodes
            INDposNode = ind.INDposNode;
            INDnegNode = ind.INDnegNode;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            INDposIbrptr = matrix.GetElement(INDposNode, INDbrEq);
            INDnegIbrptr = matrix.GetElement(INDnegNode, INDbrEq);
            INDibrNegptr = matrix.GetElement(INDbrEq, INDnegNode);
            INDibrPosptr = matrix.GetElement(INDbrEq, INDposNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            INDposIbrptr = null;
            INDnegIbrptr = null;
            INDibrNegptr = null;
            INDibrPosptr = null;
        }

        /// <summary>
        /// Load behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            INDposIbrptr.Add(1.0);
            INDnegIbrptr.Sub(1.0);
            INDibrPosptr.Add(1.0);
            INDibrNegptr.Sub(1.0);
        }
    }
}
