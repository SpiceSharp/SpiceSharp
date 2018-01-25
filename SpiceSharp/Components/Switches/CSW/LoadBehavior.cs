using SpiceSharp.Circuits;
using SpiceSharp.Components.CSW;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Behaviors.CSW
{
    /// <summary>
    /// General behavior for a <see cref="Components.CurrentSwitch"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        ModelLoadBehavior modelload;
        VSRC.LoadBehavior vsrcload;
        ModelBaseParameters mbp;

        /// <summary>
        /// Methods
        /// </summary>
        [PropertyName("v"), PropertyInfo("Switch voltage")]
        public double GetVoltage(State state) => state.Solution[CSWposNode] - state.Solution[CSWnegNode];
        [PropertyName("i"), PropertyInfo("Switch current")]
        public double GetCurrent(State state) => (state.Solution[CSWposNode] - state.Solution[CSWnegNode]) * CSWcond;
        [PropertyName("p"), PropertyInfo("Instantaneous power")]
        public double GetPower(State state) => (state.Solution[CSWposNode] - state.Solution[CSWnegNode]) *
            (state.Solution[CSWposNode] - state.Solution[CSWnegNode]) * CSWcond;
        public double CSWcond { get; internal set; }

        /// <summary>
        /// Nodes
        /// </summary>
        public int CSWcontBranch { get; private set; }
        protected int CSWposNode, CSWnegNode;
        protected MatrixElement CSWposPosptr { get; private set; }
        protected MatrixElement CSWnegPosptr { get; private set; }
        protected MatrixElement CSWposNegptr { get; private set; }
        protected MatrixElement CSWnegNegptr { get; private set; }

        /// <summary>
        /// Gets or sets the old state of the switch
        /// </summary>
        public bool CSWoldState { get; set; }

        /// <summary>
        /// Flag for using the old state or not
        /// </summary>
        public bool CSWuseOldState { get; set; } = false;

        /// <summary>
        /// Gets the current state of the switch
        /// </summary>
        public bool CSWcurrentState { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public LoadBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
            mbp = provider.GetParameterSet<ModelBaseParameters>(1);

            // Get behaviors
            modelload = provider.GetBehavior<ModelLoadBehavior>(1);
            vsrcload = provider.GetBehavior<VSRC.LoadBehavior>(2);
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
        /// <param name="nodes">Nodes</param>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            CSWcontBranch = vsrcload.VSRCbranch;
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
            CSWposNegptr = null;
            CSWnegPosptr = null;
            CSWnegNegptr = null;
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Load(BaseSimulation sim)
        {
            double g_now;
            double i_ctrl;
            bool previous_state;
            bool current_state = false;
            var state = sim.State;

            // decide the state of the switch
            if (state.Init == State.InitFlags.InitFix || state.Init == State.InitFlags.InitJct)
            {
                if (bp.CSWzero_state)
                {
                    // Switch specified "on"
                    CSWcurrentState = true;
                    current_state = true;
                }
                else
                {
                    // Switch specified "off"
                    CSWcurrentState = false;
                    current_state = false;
                }
            }
            else
            {
                // Get the previous state
                if (CSWuseOldState)
                    previous_state = CSWoldState;
                else
                    previous_state = CSWcurrentState;
                i_ctrl = state.Solution[CSWcontBranch];

                // Calculate the current state
                if (i_ctrl > (mbp.CSWthresh + mbp.CSWhyst))
                    current_state = true;
                else if (i_ctrl < (mbp.CSWthresh - mbp.CSWhyst))
                    current_state = false;
                else
                    current_state = previous_state;

                // Store the current state
                CSWcurrentState = current_state;
            }

            // Get the current conduction
            g_now = current_state != false ? (modelload.CSWonConduct) : (modelload.CSWoffConduct);
            CSWcond = g_now;

            // Load the Y-matrix
            CSWposPosptr.Add(g_now);
            CSWposNegptr.Sub(g_now);
            CSWnegPosptr.Sub(g_now);
            CSWnegNegptr.Add(g_now);
        }
    }
}
