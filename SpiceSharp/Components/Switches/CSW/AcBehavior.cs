using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors.CSW
{
    /// <summary>
    /// AC behavior for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        LoadBehavior load;
        ModelLoadBehavior modelload;

        /// <summary>
        /// Nodes
        /// </summary>
        protected int CSWposNode, CSWnegNode, CSWcontBranch;

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement CSWposPosptr { get; private set; }
        protected MatrixElement CSWnegPosptr { get; private set; }
        protected MatrixElement CSWposNegptr { get; private set; }
        protected MatrixElement CSWnegNegptr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(Entity component, Circuit ckt)
        {
            var csw = component as CurrentSwitch;

            // Get behaviors
            load = GetBehavior<LoadBehavior>(component);
            modelload = GetBehavior<ModelLoadBehavior>(csw.Model);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            CSWposPosptr = null;
            CSWnegNegptr = null;
            CSWposNegptr = null;
            CSWnegPosptr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            bool current_state;
            double g_now;
            var state = ckt.State;
            var cstate = state;

            // Get the current state
            current_state = load.CSWcurrentState;
            g_now = current_state != false ? modelload.CSWonConduct : modelload.CSWoffConduct;

            // Load the Y-matrix
            CSWposPosptr.Add(g_now);
            CSWposNegptr.Sub(g_now);
            CSWnegPosptr.Sub(g_now);
            CSWnegNegptr.Add(g_now);
        }
    }
}
