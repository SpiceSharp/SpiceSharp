using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;
using SpiceSharp.Sparse;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Provides static methods for basic simulations involving the circuit
    /// </summary>
    public static class SimulationExtensions
    {
        /// <summary>
        /// Set the initial conditions
        /// </summary>
        /// <param name="ckt"></param>
        public static void Ic(this Simulation simulation, List<CircuitObjectBehaviorIc> icbehaviors)
        {
            var ckt = simulation.Circuit;
            var state = ckt.State;
            var rstate = state;
            var nodes = ckt.Nodes;

            // Clear the current solution
            for (int i = 0; i < rstate.Solution.Length; i++)
            {
                rstate.Rhs[i] = 0.0;
            }

            // Go over all nodes
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (nodes.Nodeset.ContainsKey(node.Name))
                {
                    node.Diagonal = rstate.Matrix.GetElement(node.Index, node.Index);
                    state.HadNodeset = true;
                    rstate.Rhs[node.Index] = nodes.Nodeset[node.Name];
                }
                if (nodes.IC.ContainsKey(node.Name))
                {
                    node.Diagonal = rstate.Matrix.GetElement(node.Index, node.Index);
                    rstate.Rhs[node.Index] = nodes.IC[node.Name];
                }
            }

            // Use initial conditions
            if (state.UseIC)
            {
                foreach (var behavior in icbehaviors)
                    behavior.SetIc(ckt);
            }
        }
    }
}
