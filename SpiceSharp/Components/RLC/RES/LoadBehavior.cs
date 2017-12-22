using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Components.RES;

namespace SpiceSharp.Behaviors.RES
{
    /// <summary>
    /// General behavior for <see cref="Components.Resistor"/>
    /// </summary>
    public class LoadBehavior : Behaviors.LoadBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("i"), SpiceInfo("Current")]
        public double GetCurrent(Circuit ckt)
        {
            return (ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode]) * RESconduct;
        }
        [SpiceName("p"), SpiceInfo("Power")]
        public double GetPower(Circuit ckt)
        {
            return (ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode]) *
                (ckt.State.Solution[RESposNode] - ckt.State.Solution[RESnegNode]) * RESconduct;
        }

        /// <summary>
        /// Nodes
        /// </summary>
        public int RESposNode { get; protected set; }
        public int RESnegNode { get; protected set; }

        /// <summary>
        /// Conductance
        /// </summary>
        public double RESconduct { get; protected set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement RESposPosPtr { get; private set; }
        protected MatrixElement RESnegNegPtr { get; private set; }
        protected MatrixElement RESposNegPtr { get; private set; }
        protected MatrixElement RESnegPosPtr { get; private set; }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="pool">The behavior pool</param>
        /// <param name="parameters">Parameters</param>
        public override void Setup(ParametersCollection parameters, BehaviorPool pool)
        {
            var p = parameters.Get<BaseParameters>();
            if (!p.RESresist.Given)
            {
                var temp = pool.GetBehavior<TemperatureBehavior>();
                RESconduct = temp.RESconduct;
            }
            else
            {
                if (p.RESresist.Value < 1e-12)
                    RESconduct = 1e12;
                else
                    RESconduct = 1.0 / p.RESresist.Value;
            }
        }

        /// <summary>
        /// Connect the behavior to nodes
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            RESposNode = pins[0];
            RESnegNode = pins[1];
        }
        
        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Nodes nodes, Matrix matrix)
        {
            // Get matrix elements
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
        /// Load the matrix
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            RESposPosPtr.Add(RESconduct);
            RESnegNegPtr.Add(RESconduct);
            RESposNegPtr.Sub(RESconduct);
            RESnegPosPtr.Sub(RESconduct);
        }
    }
}
