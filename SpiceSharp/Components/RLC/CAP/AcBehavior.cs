using SpiceSharp.Behaviors;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components.CAP
{
    /// <summary>
    /// AC behaviour for <see cref="Capacitor"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private LoadBehavior load;

        /// <summary>
        /// Nodes
        /// </summary>
        private int CAPposNode, CAPnegNode;
        private MatrixElement CAPposPosptr;
        private MatrixElement CAPnegNegptr;
        private MatrixElement CAPposNegptr;
        private MatrixElement CAPnegPosptr;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component"></param>
        /// <param name="ckt"></param>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            // Get behaviors
            load = GetBehavior<LoadBehavior>(component);
            
            // Get nodes
            var cap = component as Capacitor;
            CAPposNode = cap.CAPposNode;
            CAPnegNode = cap.CAPnegNode;

            // Get matrix pointers
            var matrix = ckt.State.Matrix;
            CAPposPosptr = matrix.GetElement(CAPposNode, CAPposNode);
            CAPnegNegptr = matrix.GetElement(CAPnegNode, CAPnegNode);
            CAPnegPosptr = matrix.GetElement(CAPnegNode, CAPposNode);
            CAPposNegptr = matrix.GetElement(CAPposNode, CAPnegNode);
        }
        
        /// <summary>
        /// Execute behaviour for AC analysis
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var cstate = ckt.State;
            var val = cstate.Laplace * load.CAPcapac.Value;

            // Load the matrix
            CAPposPosptr.Add(val);
            CAPnegNegptr.Add(val);
            CAPposNegptr.Sub(val);
            CAPnegPosptr.Sub(val);
        }
    }
}
