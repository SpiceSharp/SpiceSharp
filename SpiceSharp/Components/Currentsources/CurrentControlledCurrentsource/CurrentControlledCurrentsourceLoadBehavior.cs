using System;
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
        /// Nodes
        /// </summary>
        protected int CCCScontBranch;
        protected int CCCSposNode;
        protected int CCCSnegNode;

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement CCCSposContBrptr = null;
        protected MatrixElement CCCSnegContBrptr = null;

        /// <summary>
        /// Create a getter
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="parameter">Parameter name</param>
        /// <returns></returns>
        public override Func<double> CreateGetter(Circuit ckt, string parameter)
        {
            switch (parameter)
            {
                case "i": return () => ckt.State.Solution[CCCScontBranch] * CCCScoeff;
                case "v": return () => ckt.State.Solution[CCCSposNode] - ckt.State.Solution[CCCSnegNode];
                case "p": return () =>
                    {
                        double v = ckt.State.Solution[CCCSposNode] - ckt.State.Solution[CCCSnegNode];
                        return ckt.State.Solution[CCCScontBranch] * CCCScoeff * v;
                    };
                default:
                    return base.CreateGetter(ckt, parameter);
            }
        }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <returns></returns>
        public override void Setup(CircuitObject component, Circuit ckt)
        {
            var cccs = component as CurrentControlledCurrentsource;
            var matrix = ckt.State.Matrix;

            // Get behaviors
            var vsrcload = GetBehavior<VoltagesourceLoadBehavior>(cccs.CCCScontSource);

            // Nodes
            CCCSposNode = cccs.CCCSposNode;
            CCCSnegNode = cccs.CCCSnegNode;
            CCCScontBranch = vsrcload.VSRCbranch;

            // Get matrix elements
            CCCSposContBrptr = matrix.GetElement(cccs.CCCSposNode, CCCScontBranch);
            CCCSnegContBrptr = matrix.GetElement(cccs.CCCSnegNode, CCCScontBranch);
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
