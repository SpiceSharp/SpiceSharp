using SpiceSharp.Behaviors;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="Capacitor"/>
    /// </summary>
    public class CapacitorAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("capacitance"), SpiceInfo("Device capacitance", IsPrincipal = true)]
        public Parameter CAPcapac { get; } = new Parameter();

        /// <summary>
        /// Nodes
        /// </summary>
        private int CAPstate;
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
            base.Setup(component, ckt);

            // If the capacitance is not given, try getting it from the temperature behavior
            if (!CAPcapac.Given)
            {
                var temp = component.GetBehavior(typeof(CircuitObjectBehaviorTemperature)) as CapacitorTemperatureBehavior;
                if (temp != null)
                    CAPcapac.Value = temp.CAPcapac;
            }

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
            Capacitor cap = ComponentTyped<Capacitor>();
            var cstate = ckt.State;
            var val = cstate.Laplace * CAPcapac.Value;

            // Load the matrix
            CAPposPosptr.Add(val);
            CAPnegNegptr.Add(val);
            CAPposNegptr.Sub(val);
            CAPnegPosptr.Sub(val);
        }
    }
}
