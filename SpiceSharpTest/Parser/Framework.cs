using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using SpiceSharp.Parser;
using SpiceSharp.Parameters;
using SpiceSharp.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.Diagnostics;

namespace SpiceSharpTest.Parser
{
    /// <summary>
    /// A nice framework
    /// </summary>
    public class Framework
    {
        /// <summary>
        /// Run a netlist using the standard parser
        /// </summary>
        /// <param name="netlist">The netlist string</param>
        /// <returns></returns>
        protected Netlist Run(string netlist)
        {
            MemoryStream m = new MemoryStream(Encoding.UTF8.GetBytes(netlist));

            // Create the parser and run it
            NetlistReader r = new NetlistReader();
            r.Parse(m);

            // Return the generated netlist
            return r.Netlist;
        }

        /// <summary>
        /// Perform the setup and temperature calculations
        /// This is often needed to calculate the default parameters
        /// </summary>
        /// <param name="n"></param>
        protected void Initialize(Netlist n)
        {
            n.Circuit.Setup();
            foreach (var c in n.Circuit.Objects)
                c.Temperature(n.Circuit);
        }

        /// <summary>
        /// Test a circuit object for all its parameters
        /// </summary>
        /// <param name="n">The netlist</param>
        /// <param name="name">The name of the object</param>
        /// <param name="names">The parameter names</param>
        /// <param name="values">The parameter values</param>
        /// <param name="nodes">The nodes (optional)</param>
        /// <returns></returns>
        protected T Test<T>(Netlist n, string name, string[] names = null, double[] values = null, string[] nodes = null)
        {
            ICircuitObject obj = n.Circuit.Objects[name.ToLower()];
            Assert.AreEqual(typeof(T), obj.GetType());

            // Test all parameters
            if (names != null)
                TestParameters((IParameterized)obj, names, values);

            // Test all nodes
            if (nodes != null)
                TestNodes((ICircuitComponent)obj, nodes);

            // Make sure there are no warnings
            if (CircuitWarning.Warnings.Count > 0)
                throw new Exception("Warning: " + CircuitWarning.Warnings[0]);
            return (T)obj;
        }

        /// <summary>
        /// Test the parameters of a parameterized object
        /// </summary>
        /// <param name="obj">The parameterized object</param>
        /// <param name="names">The parameter names</param>
        /// <param name="values">The expected parameter values</param>
        protected void TestParameters(IParameterized p, string[] names, double[] values)
        {
            if (names.Length != values.Length)
                throw new Exception("Unit test error: parameter name array does not match the value array");

            // Test all parameters
            for (int i = 0; i < names.Length; i++)
            {
                double expected = values[i];
                double actual = p.Ask(names[i]);
                double tol = Math.Min(Math.Abs(expected), Math.Abs(actual)) * 1e-6;
                Assert.AreEqual(expected, actual, tol);
            }
        }

        /// <summary>
        /// Test the nodes of a circuitcomponent
        /// </summary>
        /// <param name="c">The component</param>
        /// <param name="nodes">The expected node names</param>
        protected void TestNodes(ICircuitComponent c, string[] nodes)
        {
            // Test all nodes
            for (int i = 0; i < nodes.Length; i++)
            {
                string expected = nodes[i].ToLower();
                string actual = c.GetNode(i);
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
