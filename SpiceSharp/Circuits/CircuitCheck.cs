using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Provides methods for checking the integrity of a circuit.
    /// </summary>
    public class CircuitCheck
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private bool HasSource = false;
        private List<Tuple<ICircuitComponent, int, int>> voltagedriven = new List<Tuple<ICircuitComponent, int, int>>();
        private Dictionary<int, HashSet<int>> connections = new Dictionary<int, HashSet<int>>();
        private HashSet<int> unconnected = new HashSet<int>();

        /// <summary>
        /// Constructor
        /// </summary>
        public CircuitCheck()
        {
        }

        /// <summary>
        /// Check a circuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public void Check(Circuit ckt)
        {
            // Make sure the circuit is set up
            // We need this to access all the circuit component nodes
            ckt.Setup();

            // Initialize
            HasSource = false;
            voltagedriven.Clear();
            unconnected.Clear();
            connections.Clear();

            // Check all objects
            foreach (var c in ckt.Objects)
                CheckObject(c);

            // Check if a voltage source is available
            if (!HasSource)
                throw new CircuitException("No independent source found");

            // Check if a voltage driver is closing a loop
            var icc = FindVoltageDriveLoop();
            if (icc != null)
                throw new CircuitException($"{string.Join(".", ckt.Objects.FindPath(icc))} closes a loop of voltage sources");

            // Check for floating nodes
            if (FindFloatingNodes() > 0)
            {
                List<string> un = new List<string>();
                for (int i = 0; i < ckt.Nodes.Count; i++)
                {
                    int index = ckt.Nodes[i].Index;
                    if (unconnected.Contains(index))
                        un.Add(ckt.Nodes[i].Name);
                }
                throw new CircuitException($"{string.Join(", ", un)}: Floating nodes found");
            }
        }

        /// <summary>
        /// Deal with a component
        /// </summary>
        /// <param name="c">The circuit object</param>
        private void CheckObject(ICircuitObject c)
        {
            // Subcircuits
            if (c is Subcircuit)
            {
                Subcircuit subckt = (Subcircuit)c;
                foreach (var sc in subckt.Objects)
                    CheckObject(sc);
                return;
            }

            // Circuit components
            if (c is ICircuitComponent)
            {
                var icc = (ICircuitComponent)c;

                // Check for short-circuited components
                int n = -1;
                bool sc = true;
                for (int i = 0; i < icc.PinCount; i++)
                {
                    if (n < 0)
                        n = icc.GetNodeIndex(i);
                    else if (n != icc.GetNodeIndex(i))
                    {
                        sc = false;
                        break;
                    }
                }
                if (sc)
                    throw new CircuitException($"{icc.Name}: All pins have been short-circuited");

                // Get the node indices for each pin
                int[] nodes = new int[icc.PinCount];
                for (int i = 0; i < nodes.Length; i++)
                {
                    nodes[i] = icc.GetNodeIndex(i);
                    unconnected.Add(nodes[i]);
                }

                // Use attributes for checking properties
                var attributes = c.GetType().GetCustomAttributes(false);
                bool hasconnections = false;
                foreach (var attr in attributes)
                {
                    // Voltage driven nodes are checked for voltage loops
                    if (attr is VoltageDriver)
                    {
                        VoltageDriver vd = (VoltageDriver)attr;
                        voltagedriven.Add(new Tuple<ICircuitComponent, int, int>(icc, nodes[vd.Positive], nodes[vd.Negative]));
                    }

                    // At least one source needs to be available
                    if (attr is IndependentSource)
                        HasSource = true;

                    if (attr is ConnectedPins)
                    {
                        ConnectedPins conn = (ConnectedPins)attr;
                        int[] tmp = new int[conn.Pins.Length];
                        for (int i = 0; i < conn.Pins.Length; i++)
                            tmp[i] = nodes[conn.Pins[i]];
                        AddConnections(tmp);
                        hasconnections = true;
                    }
                }

                // Check connections
                if (!hasconnections)
                    AddConnections(nodes);
            }
        }

        /// <summary>
        /// Find a voltage driver that closes a voltage drive loop
        /// </summary>
        private ICircuitComponent FindVoltageDriveLoop()
        {
            // Remove the ground node and make a map for reducing the matrix complexity
            int index = 0;
            Dictionary<int, int> map = new Dictionary<int, int>();
            foreach (var vd in voltagedriven)
            {
                if (vd.Item2 != 0)
                {
                    if (!map.ContainsKey(vd.Item2))
                        map.Add(vd.Item2, index++);
                }
                if (vd.Item3 != 0)
                {
                    if (!map.ContainsKey(vd.Item3))
                        map.Add(vd.Item3, index++);
                }
            }

            // Build the connection matrix
            Matrix<double> conn = new SparseMatrix(Math.Max(voltagedriven.Count, map.Count));
            for (int i = 0; i < voltagedriven.Count; i++)
            {
                var pins = voltagedriven[i];
                if (pins.Item2 != 0)
                    conn[i, map[pins.Item2]] = 1.0;
                if (pins.Item3 != 0)
                    conn[i, map[pins.Item3]] = -1.0;
            }

            // We just built a matrix that, if correct, is able to generate one specific voltage for each node it applied to
            // Any rank that is smaller will be caused by voltage sources that are not independent = a voltage loop
            // We can use the LU decomposition to find out which voltage source closes a voltage source loop
            string orig = conn.ToString();
            LU<double> result = conn.LU();
            string u = result.U.ToString();
            for (int i = 0; i < voltagedriven.Count; i++)
            {
                int k = result.P[i];
                if (result.U[k, k] == 0)
                    return voltagedriven[i].Item1;
            }
            return null;
        }

        /// <summary>
        /// Add connected nodes that will be used to find floating nodes
        /// </summary>
        /// <param name="nodes"></param>
        private void AddConnections(int[] nodes)
        {
            if (nodes == null || nodes.Length == 0)
                return;

            // Find the minimum value
            int min = nodes[0];
            for (int i = 1; i < nodes.Length; i++)
                min = nodes[i] < min ? nodes[i] : min;

            if (!connections.ContainsKey(min))
                connections.Add(min, new HashSet<int>());

            // Add the connections
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] != min)
                    connections[min].Add(nodes[i]);
            }
        }

        /// <summary>
        /// Find a node that has no path to ground anywhere (open-circuited)
        /// </summary>
        /// <returns></returns>
        private int FindFloatingNodes()
        {
            // We start from zero and we'll see how many nodes we can eliminate
            Stack<int> todo = new Stack<int>();
            todo.Push(0);
            while (todo.Count > 0)
            {
                int c = todo.Pop();

                // Remove the node from the unconnected nodes
                unconnected.Remove(c);

                // If it maps to other nodes, try to remove them too
                if (connections.ContainsKey(c))
                {
                    // Add all these nodes to the stack
                    foreach (var a in connections[c])
                        todo.Push(a);
                }
            }

            // The list that remains are all unconnected nodes
            return unconnected.Count;
        }
    }
}
