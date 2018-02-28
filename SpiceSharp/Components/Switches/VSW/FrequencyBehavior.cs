using System;
using System.Numerics;
using SpiceSharp.Algebra;
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
        int posNode, negNode;
        protected MatrixElement<Complex> PosPosPtr { get; private set; }
        protected MatrixElement<Complex> NegPosPtr { get; private set; }
        protected MatrixElement<Complex> PosNegPtr { get; private set; }
        protected MatrixElement<Complex> NegNegPtr { get; private set; }

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
            load = provider.GetBehavior<LoadBehavior>("entity");
            modelload = provider.GetBehavior<ModelLoadBehavior>("model");
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
                throw new Diagnostics.CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            posNode = pins[0];
            negNode = pins[1];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Solver<Complex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            PosPosPtr = solver.GetMatrixElement(posNode, posNode);
            PosNegPtr = solver.GetMatrixElement(posNode, negNode);
            NegPosPtr = solver.GetMatrixElement(negNode, posNode);
            NegNegPtr = solver.GetMatrixElement(negNode, negNode);
        }
        
        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            PosPosPtr = null;
            NegNegPtr = null;
            PosNegPtr = null;
            NegPosPtr = null;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double g_now;

            // Get the current state
            g_now = load.CurrentState == true ? modelload.OnConductance : modelload.OffConductance;

            // Load Y-matrix
            PosPosPtr.Value += g_now;
            PosNegPtr.Value -= g_now;
            NegPosPtr.Value -= g_now;
            NegNegPtr.Value += g_now;
        }
    }
}
