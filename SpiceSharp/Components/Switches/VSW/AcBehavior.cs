using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors.VSW
{
    /// <summary>
    /// AC behavior for <see cref="VoltageSwitch"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private LoadBehavior load;
        private ModelLoadBehavior modelload;

        /// <summary>
        /// Nodes
        /// </summary>
        protected int VSWposNode, VSWnegNode, VSWcontPosNode, VSWcontNegNode;

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement SWposPosptr { get; private set; }
        protected MatrixElement SWnegPosptr { get; private set; }
        protected MatrixElement SWposNegptr { get; private set; }
        protected MatrixElement SWnegNegptr { get; private set; }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(Entity component, Circuit ckt)
        {
            var vsw = component as VoltageSwitch;

            // Get behaviors
            load = GetBehavior<LoadBehavior>(component);
            modelload = GetBehavior<ModelLoadBehavior>(vsw.Model);

            // Get nodes
            VSWposNode = vsw.VSWposNode;
            VSWnegNode = vsw.VSWnegNode;
            VSWcontPosNode = vsw.VSWcontPosNode;
            VSWcontNegNode = vsw.VSWcontNegNode;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            SWposPosptr = matrix.GetElement(VSWposNode, VSWposNode);
            SWposNegptr = matrix.GetElement(VSWposNode, VSWnegNode);
            SWnegPosptr = matrix.GetElement(VSWnegNode, VSWposNode);
            SWnegNegptr = matrix.GetElement(VSWnegNode, VSWnegNode);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            SWposPosptr = null;
            SWnegNegptr = null;
            SWposNegptr = null;
            SWnegPosptr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            double current_state, g_now;
            var state = ckt.State;
            var cstate = state;

            // Get the current state
            current_state = state.States[0][load.VSWstate];
            g_now = current_state > 0.0 ? modelload.VSWonConduct : modelload.VSWoffConduct;

            // Load the Y-matrix
            SWposPosptr.Add(g_now);
            SWposNegptr.Sub(g_now);
            SWnegPosptr.Sub(g_now);
            SWnegNegptr.Add(g_now);
        }
    }
}
