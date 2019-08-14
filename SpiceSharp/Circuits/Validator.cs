using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System.Reflection;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// A class that describes methods for validating a circuit.
    /// </summary>
    public class Validator
    {
        /// <summary>
        /// Defines a voltage driver.
        /// </summary>
        private class VoltageDriver
        {
            public Component Source;
            public int Node1;
            public int Node2;
        }

        /// <summary>
        /// Private variables
        /// </summary>
        private bool _hasSource;
        private bool _hasGround;
        private readonly List<VoltageDriver> _voltageDriven = new List<VoltageDriver>();
        private readonly Dictionary<int, int> _connectedGroups = new Dictionary<int, int>();
        private int _cgroup;
        private readonly VariableSet _nodes = new VariableSet();
        
        /// <summary>
        /// Validate a circuit.
        /// </summary>
        /// <param name="entities">The circuit to be validated.</param>
        public void Validate(EntityCollection entities)
        {
            entities.ThrowIfNull(nameof(entities));

            // Initialize
            _hasSource = false;
            _voltageDriven.Clear();
            _connectedGroups.Clear();
            _connectedGroups.Add(0, 0);
            _cgroup = 1;
            _nodes.Clear();

            // Check all entities
            foreach (var c in entities)
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
                var un = new List<string>();
                foreach (var n in _nodes)
                {
                    var index = n.Index;
                    if (unconnected.Contains(index))
                        un.Add(n.Name);
                }
                throw new CircuitException("{0}: Floating nodes found".FormatString(string.Join(",", un)));
            }
        }

        /// <summary>
        /// Perform checks on an entity.
        /// </summary>
        /// <param name="c">The entity to be checked.</param>
        private void CheckEntity(Entity c)
        {
            // Circuit components
            if (c is Component icc)
            {
                var i = 0;
                var nodes = new int[icc.PinCount];
                foreach (var index in icc.GetNodeIndexes(_nodes))
                {
                    // Group indices
                    nodes[i++] = index;
                    if (!_connectedGroups.ContainsKey(index))
                        _connectedGroups.Add(index, _cgroup++);
                }

                if (IsShortCircuited(icc))
                    throw new CircuitException("{0}: All pins are short-circuited".FormatString(icc.Name));
                
                // Use attributes for checking properties
                var attributes = c.GetType().GetTypeInfo().GetCustomAttributes(false);
                var hasconnections = false;
                foreach (var attr in attributes)
                {
                    // Voltage driven nodes are checked for voltage loops
                    switch (attr)
                    {
                        case VoltageDriverAttribute vd:
                            _voltageDriven.Add(new VoltageDriver {
                                Source = icc,
                                Node1 = nodes[vd.Positive],
                                Node2 = nodes[vd.Negative]
                            });
                            break;
                        case IndependentSourceAttribute _:
                            _hasSource = true;
                            break;
                        case ConnectedAttribute conn:
                            // Add connection between pins
                            if (conn.Pin1 >= 0 && conn.Pin2 >= 0)
                                AddConnections(new[] { nodes[conn.Pin1], nodes[conn.Pin2] });
                            hasconnections = true;
                            break;
                    }
                }

                // If the entities do not have connected pins specified, assume they're all connected
                if (!hasconnections)
                    AddConnections(nodes);
            }
        }

        /// <summary>
        /// Determines if all pins of the component are short-circuited together.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <returns>
        ///   <c>true</c> if all component pins are short-circuited; otherwise, <c>false</c>.
        /// </returns>
        private bool IsShortCircuited(Component component)
        {
            // Check for ground node and for short-circuited components
            var n = -1;
            var isShortcircuit = false;
            foreach (var index in component.GetNodeIndexes(_nodes))
            {
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
            }

            return isShortcircuit;
        }

        /// <summary>
        /// Find a voltage driver that closes a voltage drive loop.
        /// </summary>
        /// <returns>
        /// The component that closes the loop.
        /// </returns>
        private Component FindVoltageDriveLoop()
        {
            // Remove the ground node and make a map for reducing the matrix complexity
            var index = 1;
            var map = new Dictionary<int, int> {{0, 0}};
            foreach (var vd in _voltageDriven)
            {
                if (vd.Node1 != 0)
                {
                    if (!map.ContainsKey(vd.Node1))
                        map.Add(vd.Node1, index++);
                }
                if (vd.Node2 != 0)
                {
                    if (!map.ContainsKey(vd.Node2))
                        map.Add(vd.Node2, index++);
                }
            }

            // Determine the rank of the matrix
            var solver = new RealSolver(Math.Max(_voltageDriven.Count, map.Count));
            for (var i = 0; i < _voltageDriven.Count; i++)
            {
                var pins = _voltageDriven[i];
                solver.GetMatrixElement(i + 1, map[pins.Node1]).Value += 1.0;
                solver.GetMatrixElement(i + 1, map[pins.Node2]).Value += 1.0;
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
                    var indices = new LinearSystemIndices(exception.Index);
                    solver.InternalToExternal(indices);
                    return _voltageDriven[indices.Row - 1].Source;
                }
            }
            return null;
        }

        /// <summary>
        /// Add connected nodes that will be used to find floating nodes.
        /// </summary>
        /// <param name="nodes">The nodes that are connected together.</param>
        private void AddConnections(int[] nodes)
        {
            if (nodes == null || nodes.Length == 0)
                return;

            // All connections
            for (var i = 0; i < nodes.Length; i++)
            {
                for (var j = i + 1; j < nodes.Length; j++)
                    AddConnection(nodes[i], nodes[j]);
            }
        }

        /// <summary>
        /// Add a connection for checking for floating nodes.
        /// </summary>
        /// <param name="a">The first node index.</param>
        /// <param name="b">The second node index.</param>
        private void AddConnection(int a, int b)
        {
            if (a == b)
                return;

            var hasa = _connectedGroups.TryGetValue(a, out var groupa);
            var hasb = _connectedGroups.TryGetValue(b, out var groupb);

            if (hasa && hasb)
            {
                // Connect the two groups to that of the minimum group
                var newgroup = Math.Min(groupa, groupb);
                var oldgroup = Math.Max(groupa, groupb);
                var keys = _connectedGroups.Keys.ToArray();
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
        /// Find a node that has no path to ground anywhere (open-circuited).
        /// </summary>
        /// <returns>A set of node indices that has no DC path to ground.</returns>
        private HashSet<int> FindFloatingNodes()
        {
            var unconnected = new HashSet<int>();

            foreach (var key in _connectedGroups.Keys)
            {
                if (_connectedGroups[key] != 0)
                    unconnected.Add(key);
            }

            return unconnected;
        }
    }
}
