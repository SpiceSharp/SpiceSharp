using System;
using System.IO;
using System.Text.RegularExpressions;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharp.Circuits;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.Behaviors;

namespace SpiceSharpTest.Models
{
    /// <summary>
    /// Framework for testing models
    /// </summary>
    public class Framework
    {
        /// <summary>
        /// Apply a parameter definition to an entity
        /// Parameters are a series of assignments [name]=[value] delimited by spaces.
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="definition">Definition string</param>
        protected void ApplyParameters(Entity entity, string definition)
        {
            // Get all assignments
            definition = Regex.Replace(definition, @"\s*\=\s*", "=");
            string[] assignments = definition.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var assignment in assignments)
            {
                // Get the name and value
                string[] parts = assignment.Split('=');
                if (parts.Length != 2)
                    throw new Exception("Invalid assignment");
                string name = parts[0].ToLower();
                double value = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);

                // Set the entity parameter
                entity.Parameters.Set(name, value);
            }
        }

        /// <summary>
        /// Perform a test for a DC analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyzeDC(DC sim, Circuit ckt, List<Export> exports, List<double[]> references)
        {
            double abstol = sim.CurrentConfig.AbsTol;
            double reltol = sim.CurrentConfig.RelTol;

            int index = 0;
            Func<double> current = null;
            sim.InitializeSimulationExport += (object sender, BehaviorPool pool) =>
            {
                current = pool.GetEntityBehaviors(source).Get<SpiceSharp.Behaviors.VSRC.LoadBehavior>().CreateExport(ckt.State, "i");
            };
            sim.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double actual = current();
                double expected = reference[index++];
                double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * reltol + abstol;

                try
                {
                    Assert.AreEqual(expected, actual, tol);
                }
                catch (Exception ex)
                {
                    string[] sweeps = new string[sim.Sweeps.Count];
                    for (int i = 0; i < sim.Sweeps.Count; i++)
                        sweeps[i] += $"{sim.Sweeps[i].ComponentName}={sim.Sweeps[i].CurrentValue}";
                    string msg = ex.Message + " : " + string.Join(" ", sweeps);
                    throw new Exception(msg);
                }
            };
            sim.Run(ckt);
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
            int index = 0;
            netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double time = data.GetTime();
                double actual = netlist.Exports[0].Extract(data);
                double expected = refv[index]; // interpolation.Interpolate(time);
                double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * 1e-5 + 1e-9;
                Assert.AreEqual(expected, actual, tol);
                index++;
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
