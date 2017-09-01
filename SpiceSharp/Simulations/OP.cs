using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Circuits;
using static SpiceSharp.Simulations.SimulationIterate;

namespace SpiceSharp.Simulations
{
    public class OP : Simulation<OP>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the simulation</param>
        /// <param name="config">Configuration</param>
        public OP(string name) : base(name)
        {
        }

        /// <summary>
        /// Execute the DC simulation
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <param name="reset">Restart the circuit if true</param>
        public override void Execute(Circuit ckt)
        {
            // Setup the state
            var state = ckt.State;
            var rstate = state.Real;
            state.UseIC = false; // UseIC is only used in transient simulations
            state.UseDC = true;
            state.UseSmallSignal = false;
            state.Domain = CircuitState.DomainTypes.None;
            state.Gmin = Config.Gmin;

            Initialize(ckt);
            Op(Config, ckt, CurrentConfig.DcMaxIterations);
            Export(ckt);
            Finalize(ckt);
        }
    }
}
