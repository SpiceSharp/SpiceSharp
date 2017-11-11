using System;
using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;
using System.Linq;
using SpiceSharp.Sparse;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Provides methods for checking the integrity of a circuit.
    /// </summary>
    public class CircuitCheck
    {
        /// <summary>
        /// Constants
        /// </summary>
        private const double PivotAbsTol = 1e-6;
        private const double PivotRelTol = 1e-3;

        /// <summary>
        /// Private variables
        /// </summary>
        private bool HasSource = false;
        private bool HasGround = false;
        private List<Tuple<ICircuitComponent, int, int>> voltagedriven = new List<Tuple<ICircuitComponent, int, int>>();
        private Dictionary<int, int> connectedgroups = new Dictionary<int, int>();
        private int cgroup = 0;

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
            connectedgroups.Clear();
            connectedgroups.Add(0, 0);
            cgroup = 1;

            // Check all objects
            foreach (var c in ckt.Objects)
                CheckObject(c);

            // Check if a voltage source is available
            if (!HasSource)
                throw new CircuitException("No independent source found");

            // Check if a circuit has ground
            if (!HasGround)
                throw new CircuitException("No ground found");

            // Check if a voltage driver is closing a loop
            var icc = FindVoltageDriveLoop();
            if (icc != null)
                throw new CircuitException($"{icc.Name} closes a loop of voltage sources");

            // Check for floating nodes
            var unconnected = FindFloatingNodes();
            if (unconnected.Count > 0)
            {
                List<CircuitIdentifier> un = new List<CircuitIdentifier>();
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
            // Circuit components
            if (c is ICircuitComponent icc)
            {
                //Check for ground node
                for (int i = 0; i < icc.PinCount; i++)
                {
                    var id = icc.GetNode(i);
                    if (id.Name == "0" || id.Name == "gnd")
                    {
                        HasGround = true;
                    }
                }

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
                    if (!connectedgroups.ContainsKey(nodes[i]))
                        connectedgroups.Add(nodes[i], cgroup++);
                }

                // Use attributes for checking properties
                var attributes = c.GetType().GetCustomAttributes(false);
                bool hasconnections = false;
                foreach (var attr in attributes)
                {
                    // Voltage driven nodes are checked for voltage loops
                    if (attr is VoltageDriver vd)
                        voltagedriven.Add(new Tuple<ICircuitComponent, int, int>(icc, nodes[vd.Positive], nodes[vd.Negative]));

                    // At least one source needs to be available
                    if (attr is IndependentSource)
                        HasSource = true;

                    if (attr is ConnectedPins conn)
                    {
                        int[] tmp = new int[conn.Pins.Length];
                        for (int i = 0; i < conn.Pins.Length; i++)
                            tmp[i] = nodes[conn.Pins[i]];
                        AddConnections(tmp);
                        hasconnections = true;
                    }
                }

                // If the object does not have connected pins specified, assume they're all connected
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
            int index = 1;
            Dictionary<int, int> map = new Dictionary<int, int>();
            map.Add(0, 0);
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

            // Determine the rank of the matrix
            Matrix conn = new Matrix(Math.Max(voltagedriven.Count, map.Count), false);
            for (int i = 0; i < voltagedriven.Count; i++)
            {
                var pins = voltagedriven[i];
                conn.GetElement(i + 1, map[pins.Item2]).Add(1.0);
                conn.GetElement(i + 1, map[pins.Item3]).Add(1.0);
            }
            var error = conn.OrderAndFactor(null, PivotRelTol, PivotAbsTol, true);
            conn.SingularAt(out int row, out _);
            row--;
            if (error == SparseError.Singular && row < voltagedriven.Count)
                return voltagedriven[row].Item1;
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

            // All connections
            for (int i = 0; i < nodes.Length; i++)
            {
                for (int j = i + 1; j < nodes.Length; j++)
                    AddConnection(nodes[i], nodes[j]);
            }
        }

        /// <summary>
        /// Add a connection for checking for floating nodes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        private void AddConnection(int a, int b)
        {
            if (a == b)
                return;

            int groupa, groupb;
            bool hasa = connectedgroups.TryGetValue(a, out groupa);
            bool hasb = connectedgroups.TryGetValue(b, out groupb);

            if (hasa && hasb)
            {
                // Connect the two groups to that of the minimum group
                int newgroup = Math.Min(groupa, groupb);
                int oldgroup = Math.Max(groupa, groupb);
                int[] keys = connectedgroups.Keys.ToArray();
                foreach (var key in keys)
                {
                    if (connectedgroups[key] == oldgroup)
                        connectedgroups[key] = newgroup;
                }
            }
            else if (hasa)
                connectedgroups.Add(b, groupa);
            else if (hasb)
                connectedgroups.Add(a, groupb);
        }

        /// <summary>
        /// Find a node that has no path to ground anywhere (open-circuited)
        /// </summary>
        /// <returns></returns>
        private HashSet<int> FindFloatingNodes()
        {
            HashSet<int> unconnected = new HashSet<int>();

            foreach (var key in connectedgroups.Keys)
            {
                if (connectedgroups[key] != 0)
                    unconnected.Add(key);
            }

            return unconnected;
        }
    }
}
