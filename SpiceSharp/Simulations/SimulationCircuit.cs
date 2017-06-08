using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Circuits;
using MathNet.Numerics.LinearAlgebra;

namespace SpiceSharp.Simulations
{
    public static class SimulationCircuit
    {
        /// <summary>
        /// Load the circuit for simulation
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public static void Load(this Circuit ckt)
        {
            var state = ckt.State;
            var nodes = ckt.Nodes;

            // Start the stopwatch
            ckt.Statistics.LoadTime.Start();

            // Clear rhs and matrix
            ckt.State.Clear();

            // Load all devices
            // ckt.Load(this, state);
            foreach (var c in ckt.Components)
                c.Load(ckt);

            // Check modes
            if (state.IsDc)
            {
                // Consider doing nodeset & ic assignments
                if ((state.Init & (CircuitState.InitFlags.InitJct | CircuitState.InitFlags.InitFix)) != 0)
                {
                    // Do nodesets
                    for (int i = 0; i < ckt.Nodes.Count; i++)
                    {
                        var node = ckt.Nodes[i];
                        if (nodes.Nodeset.ContainsKey(node.Name))
                        {
                            double ns = nodes.Nodeset[node.Name];
                            if (ZeroNoncurRow(state.Matrix, nodes, node.Index))
                            {
                                state.Rhs[node.Index] = 1.0e10 * ns;
                                state.Matrix[node.Index, node.Index] = 1.0e10;
                            }
                            else
                            {
                                state.Rhs[node.Index] = ns;
                                state.Solution[node.Index] = ns;
                                state.Matrix[node.Index, node.Index] = 1.0;
                            }
                        }
                    }
                }

                if (state.UseIC)
                {
                    for (int i = 0; i < ckt.Nodes.Count; i++)
                    {
                        var node = ckt.Nodes[i];
                        if (ckt.Nodes.IC.ContainsKey(node.Name))
                        {
                            double ic = ckt.Nodes.IC[node.Name];
                            if (ZeroNoncurRow(ckt.State.Matrix, ckt.Nodes, node.Index))
                            {
                                ckt.State.Rhs[node.Index] = 1.0e10 * ic;
                                ckt.State.Matrix[node.Index, node.Index] = 1.0e10;
                            }
                            else
                            {
                                ckt.State.Rhs[node.Index] = ic;
                                ckt.State.Solution[node.Index] = ic;
                                ckt.State.Matrix[node.Index, node.Index] = 1.0;
                            }
                        }
                    }
                }
            }

            // Keep statistics
            ckt.Statistics.LoadTime.Stop();
        }

        /// <summary>
        /// Set the initial conditions
        /// </summary>
        /// <param name="ckt"></param>
        public static void Ic(this Circuit ckt)
        {
            var state = ckt.State;
            var nodes = ckt.Nodes;

            // Clear the current solution
            state.Solution.Clear();

            // Go over all nodes
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (nodes.Nodeset.ContainsKey(node.Name))
                {
                    state.HadNodeset = true;
                    state.Solution[node.Index] = nodes.Nodeset[node.Name];
                }
                if (nodes.IC.ContainsKey(node.Name))
                {
                    state.Solution[node.Index] = nodes.IC[node.Name];
                }
            }

            // Use initial conditions
            if (state.UseIC)
            {
                foreach (var c in ckt.Components)
                    c.SetIc(ckt);
            }
        }

        /// <summary>
        /// Reset the row to 0.0 and return true if the row is a current equation
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="nodes">The list of nodes</param>
        /// <param name="rownum">The row number</param>
        /// <returns></returns>
        private static bool ZeroNoncurRow(Matrix<double> matrix, Nodes nodes, int rownum)
        {
            bool currents = false;
            for (int n = 0; n < nodes.Count; n++)
            {
                var node = nodes[n];
                double x = matrix[rownum, node.Index];
                if (x != 0.0)
                {
                    if (node.Type == CircuitNode.NodeType.Current)
                        currents = true;
                    else
                        matrix[rownum, node.Index] = 0.0;
                }
            }
            return currents;
        }
    }
}
