using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Parameters;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class describes a current-controlled current source
    /// </summary>
    public class CurrentControlledCurrentsource : CircuitComponent
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("gain"), SpiceInfo("Gain of the source")]
        public Parameter<double> CCCScoeff { get; } = new Parameter<double>();
        [SpiceName("control"), SpiceInfo("Name of the controlling source")]
        public string CCCScontName { get; set; }
        [SpiceName("i"), SpiceInfo("CCCS output current")]
        public double GetCurrent(Circuit ckt) => ckt.State.Real.Solution[CCCScontBranch] * CCCScoeff;
        [SpiceName("v"), SpiceInfo("CCCS voltage at output")]
        public double GetVoltage(Circuit ckt) => ckt.State.Real.Solution[CCCSposNode] - ckt.State.Real.Solution[CCCSnegNode];
        [SpiceName("p"), SpiceInfo("CCCS power")]
        public double GetPower(Circuit ckt) => ckt.State.Real.Solution[CCCScontBranch] * CCCScoeff *
            (ckt.State.Real.Solution[CCCSposNode] - ckt.State.Real.Solution[CCCSnegNode]);

        /// <summary>
        /// Nodes
        /// </summary>
        [SpiceName("pos_node"), SpiceInfo("Positive node of the source")]
        public int CCCSposNode { get; private set; }
        [SpiceName("neg_node"), SpiceInfo("Negative node of the source")]
        public int CCCSnegNode { get; private set; }
        private int CCCScontBranch;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current controlled current source</param>
        public CurrentControlledCurrentsource(string name) : base(name, 2)
        {
            // Make sure the current controlled current source happens after voltage sources
            Priority = -1;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the current controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="vsource">The name of the voltage source</param>
        /// <param name="gain">The current gain</param>
        public CurrentControlledCurrentsource(string name, string pos, string neg, string vsource, double gain) : base(name, 2)
        {
            Connect(pos, neg);
            CCCScoeff.Set(gain);
            CCCScontName = vsource;
        }

        /// <summary>
        /// No model
        /// </summary>
        /// <returns>Returns null</returns>
        public override CircuitModel GetModel() => null;

        /// <summary>
        /// Setup the current controlled current source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            CCCSposNode = nodes[0].Index;
            CCCSnegNode = nodes[1].Index;

            // Find the voltage source for which the current is being measured
            var vsrc = ckt.Components[CCCScontName];
            if (vsrc is Voltagesource)
                CCCScontBranch = ((Voltagesource)vsrc).VSRCbranch;
            else
                throw new CircuitException($"{Name}: Could not find voltage source '{CCCScontName}'");
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
        }

        /// <summary>
        /// Load the current-controlled current source
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Load(Circuit ckt)
        {
            var rstate = ckt.State.Real;
            rstate.Matrix[CCCSposNode, CCCScontBranch] += CCCScoeff.Value;
            rstate.Matrix[CCCSnegNode, CCCScontBranch] -= CCCScoeff.Value;
        }

        /// <summary>
        /// Load the current-controlled current source for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var cstate = ckt.State.Complex;
            cstate.Matrix[CCCSposNode, CCCScontBranch] += CCCScoeff.Value;
            cstate.Matrix[CCCSnegNode, CCCScontBranch] -= CCCScoeff.Value;
        }
    }
}
