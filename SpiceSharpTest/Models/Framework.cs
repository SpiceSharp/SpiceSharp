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
        /// Test using DC simulation
        /// The netlist should contain one DC simulation. The first exporter is tested to the reference
        /// </summary>
        /// <param name="netlist">Netlist</param>
        /// <param name="reference">Reference values</param>
        protected void TestDC(Netlist netlist, double[] reference)
        {
            int index = 0;
            netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double actual = netlist.Exports[0].Extract(data);
                double expected = reference[index++];
                double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * 1e-3 + 1e-14;
                Assert.AreEqual(expected, actual, tol);
            };
            netlist.Simulate();
        }

        /// <summary>
        /// Test using AC analysis
        /// The netlist should contain one AC analysis and two exporters. The first exporter is the real part,
        /// the second exporter the second part. The reference is the real part followed by the imaginary part for each datapoint.
        /// </summary>
        /// <param name="netlist">Netlist</param>
        /// <param name="reference">Reference values</param>
        protected void TestAC(Netlist netlist, double[] reference)
        {
            int index = 0;
            netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                // Test real part
                double actual = netlist.Exports[0].Extract(data);
                double expected = reference[index++];
                double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * 1e-6 + 1e-12;
                Assert.AreEqual(expected, actual, tol);

                // Test the imaginary part
                actual = netlist.Exports[1].Extract(data);
                expected = reference[index++];
                tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * 1e-6 + 1e-12;
                Assert.AreEqual(expected, actual, tol);
            };
            netlist.Simulate();
        }

        /// <summary>
        /// Test using transient simulation
        /// The netlist should contain a transient simulation. The first exporter is tested to the reference
        /// </summary>
        /// <param name="netlist">Netlist</param>
        /// <param name="reft">Reference time values</param>
        /// <param name="refv">Reference values</param>
        protected void TestTransient(Netlist netlist, double[] reft, double[] refv)
        {
            var interpolation = LinearSpline.Interpolate(reft, refv);
            netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double time = data.GetTime();
                double actual = netlist.Exports[0].Extract(data);
                double expected = interpolation.Interpolate(time);
                double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * 1e-3 + 1e-6;
                Assert.AreEqual(expected, actual, tol);
            };
            netlist.Simulate();
        }

        /// <summary>
        /// Test using the noise simulation
        /// The netlist should contain a noise simulation. Input and output referred noise density are checked
        /// </summary>
        /// <param name="netlist">Netlist</param>
        /// <param name="reference">Reference values</param>
        protected void TestNoise(Netlist netlist, double[] reference_in, double[] reference_out)
        {
            int index = 0;
            netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double freq = data.GetFrequency();
                double actual = Math.Log(data.GetInputNoiseDensity());
                double expected = Math.Log(reference_in[index]);
                double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * 1e-3 + 1e-12;
                Assert.AreEqual(expected, actual, tol);

                actual = Math.Log(data.GetOutputNoiseDensity());
                expected = Math.Log(reference_out[index]);
                tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * 1e-3 + 1e-12;
                Assert.AreEqual(expected, actual, tol);

                index++;
            };
            netlist.Simulate();
        }
    }
}
