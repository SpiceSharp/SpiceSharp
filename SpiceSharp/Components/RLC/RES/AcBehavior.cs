using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.RES
{
    /// <summary>
    /// AC behavior for <see cref="Components.Resistor"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [PropertyName("v"), PropertyInfo("Voltage")]
        public Complex GetVoltage(State state)
        {
            return new Complex(
                state.Solution[RESposNode] - state.Solution[RESnegNode],
                state.iSolution[RESposNode] - state.iSolution[RESnegNode]);
        }
        [PropertyName("i"), PropertyInfo("Current")]
        public Complex GetCurrent(State state)
        {
            var voltage = new Complex(
                state.Solution[RESposNode] - state.Solution[RESnegNode],
                state.iSolution[RESposNode] - state.iSolution[RESnegNode]);
            return voltage * load.RESconduct;
        }
        [PropertyName("p"), PropertyInfo("Power")]
        public Complex GetPower(State state)
        {
            var voltage = new Complex(
                state.Solution[RESposNode] - state.Solution[RESnegNode],
                state.iSolution[RESposNode] - state.iSolution[RESnegNode]);
            return voltage * Complex.Conjugate(voltage) * load.RESconduct;
        }

        /// <summary>
        /// Necessary behaviors
        /// </summary>
        LoadBehavior load;

        /// <summary>
        /// Nodes
        /// </summary>
        public int RESposNode { get; protected set; }
        public int RESnegNode { get; protected set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement RESposPosPtr { get; private set; }
        protected MatrixElement RESnegNegPtr { get; private set; }
        protected MatrixElement RESposNegPtr { get; private set; }
        protected MatrixElement RESnegPosPtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>();
        }
        
        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            RESposPosPtr = matrix.GetElement(RESposNode, RESposNode);
            RESnegNegPtr = matrix.GetElement(RESnegNode, RESnegNode);
            RESposNegPtr = matrix.GetElement(RESposNode, RESnegNode);
            RESnegPosPtr = matrix.GetElement(RESnegNode, RESposNode);
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            RESposPosPtr = null;
            RESnegNegPtr = null;
            RESposNegPtr = null;
            RESnegPosPtr = null;
        }

        /// <summary>
        /// Connect behavior
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            RESposNode = pins[0];
            RESnegNode = pins[1];
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public override void Load(FrequencySimulation sim)
        {
            RESposPosPtr.Add(load.RESconduct);
            RESnegNegPtr.Add(load.RESconduct);
            RESposNegPtr.Sub(load.RESconduct);
            RESnegPosPtr.Sub(load.RESconduct);
        }
    }
}
