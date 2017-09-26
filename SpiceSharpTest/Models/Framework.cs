using System;
using System.IO;
using System.Text;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharp.Components;
using SpiceSharp.Parser.Readers;
using MathNet.Numerics.Interpolation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SpiceSharpTest.Models
{
    /// <summary>
    /// Framework for testing models
    /// </summary>
    public class Framework
    {
        /// <summary>
        /// Run a netlist using the standard parser
        /// </summary>
        /// <param name="lines">The netlist to parse</param>
        /// <returns></returns>
        protected Netlist Run(params string[] lines)
        {
            string netlist = string.Join(Environment.NewLine, lines);
            MemoryStream m = new MemoryStream(Encoding.UTF8.GetBytes(netlist));

            // Create the parser and run it
            NetlistReader r = new NetlistReader();

            // Add our BSIM transistor models
            var mosfets = r.Netlist.Readers[StatementType.Component].Find<MosfetReader>().Mosfets;
            BSIMParser.AddMosfetGenerators(mosfets);
            var levels = r.Netlist.Readers[StatementType.Model].Find<MosfetModelReader>().Levels;
            BSIMParser.AddMosfetModelGenerators(levels);
            r.Parse(m);

            // Return the generated netlist
            return r.Netlist;
        }

        /// <summary>
        /// Test using transient simulation
        /// The netlist should contain a transient simulation. The first exporter is tested to the voltage
        /// </summary>
        /// <param name="netlist"></param>
        /// <param name="reft"></param>
        /// <param name="refv"></param>
        protected void TestTransient(Netlist netlist, double[] reft, double[] refv)
        {
            var interpolation = LinearSpline.Interpolate(reft, refv);
            netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double time = data.GetTime();
                double actual = netlist.Exports[0].Extract(data);
                double expected = interpolation.Interpolate(time);
                double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * 1e-3 + 1e-12;
                Assert.AreEqual(expected, actual, tol);
            };
            netlist.Simulate();
        }
    }
}
