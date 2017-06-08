using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A simulation that executes a transient analysis
    /// </summary>
    public class Transient : Simulation
    {
        /// <summary>
        /// Iterate for transient simulations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <param name="state">The current iteration state</param>
        /// <param name="iterlim">The maximum number of iterations</param>
        /// <returns></returns>
        protected bool Iterate(Circuit ckt, int maxiter)
        {
            // Ignore operating condition point, just use the solution as-is
            if (ckt.State.UseIC)
            {
                // Store the current solution
                ckt.State.StoreSolution();
                ckt.Load();
                return true;
            }

            // Do the base iteration
            return SimulationIterate.Iterate(this, ckt, maxiter);
        }
    }
}
