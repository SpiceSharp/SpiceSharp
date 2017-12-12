using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Parameters;
using SpiceSharp.Circuits;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// AC behaviour for <see cref="CurrentControlledCurrentsource"/>
    /// </summary>
    public class CurrentControlledCurrentsourceAcBehavior : CircuitObjectBehaviorAcLoad
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private CurrentControlledCurrentsourceLoadBehavior load;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("i"), SpiceInfo("CCCS output current")]
        public Complex GetCurrent(Circuit ckt)
        {
            return new Complex(ckt.State.Solution[CCCScontBranch], ckt.State.iSolution[CCCScontBranch]) * load.CCCScoeff.Value;
        }
        [SpiceName("v"), SpiceInfo("CCCS voltage at output")]
        public Complex GetVoltage(Circuit ckt)
        {
            return new Complex(
                ckt.State.Solution[CCCSposNode] - ckt.State.Solution[CCCSnegNode],
                ckt.State.iSolution[CCCSposNode] - ckt.State.iSolution[CCCSnegNode]);
        }
        [SpiceName("p"), SpiceInfo("CCCS power")]
        public Complex GetPower(Circuit ckt)
        {
            Complex current = new Complex(ckt.State.Solution[CCCScontBranch], ckt.State.iSolution[CCCScontBranch]) * load.CCCScoeff.Value;
            Complex voltage = new Complex(
                ckt.State.Solution[CCCSposNode] - ckt.State.Solution[CCCSnegNode],
                ckt.State.iSolution[CCCSposNode] - ckt.State.iSolution[CCCSnegNode]);
            return current * voltage;
        }

        /// <summary>
        /// Private variables
        /// </summary>
        private MatrixElement CCCSposContBrptr = null;
        private MatrixElement CCCSnegContBrptr = null;
        private int CCCScontBranch = 0;
        private int CCCSposNode = 0;
        private int CCCSnegNode = 0;
        
        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override bool Setup(CircuitObject component, Circuit ckt)
        {
            var cccs = component as CurrentControlledCurrentsource;
            var matrix = ckt.State.Matrix;

            // Extract necessary info from the load behavior
            load = GetBehavior<CurrentControlledCurrentsourceLoadBehavior>(component);

            // Get nodes
            CCCSposNode = cccs.CCCSposNode;
            CCCSnegNode = cccs.CCCSnegNode;
            CCCScontBranch = cccs.CCCScontBranch;
            CCCSposContBrptr = matrix.GetElement(cccs.CCCSposNode, cccs.CCCScontBranch);
            CCCSnegContBrptr = matrix.GetElement(cccs.CCCSnegNode, cccs.CCCScontBranch);
            return true;
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            CCCSposContBrptr = null;
            CCCSnegContBrptr = null;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            CCCSposContBrptr.Add(load.CCCScoeff);
            CCCSnegContBrptr.Sub(load.CCCScoeff);
        }
    }
}
