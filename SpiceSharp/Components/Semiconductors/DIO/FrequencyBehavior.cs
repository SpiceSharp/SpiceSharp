using System;
using System.Numerics;
using SpiceSharp.Sparse;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;

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
        protected Element<Complex> PosPosPrimePtr { get; private set; }
        protected Element<Complex> NegPosPrimePtr { get; private set; }
        protected Element<Complex> PosPrimePosPtr { get; private set; }
        protected Element<Complex> PosPrimeNegPtr { get; private set; }
        protected Element<Complex> PosPosPtr { get; private set; }
        protected Element<Complex> NegNegPtr { get; private set; }
        protected Element<Complex> PosPrimePosPrimePtr { get; private set; }

        /// <summary>
        /// Gets the junction capacitance
        /// </summary>
        [PropertyName("cd"), PropertyInfo("Diode capacitance")]
        public double Capacitance { get; protected set; }
        [PropertyName("vd"), PropertyInfo("Voltage across the internal diode")]
        public Complex GetDiodeVoltage(ComplexState state) => state.Solution[posPrimeNode] - state.Solution[negNode];
        [PropertyName("v"), PropertyInfo("Voltage across the diode")]
        public Complex GetVoltage(ComplexState state) => state.Solution[posNode] - state.Solution[negNode];
        [PropertyName("i"), PropertyName("id"), PropertyInfo("Current through the diode")]
        public Complex GetCurrent(ComplexState state)
        {
            Complex geq = Capacitance * state.Laplace + load.Conduct;
            Complex voltage = state.Solution[posPrimeNode] - state.Solution[negNode];
            return voltage * geq;
        }
        [PropertyName("p"), PropertyName("pd"), PropertyInfo("Power")]
        public Complex GetPower(ComplexState state)
        {
            Complex geq = Capacitance * state.Laplace + load.Conduct;
            Complex current = (state.Solution[posPrimeNode] - state.Solution[negNode]) * geq;
            Complex voltage = (state.Solution[posNode] - state.Solution[negNode]);
            return voltage * -Complex.Conjugate(current);
        }

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
            bp = provider.GetParameterSet<BaseParameters>("entity");
            mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>("entity");
            temp = provider.GetBehavior<TemperatureBehavior>("entity");
            modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
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
                throw new Diagnostics.CircuitException("Pin count mismatch: 2 pins expected, {0} given".FormatString(pins.Length));
            posNode = pins[0];
            negNode = pins[1];
        }
        
        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix<Complex> matrix)
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
        /// <param name="simulation"></param>
        public override void InitializeParameters(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.RealState;
            double arg, czero, sarg, capd, czof2;
            double vd = state.Solution[posPrimeNode] - state.Solution[negNode];

            // charge storage elements
            czero = temp.TempJunctionCap * bp.Area;
            if (vd < temp.TempDepletionCap)
            {
                arg = 1 - vd / mbp.JunctionPotential;
                sarg = Math.Exp(-mbp.GradingCoefficient * Math.Log(arg));
                capd = mbp.TransitTime * load.Conduct + czero * sarg;
            }
            else
            {
                czof2 = czero / modeltemp.F2;
                capd = mbp.TransitTime * load.Conduct + czof2 * (modeltemp.F3 + mbp.GradingCoefficient * vd / mbp.JunctionPotential);
            }
            Capacitance = capd;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public override void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var state = simulation.ComplexState;
            double gspr, geq, xceq;

            gspr = modeltemp.Conductance * bp.Area;
            geq = load.Conduct;
            xceq = Capacitance * state.Laplace.Imaginary;

            PosPosPtr.Add(gspr);
            NegNegPtr.Add(new Complex(geq, xceq));

            PosPrimePosPrimePtr.Add(new Complex(geq + gspr, xceq));

            PosPosPrimePtr.Sub(gspr);
            NegPosPrimePtr.Sub(new Complex(geq, xceq));

            PosPrimePosPtr.Sub(gspr);
            PosPrimeNegPtr.Sub(new Complex(geq, xceq));
        }
    }
}
