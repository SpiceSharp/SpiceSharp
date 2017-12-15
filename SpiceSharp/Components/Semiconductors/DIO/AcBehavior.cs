using System.Numerics;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;
using SpiceSharp.Components;

namespace SpiceSharp.Behaviors.DIO
{
    /// <summary>
    /// AC behaviour for <see cref="Diode"/>
    /// </summary>
    public class AcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private LoadBehavior load;
        private ModelTemperatureBehavior modeltemp;

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
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var diode = component as Diode;

            // Get necessary behaviors
            load = GetBehavior<LoadBehavior>(component);
            modeltemp = GetBehavior<ModelTemperatureBehavior>(diode.Model);

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

            gspr = modeltemp.DIOconductance * load.DIOarea;
            geq = state.States[0][load.DIOstate + LoadBehavior.DIOconduct];
            xceq = state.States[0][load.DIOstate + LoadBehavior.DIOcapCurrent] * state.Laplace.Imaginary;

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
