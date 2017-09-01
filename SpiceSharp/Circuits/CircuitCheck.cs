using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Circuits
{
    public class CircuitCheck
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private bool HasSource = false;
        private List<Tuple<ICircuitComponent, int, int>> voltagedriven = new List<Tuple<ICircuitComponent, int, int>>();

        /// <summary>
        /// Constructor
        /// </summary>
        public CircuitCheck()
        {
        }

        /// <summary>
        /// Check the circuit
        /// </summary>
        /// <param name="ckt"></param>
        public void Check(Circuit ckt)
        {
            // Make sure the circuit is set up
            ckt.Setup();

            // Initialize
            HasSource = false;
            voltagedriven.Clear();

            // Check all objects
            foreach (var c in ckt.Objects)
                CheckObject(c);

            // Check if a voltage source is available
            if (!HasSource)
                throw new CircuitException("No independent source found");

            // Check if a voltage driver is closing a loop
            var icc = FindVoltageDriveLoop();
            if (icc != null)
                throw new CircuitException($"{icc.Name} closes a loop of voltage sources");
        }

        /// <summary>
        /// Deal with a component
        /// </summary>
        /// <param name="c"></param>
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

                // Use attributes for checking properties
                var attributes = c.GetType().GetCustomAttributes(false);
                foreach (var attr in attributes)
                {
                    // Voltage driven nodes are checked for voltage loops
                    if (attr is VoltageDriver)
                    {
                        VoltageDriver vd = (VoltageDriver)attr;
                        int pos = icc.GetNodeIndex(vd.Positive);
                        int neg = icc.GetNodeIndex(vd.Negative);
                        voltagedriven.Add(new Tuple<ICircuitComponent, int, int>(icc, pos, neg));
                    }

                    // At least one source needs to be available
                    if (attr is IndependentSource)
                        HasSource = true;
                }
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
                if (vd.Item2 != 0)
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
            LU<double> result = conn.LU();
            for (int i = 0; i < result.U.RowCount; i++)
            {
                if (result.U[i, i] == 0)
                    return voltagedriven[result.P[i]].Item1;
            }
            return null;
        }
    }
}
