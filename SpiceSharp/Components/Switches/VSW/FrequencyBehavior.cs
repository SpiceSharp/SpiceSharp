using System;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.VoltageSwitchBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="VoltageSwitch"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        LoadBehavior load;
        ModelLoadBehavior modelload;

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
        public FrequencyBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>(0);
            modelload = provider.GetBehavior<ModelLoadBehavior>(1);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 4)
                throw new Diagnostics.CircuitException($"Pin count mismatch: 4 pins expected, {pins.Length} given");
            VSWposNode = pins[0];
            VSWnegNode = pins[1];
            VSWcontPosNode = pins[2];
            VSWcontNegNode = pins[3];
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

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
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            double g_now;
            var state = sim.State;
            var cstate = state;

            // Get the current state
            g_now = load.VSWcurrentState == true ? modelload.VSWonConduct : modelload.VSWoffConduct;

            // Load the Y-matrix
            SWposPosptr.Add(g_now);
            SWposNegptr.Sub(g_now);
            SWnegPosptr.Sub(g_now);
            SWnegNegptr.Add(g_now);
        }
    }
}
