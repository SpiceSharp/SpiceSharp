using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Components.VSW;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.VSW
{
    /// <summary>
    /// Load behavior for a <see cref="Components.VoltageSwitch"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        ModelLoadBehavior modelload;
        ModelBaseParameters mbp;

        /// <summary>
        /// Gets or sets the previous state
        /// </summary>
        public bool VSWoldState { get; set; } = false;

        /// <summary>
        /// Flag for using the previous state or not
        /// </summary>
        public bool VSWuseOldState { get; set; } = false;

        /// <summary>
        /// The current state
        /// </summary>
        public bool VSWcurrentState { get; protected set; } = false;

        /// <summary>
        /// The current conductance
        /// </summary>
        public double VSWcond { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        int VSWposNode, VSWnegNode, VSWcontPosNode, VSWcontNegNode;
        protected MatrixElement SWposPosptr { get; private set; }
        protected MatrixElement SWnegPosptr { get; private set; }
        protected MatrixElement SWposNegptr { get; private set; }
        protected MatrixElement SWnegNegptr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
            mbp = provider.GetParameterSet<ModelBaseParameters>(1);

            // Get behaviors
            modelload = provider.GetBehavior<ModelLoadBehavior>(1);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            VSWposNode = pins[0];
            VSWnegNode = pins[1];
            VSWcontPosNode = pins[2];
            VSWcontNegNode = pins[3];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="matrix"></param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            SWposPosptr = matrix.GetElement(VSWposNode, VSWposNode);
            SWposNegptr = matrix.GetElement(VSWposNode, VSWnegNode);
            SWnegPosptr = matrix.GetElement(VSWnegNode, VSWposNode);
            SWnegNegptr = matrix.GetElement(VSWnegNode, VSWnegNode);
        }
        
        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            SWposPosptr = null;
            SWposNegptr = null;
            SWnegPosptr = null;
            SWnegNegptr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            double g_now;
            double v_ctrl;
            bool previous_state;
            bool current_state = false;
            var state = sim.State;

            if (state.Init == State.InitFlags.InitFix || state.Init == State.InitFlags.InitJct)
            {
                if (bp.VSWzero_state)
                {
                    // Switch specified "on"
                    VSWcurrentState = true;
                    current_state = true;
                }
                else
                {
                    // Switch specified "off"
                    VSWcurrentState = false;
                    current_state = false;
                }
            }
            else
            {
                if (VSWuseOldState)
                    previous_state = VSWoldState;
                else
                    previous_state = VSWcurrentState;
                v_ctrl = state.Solution[VSWcontPosNode] - state.Solution[VSWcontNegNode];

                // Calculate the current state
                if (v_ctrl > (mbp.VSWthresh + mbp.VSWhyst))
                    current_state = true;
                else if (v_ctrl < (mbp.VSWthresh - mbp.VSWhyst))
                    current_state = false;
                else
                    current_state = previous_state;

                // Store the current state
                VSWcurrentState = current_state;

                // If the state changed, ensure one more iteration
                if (current_state != previous_state)
                    state.IsCon = false;
            }

            g_now = current_state == true ? modelload.VSWonConduct : modelload.VSWoffConduct;
            VSWcond = g_now;

            // Load the Y-matrix
            SWposPosptr.Add(g_now);
            SWposNegptr.Sub(g_now);
            SWnegPosptr.Sub(g_now);
            SWnegNegptr.Add(g_now);
        }

    }
}
