using System;
using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;
using System.Linq;
using SpiceSharp.Sparse;
using SpiceSharp.Attributes;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Provides methods for checking the integrity of a circuit.
    /// </summary>
    public class Validator
    {
        /// <summary>
        /// Constants
        /// </summary>
        const double PivotAbsTol = 1e-6;
        const double PivotRelTol = 1e-3;

        /// <summary>
        /// Private variables
        /// </summary>
        bool HasSource;
        bool HasGround;
        List<Tuple<Component, int, int>> voltageDriven = new List<Tuple<Component, int, int>>();
        Dictionary<int, int> connectedGroups = new Dictionary<int, int>();
        int cgroup;
        
        /// <summary>
        /// Validate a circuit
        /// </summary>
        /// <param name="circuit">The circuit</param>
        public void Validate(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));

            // Connect all objects in the circuit, we need this information to find connectivity issues
            circuit.Objects.BuildOrderedComponentList();
            foreach (var o in circuit.Objects)
                o.Setup(circuit);

            // Initialize
            HasSource = false;
            voltageDriven.Clear();
            connectedGroups.Clear();
            connectedGroups.Add(0, 0);
            cgroup = 1;

            // Check all objects
            foreach (var c in circuit.Objects)
                CheckEntity(c);

            // Check if a voltage source is available
            if (!HasSource)
                throw new CircuitException("No independent source found");

            // Check if a circuit has ground
            if (!HasGround)
                throw new CircuitException("No ground found");

            // Check if a voltage driver is closing a loop
            var icc = FindVoltageDriveLoop();
            if (icc != null)
                throw new CircuitException("{0} closes a loop of voltage sources".FormatString(icc.Name));

            // Check for floating nodes
            var unconnected = FindFloatingateNodes();
            if (unconnected.Count > 0)
            {
                List<Identifier> un = new List<Identifier>();
                for (int i = 0; i < circuit.Nodes.Count; i++)
                {
                    int index = circuit.Nodes[i].Index;
                    if (unconnected.Contains(index))
                        un.Add(circuit.Nodes[i].Name);
                }
                throw new CircuitException("{0}: Floating nodes found".FormatString(string.Join(",", un)));
            }
        }

        /// <summary>
        /// Deal with a component
        /// </summary>
        /// <param name="c">The circuit object</param>
        void CheckEntity(Entity c)
        {
            // Circuit components
            if (c is Component icc)
            {
                // Check for ground node and for short-circuited components
                int n = -1;
                bool isShortcircuit = false;
                int[] nodes = new int[icc.PinCount];
                for (int i = 0; i < icc.PinCount; i++)
                {
                    var index = icc.GetNodeIndex(i);

                    // Check for a connection to ground
                    if (index == 0)
                        HasGround = true;

                    // Check for short-circuited devices
                    if (n < 0)
                    {
                        // We have at least one node, so we potentially have a short-circuited component
                        n = index;
                        isShortcircuit = true;
                    }
                    else if (n != index)
                    {
                        // Is not short-circuited, so OK!
                        isShortcircuit = false;
                    }

                    // Group indices
                    nodes[i] = index;
                    if (!connectedGroups.ContainsKey(index))
                        connectedGroups.Add(index, cgroup++);
                }
                if (isShortcircuit)
                    throw new CircuitException("{0}: All pins are short-circuited".FormatString(icc.Name));
                
                // Use attributes for checking properties
                var attributes = c.GetType().GetCustomAttributes(false);
                bool hasconnections = false;
                foreach (var attr in attributes)
                {
                    // Voltage driven nodes are checked for voltage loops
                    if (attr is VoltageDriverAttribute vd)
                        voltageDriven.Add(new Tuple<Component, int, int>(icc, nodes[vd.Positive], nodes[vd.Negative]));

                    // At least one source needs to be available
                    if (attr is IndependentSourceAttribute)
                        HasSource = true;

                    if (attr is ConnectedAttribute conn)
                    {
                        // Add connection between pins
                        if (conn.Pin1 >= 0 && conn.Pin2 >= 0)
                            AddConnections(new int[] { nodes[conn.Pin1], nodes[conn.Pin2] });
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
        Component FindVoltageDriveLoop()
        {
            // Remove the ground node and make a map for reducing the matrix complexity
            int index = 1;
            Dictionary<int, int> map = new Dictionary<int, int>();
            map.Add(0, 0);
            foreach (var vd in voltageDriven)
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
            Matrix<double> conn = new Matrix<double>(Math.Max(voltageDriven.Count, map.Count), false);
            for (int i = 0; i < voltageDriven.Count; i++)
            {
                var pins = voltageDriven[i];
                conn.GetElement(i + 1, map[pins.Item2]).Add(1.0);
                conn.GetElement(i + 1, map[pins.Item3]).Add(1.0);
            }
            var error = conn.OrderAndFactor(null, PivotRelTol, PivotAbsTol, true);
            conn.SingularAt(out int row, out _);
            row--;
            if (error == SparseError.Singular && row < voltageDriven.Count)
                return voltageDriven[row].Item1;
            return null;
        }

        /// <summary>
        /// Add connected nodes that will be used to find floating nodes
        /// </summary>
        /// <param name="nodes"></param>
        void AddConnections(int[] nodes)
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
        void AddConnection(int a, int b)
        {
            if (a == b)
                return;

            int groupa, groupb;
            bool hasa = connectedGroups.TryGetValue(a, out groupa);
            bool hasb = connectedGroups.TryGetValue(b, out groupb);

            if (hasa && hasb)
            {
                // Connect the two groups to that of the minimum group
                int newgroup = Math.Min(groupa, groupb);
                int oldgroup = Math.Max(groupa, groupb);
                int[] keys = connectedGroups.Keys.ToArray();
                foreach (var key in keys)
                {
                    if (connectedGroups[key] == oldgroup)
                        connectedGroups[key] = newgroup;
                }
            }
            else if (hasa)
                connectedGroups.Add(b, groupa);
            else if (hasb)
                connectedGroups.Add(a, groupb);
        }

        /// <summary>
        /// Find a node that has no path to ground anywhere (open-circuited)
        /// </summary>
        /// <returns></returns>
        HashSet<int> FindFloatingateNodes()
        {
            HashSet<int> unconnected = new HashSet<int>();

            foreach (var key in connectedGroups.Keys)
            {
                if (connectedGroups[key] != 0)
                    unconnected.Add(key);
            }

            return unconnected;
        }
    }
}
