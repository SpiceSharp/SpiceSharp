using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
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
        private int CurrentEquations = 0;

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
            CurrentEquations = 0;

            // Check all objects
            foreach (var c in ckt.Objects)
                CheckObject(c);

            // Check all nodes
            for (int i = 0; i < ckt.Nodes.Count; i++)
            {
                CircuitNode node = ckt.Nodes[i];
                if (node.Type == CircuitNode.NodeType.Current)
                    CurrentEquations++;
            }
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
                // Detect independent sources
                if (c is Voltagesource || c is Currentsource)
                    HasSource = true;
            }
        }

        /// <summary>
        /// Check the matrix for any problems
        /// </summary>
        /// <param name="yn">Yn-matrix</param>
        /// <param name="vn">Vn-matrix</param>
        private void CheckMatrix(Circuit ckt)
        {
            Matrix<double> yn = ckt.State.Real.Matrix;
            Vector<double> vn = ckt.State.Real.Rhs;
            Dictionary<int, int> nmap = new Dictionary<int, int>();
            SparseMatrix conn = new SparseMatrix(CurrentEquations, yn.ColumnCount);
            int index = 0;

            // Build a list of current-equations, these should not be dependent
            for (int i = 0; i < ckt.Nodes.Count; i++)
            {
                CircuitNode node = ckt.Nodes[i];
                if (node.Type == CircuitNode.NodeType.Current)
                {
                    nmap.Add(index, i);

                    // Copy the row of the matrix
                    conn.SetSubMatrix(index, node.Index, 1, 0, 0, conn.ColumnCount, ckt.State.Real.Matrix);
                    index++;
                }
            }
        }
    }
}
