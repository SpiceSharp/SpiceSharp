using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Behavior for a <see cref="CurrentControlledCurrentsource"/>
    /// </summary>
    public class CurrentControlledCurrentsourceLoadBehavior : CircuitObjectBehaviorLoad
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("gain"), SpiceInfo("Gain of the source")]
        public Parameter CCCScoeff { get; } = new Parameter();

        [SpiceName("i"), SpiceInfo("CCCS output current")]
        public double GetCurrent(Circuit ckt) => ckt.State.Solution[CCCScontBranch] * CCCScoeff;
        [SpiceName("v"), SpiceInfo("CCCS voltage at output")]
        public double GetVoltage(Circuit ckt) => ckt.State.Solution[CCCSposNode] - ckt.State.Solution[CCCSnegNode];
        [SpiceName("p"), SpiceInfo("CCCS power")]
        public double GetPower(Circuit ckt) => ckt.State.Solution[CCCScontBranch] * CCCScoeff *
            (ckt.State.Solution[CCCSposNode] - ckt.State.Solution[CCCSnegNode]);

        /// <summary>
        /// Private variables
        /// </summary>
        private MatrixElement CCCSposContBrptr = null;
        private MatrixElement CCCSnegContBrptr = null;
        private int CCCScontBranch;
        private int CCCSposNode;
        private int CCCSnegNode;

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var cccs = component as CurrentControlledCurrentsource;
            var matrix = ckt.State.Matrix;

            CCCSposNode = cccs.CCCSposNode;
            CCCSnegNode = cccs.CCCSnegNode;
            CCCScontBranch = cccs.CCCScontBranch;
            CCCSposContBrptr = matrix.GetElement(cccs.CCCSposNode, cccs.CCCScontBranch);
            CCCSnegContBrptr = matrix.GetElement(cccs.CCCSnegNode, cccs.CCCScontBranch);
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Load(Circuit ckt)
        {
            CCCSposContBrptr.Add(CCCScoeff.Value);
            CCCSnegContBrptr.Sub(CCCScoeff.Value);
        }
    }
}
