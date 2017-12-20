using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Behaviors.CAP
{
    /// <summary>
    /// AC behavior for <see cref="Capacitor"/>
    /// </summary>
    public class AcBehavior : Behaviors.AcBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private TransientBehavior tran;

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
        public override void Setup(Entity component, Circuit ckt)
        {
            // Get behaviors
            tran = GetBehavior<TransientBehavior>(component);
            
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
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var cstate = ckt.State;
            var val = cstate.Laplace * tran.CAPcapac.Value;

            // Load the matrix
            CAPposPosptr.Add(val);
            CAPnegNegptr.Add(val);
            CAPposNegptr.Sub(val);
            CAPnegPosptr.Sub(val);
        }
    }
}
