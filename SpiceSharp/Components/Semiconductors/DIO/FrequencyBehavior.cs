using System;
using System.Numerics;
using SpiceSharp.Sparse;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// AC behavior for <see cref="Diode"/>
    /// </summary>
    public class FrequencyBehavior : Behaviors.FrequencyBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        BaseParameters bp;
        ModelBaseParameters mbp;
        LoadBehavior load;
        TemperatureBehavior temp;
        ModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Nodes
        /// </summary>
        int posNode, negNode, posPrimeNode;
        protected MatrixElement PosPosPrimePtr { get; private set; }
        protected MatrixElement NegPosPrimePtr { get; private set; }
        protected MatrixElement PosPrimePosPtr { get; private set; }
        protected MatrixElement PosPrimeNegPtr { get; private set; }
        protected MatrixElement PosPosPtr { get; private set; }
        protected MatrixElement NegNegPtr { get; private set; }
        protected MatrixElement PosPrimePosPrimePtr { get; private set; }

        /// <summary>
        /// Gets the junction capacitance
        /// </summary>
        public double Cap { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>(0);
            mbp = provider.GetParameterSet<ModelBaseParameters>(1);

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>(0);
            temp = provider.GetBehavior<TemperatureBehavior>(0);
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>(1);
        }
        
        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 2)
                throw new Diagnostics.CircuitException($"Pin count mismatch: 2 pins expected, {pins.Length} given");
            posNode = pins[0];
            negNode = pins[1];
        }
        
        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));

            // Get node
            posPrimeNode = load.PosPrimeNode;

            // Get matrix pointers
            PosPosPrimePtr = matrix.GetElement(posNode, posPrimeNode);
            NegPosPrimePtr = matrix.GetElement(negNode, posPrimeNode);
            PosPrimePosPtr = matrix.GetElement(posPrimeNode, posNode);
            PosPrimeNegPtr = matrix.GetElement(posPrimeNode, negNode);
            PosPosPtr = matrix.GetElement(posNode, posNode);
            NegNegPtr = matrix.GetElement(negNode, negNode);
            PosPrimePosPrimePtr = matrix.GetElement(posPrimeNode, posPrimeNode);
        }

        /// <summary>
        /// Unsetup the device
        /// </summary>
        public override void Unsetup()
        {
            PosPosPrimePtr = null;
            NegPosPrimePtr = null;
            PosPrimePosPtr = null;
            PosPrimeNegPtr = null;
            PosPosPtr = null;
            NegNegPtr = null;
            PosPrimePosPrimePtr = null;
        }

        /// <summary>
        /// Calculate AC parameters
        /// </summary>
        /// <param name="sim"></param>
        public override void InitializeParameters(FrequencySimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            var state = sim.State;
            double arg, czero, sarg, capd, czof2;
            double vd = state.Solution[posPrimeNode] - state.Solution[negNode];

            // charge storage elements
            czero = temp.TJctCap * bp.Area;
            if (vd < temp.TDepCap)
            {
                arg = 1 - vd / mbp.JunctionPot;
                sarg = Math.Exp(-mbp.GradingCoeff * Math.Log(arg));
                capd = mbp.TransitTime * load.Conduct + czero * sarg;
            }
            else
            {
                czof2 = czero / modeltemp.F2;
                capd = mbp.TransitTime * load.Conduct + czof2 * (modeltemp.F3 + mbp.GradingCoeff * vd / mbp.JunctionPot);
            }
            Cap = capd;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            var state = sim.State;
            double gspr, geq, xceq;

            gspr = modeltemp.Conductance * bp.Area;
            geq = load.Conduct;
            xceq = Cap * state.Laplace.Imaginary;

            PosPosPtr.Value.Real += gspr;
            NegNegPtr.Value.Cplx += new Complex(geq, xceq);

            PosPrimePosPrimePtr.Value.Cplx += new Complex(geq + gspr, xceq);

            PosPosPrimePtr.Value.Real -= gspr;
            NegPosPrimePtr.Value.Cplx -= new Complex(geq, xceq);

            PosPrimePosPtr.Value.Real -= gspr;
            PosPrimeNegPtr.Value.Cplx -= new Complex(geq, xceq);
        }
    }
}
