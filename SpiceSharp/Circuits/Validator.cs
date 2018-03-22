using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Provides methods for checking the integrity of a circuit.
    /// </summary>
    public class Validator
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private bool _hasSource;
        private bool _hasGround;
        private readonly List<Tuple<Component, int, int>> _voltageDriven = new List<Tuple<Component, int, int>>();
        private readonly Dictionary<int, int> _connectedGroups = new Dictionary<int, int>();
        private int _cgroup;
        private OP _op = new OP("temp");
        
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
                o.Setup(_op);

            // Initialize
            _hasSource = false;
            _voltageDriven.Clear();
            _connectedGroups.Clear();
            _connectedGroups.Add(0, 0);
            _cgroup = 1;

            // Check all objects
            foreach (var c in circuit.Objects)
                CheckEntity(c);

            // Check if a voltage source is available
            if (!_hasSource)
                throw new CircuitException("No independent source found");

            // Check if a circuit has ground
            if (!_hasGround)
                throw new CircuitException("No ground found");

            // Check if a voltage driver is closing a loop
            var icc = FindVoltageDriveLoop();
            if (icc != null)
                throw new CircuitException("{0} closes a loop of voltage sources".FormatString(icc.Name));

            // Check for floating nodes
            var unconnected = FindFloatingNodes();
            if (unconnected.Count > 0)
            {
                List<Identifier> un = new List<Identifier>();
                for (int i = 0; i < _op.Nodes.Count; i++)
                {
                    int index = _op.Nodes[i].Index;
                    if (unconnected.Contains(index))
                        un.Add(_op.Nodes[i].Name);
                }
                throw new CircuitException("{0}: Floating nodes found".FormatString(string.Join(",", un)));
            }
        }

        /// <summary>
        /// Deal with a component
        /// </summary>
        /// <param name="c">The circuit object</param>
        private void CheckEntity(Entity c)
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
                        _hasGround = true;

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
                    if (!_connectedGroups.ContainsKey(index))
                        _connectedGroups.Add(index, _cgroup++);
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
                        _voltageDriven.Add(new Tuple<Component, int, int>(icc, nodes[vd.Positive], nodes[vd.Negative]));

                    // At least one source needs to be available
                    if (attr is IndependentSourceAttribute)
                        _hasSource = true;

                    if (attr is ConnectedAttribute conn)
                    {
                        // Add connection between pins
                        if (conn.Pin1 >= 0 && conn.Pin2 >= 0)
                            AddConnections(new[] { nodes[conn.Pin1], nodes[conn.Pin2] });
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
        private Component FindVoltageDriveLoop()
        {
            // Remove the ground node and make a map for reducing the matrix complexity
            int index = 1;
            Dictionary<int, int> map = new Dictionary<int, int> {{0, 0}};
            foreach (var vd in _voltageDriven)
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
            RealSolver solver = new RealSolver(Math.Max(_voltageDriven.Count, map.Count));
            for (int i = 0; i < _voltageDriven.Count; i++)
            {
                var pins = _voltageDriven[i];
                solver.GetMatrixElement(i + 1, map[pins.Item2]).Value += 1.0;
                solver.GetMatrixElement(i + 1, map[pins.Item3]).Value += 1.0;
            }
            try
            {
                // Try refactoring the matrix
                solver.OrderAndFactor();
            }
            catch (SingularException exception)
            {
                /*
                 * If the rank of the matrix is lower than the number of driven nodes, then
                 * the matrix is not solvable for those nodes. This means that there are
                 * voltage sources driving nodes in such a way that they cannot be solved.
                 */
                if (exception.Index <= _voltageDriven.Count)
                {
                    var indices = solver.InternalToExternal(new Tuple<int, int>(exception.Index, exception.Index));
                    return _voltageDriven[indices.Item1 - 1].Item1;
                }
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

            bool hasa = _connectedGroups.TryGetValue(a, out var groupa);
            bool hasb = _connectedGroups.TryGetValue(b, out var groupb);

            if (hasa && hasb)
            {
                // Connect the two groups to that of the minimum group
                int newgroup = Math.Min(groupa, groupb);
                int oldgroup = Math.Max(groupa, groupb);
                int[] keys = _connectedGroups.Keys.ToArray();
                foreach (var key in keys)
                {
                    if (_connectedGroups[key] == oldgroup)
                        _connectedGroups[key] = newgroup;
                }
            }
            else if (hasa)
                _connectedGroups.Add(b, groupa);
            else if (hasb)
                _connectedGroups.Add(a, groupb);
        }

        /// <summary>
        /// Find a node that has no path to ground anywhere (open-circuited)
        /// </summary>
        /// <returns></returns>
        private HashSet<int> FindFloatingNodes()
        {
            HashSet<int> unconnected = new HashSet<int>();

            foreach (var key in _connectedGroups.Keys)
            {
                if (_connectedGroups[key] != 0)
                    unconnected.Add(key);
            }

            return unconnected;
        }
    }
}
