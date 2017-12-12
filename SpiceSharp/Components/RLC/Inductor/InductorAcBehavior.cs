using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="Inductor"/>
    /// </summary>
    public class InductorAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private InductorLoadBehavior load;

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
        protected MatrixElement INDibrIbrptr { get; private set; }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            var ind = component as Inductor;

            // Get behaviors
            load = GetBehavior<InductorLoadBehavior>(component);

            // Get nodes
            INDposNode = ind.INDposNode;
            INDnegNode = ind.INDnegNode;
            INDbrEq = load.INDbrEq;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            INDposIbrptr = matrix.GetElement(INDposNode, INDbrEq);
            INDnegIbrptr = matrix.GetElement(INDnegNode, INDbrEq);
            INDibrNegptr = matrix.GetElement(INDbrEq, INDnegNode);
            INDibrPosptr = matrix.GetElement(INDbrEq, INDposNode);
            INDibrIbrptr = matrix.GetElement(INDbrEq, INDbrEq);
            return true;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var cstate = ckt.State;
            Complex val = cstate.Laplace * load.INDinduct.Value;

            INDposIbrptr.Add(1.0);
            INDnegIbrptr.Sub(1.0);
            INDibrNegptr.Sub(1.0);
            INDibrPosptr.Add(1.0);
            INDibrIbrptr.Sub(val);
        }
    }
}
