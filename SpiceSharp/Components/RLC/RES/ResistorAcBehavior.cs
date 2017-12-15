using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="Resistor"/>
    /// </summary>
    public class ResistorAcBehavior : CircuitObjectBehaviorAcLoad
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
        private ResistorLoadBehavior load;

        /// <summary>
        /// Nodes
        /// </summary>
        public int RESposNode { get; private set; }
        public int RESnegNode { get; private set; }

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
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var res = component as Resistor;

            // Get behaviors
            load = GetBehavior<ResistorLoadBehavior>(component);

            // Nodes
            RESposNode = res.RESposNode;
            RESnegNode = res.RESnegNode;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            RESposPosPtr = matrix.GetElement(RESposNode, RESposNode);
            RESnegNegPtr = matrix.GetElement(RESnegNode, RESnegNode);
            RESposNegPtr = matrix.GetElement(RESposNode, RESnegNode);
            RESnegPosPtr = matrix.GetElement(RESnegNode, RESposNode);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Unsetup()
        {
            // Remove references
            RESposPosPtr = null;
            RESnegNegPtr = null;
            RESposNegPtr = null;
            RESnegPosPtr = null;
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
