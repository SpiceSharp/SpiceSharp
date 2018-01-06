using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors.CSW
{
    /// <summary>
    /// AC behavior for a <see cref="Components.CurrentSwitch"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        LoadBehavior load;
        ModelLoadBehavior modelload;

        /// <summary>
        /// Nodes
        /// </summary>
        int CSWposNode, CSWnegNode, CSWcontBranch;
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
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>();
            modelload = provider.GetBehavior<ModelLoadBehavior>(1);
        }
        
        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            CSWposNode = pins[0];
            CSWnegNode = pins[1];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            CSWcontBranch = load.CSWcontBranch;
            CSWposPosptr = matrix.GetElement(CSWposNode, CSWposNode);
            CSWposNegptr = matrix.GetElement(CSWposNode, CSWnegNode);
            CSWnegPosptr = matrix.GetElement(CSWnegNode, CSWposNode);
            CSWnegNegptr = matrix.GetElement(CSWnegNode, CSWnegNode);
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
