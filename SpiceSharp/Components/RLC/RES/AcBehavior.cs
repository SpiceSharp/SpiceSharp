using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Attributes;

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
        /// <param name="ckt"></param>
        /// <returns></returns>
        [SpiceName("i"), SpiceInfo("Current")]
        public Complex GetCurrent(Circuit ckt)
        {
            var voltage = new Complex(
                ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode],
                ckt.State.iSolution[RESposNode] - ckt.State.iSolution[RESnegNode]);
            return voltage * load.RESconduct;
        }
        [SpiceName("p"), SpiceInfo("Power")]
        public Complex GetPower(Circuit ckt)
        {
            var voltage = new Complex(
                ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode],
                ckt.State.iSolution[RESposNode] - ckt.State.iSolution[RESnegNode]);
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
        /// <param name="parameters">Parameters</param>
        /// <param name="pool">Pool of behaviors</param>
        public override void Setup(ParametersCollection parameters, BehaviorPool pool)
        {
            load = pool.GetBehavior<LoadBehavior>();
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
        /// <param name="nodes"></param>
        public void Connect(params int[] pins)
        {
            RESposNode = pins[0];
            RESnegNode = pins[1];
        }

        /// <summary>
        /// Perform AC calculations
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            RESposPosPtr.Add(load.RESconduct);
            RESnegNegPtr.Add(load.RESconduct);
            RESposNegPtr.Sub(load.RESconduct);
            RESnegPosPtr.Sub(load.RESconduct);
        }
    }
}
