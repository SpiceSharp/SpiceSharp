using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="Diode"/>
    /// </summary>
    public class DiodeAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private DiodeModelTemperatureBehavior modeltemp;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("area"), SpiceInfo("Area factor")]
        public Parameter DIOarea { get; } = new Parameter(1);

        /// <summary>
        /// Nodes
        /// </summary>
        private int DIOposNode, DIOnegNode, DIOposPrimeNode;
        private MatrixElement DIOposPosPrimePtr;
        private MatrixElement DIOnegPosPrimePtr;
        private MatrixElement DIOposPrimePosPtr;
        private MatrixElement DIOposPrimeNegPtr;
        private MatrixElement DIOposPosPtr;
        private MatrixElement DIOnegNegPtr;
        private MatrixElement DIOposPrimePosPrimePtr;

        /// <summary>
        /// Private variables
        /// </summary>
        private int DIOstate;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            var diode = component as Diode;
            var model = diode.Model as DiodeModel;

            // Get necessary behaviors
            modeltemp = model.GetBehavior(typeof(CircuitObjectBehaviorTemperature)) as DiodeModelTemperatureBehavior;
            var load = diode.GetBehavior(typeof(CircuitObjectBehaviorLoad)) as DiodeLoadBehavior;

            // Nodes
            DIOposNode = diode.DIOposNode;
            DIOnegNode = diode.DIOnegNode;
            DIOposPrimeNode = load.DIOposPrimeNode;

            var matrix = ckt.State.Matrix;
            DIOposPosPrimePtr = matrix.GetElement(DIOposNode, DIOposPrimeNode);
            DIOnegPosPrimePtr = matrix.GetElement(DIOnegNode, DIOposPrimeNode);
            DIOposPrimePosPtr = matrix.GetElement(DIOposPrimeNode, DIOposNode);
            DIOposPrimeNegPtr = matrix.GetElement(DIOposPrimeNode, DIOnegNode);
            DIOposPosPtr = matrix.GetElement(DIOposNode, DIOposNode);
            DIOnegNegPtr = matrix.GetElement(DIOnegNode, DIOnegNode);
            DIOposPrimePosPrimePtr = matrix.GetElement(DIOposPrimeNode, DIOposPrimeNode);
            return true;
        }

        /// <summary>
        /// Unsetup the device
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Unsetup()
        {
            DIOposPosPrimePtr = null;
            DIOnegPosPrimePtr = null;
            DIOposPrimePosPtr = null;
            DIOposPrimeNegPtr = null;
            DIOposPosPtr = null;
            DIOnegNegPtr = null;
            DIOposPrimePosPrimePtr = null;
        }

        /// <summary>
        /// Perform AC analysis
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            double gspr, geq, xceq;

            gspr = modeltemp.DIOconductance * DIOarea;
            geq = state.States[0][DIOstate + DiodeLoadBehavior.DIOconduct];
            xceq = state.States[0][DIOstate + DiodeLoadBehavior.DIOcapCurrent] * state.Laplace.Imaginary;

            DIOposPosPtr.Value.Real += gspr;
            DIOnegNegPtr.Value.Cplx += new Complex(geq, xceq);

            DIOposPrimePosPrimePtr.Value.Cplx += new Complex(geq + gspr, xceq);

            DIOposPosPrimePtr.Value.Real -= gspr;
            DIOnegPosPrimePtr.Value.Cplx -= new Complex(geq, xceq);

            DIOposPrimePosPtr.Value.Real -= gspr;
            DIOposPrimeNegPtr.Value.Cplx -= new Complex(geq, xceq);
        }
    }
}
