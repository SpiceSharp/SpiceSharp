using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharp.Circuits;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        /// Perform a test for OP analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyseOp(OP sim, Circuit ckt, IEnumerable<Func<State, double>> exports, IEnumerable<double> references)
        {
            double abstol = sim.CurrentConfig.AbsTol;
            double reltol = sim.CurrentConfig.RelTol;

            sim.OnExportSimulationData += (object sender, SimulationDataEventArgs data) =>
            {
                var export_it = exports.GetEnumerator();
                var references_it = references.GetEnumerator();

                while (export_it.MoveNext() && references_it.MoveNext())
                {
                    double actual = export_it.Current(data.Circuit.State);
                    double expected = references_it.Current;
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * reltol + abstol;
                    Assert.AreEqual(expected, actual, tol);
                }
            };
            sim.Run(ckt);
        }

        /// <summary>
        /// Perform a test for DC analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyzeDC(DC sim, Circuit ckt, IEnumerable<Func<State, double>> exports, IEnumerable<double[]> references)
        {
            double abstol = sim.CurrentConfig.AbsTol;
            double reltol = sim.CurrentConfig.RelTol;

            int index = 0;
            sim.OnExportSimulationData += (object sender, SimulationDataEventArgs data) =>
            {
                var export_it = exports.GetEnumerator();
                var references_it = references.GetEnumerator();

                while (export_it.MoveNext() && references_it.MoveNext())
                {
                    double actual = export_it.Current(data.Circuit.State);
                    double expected = references_it.Current[index];
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * reltol + abstol;

                    try
                    {
                        Assert.AreEqual(expected, actual, tol);
                    }
                    catch (Exception ex)
                    {
                        string[] sweeps = new string[sim.Sweeps.Count];
                        for (int k = 0; k < sim.Sweeps.Count; k++)
                            sweeps[k] += $"{sim.Sweeps[k].ComponentName}={sim.Sweeps[k].CurrentValue}";
                        string msg = ex.Message + " at " + string.Join(" ", sweeps);
                        throw new Exception(msg, ex);
                    }
                }
                index++;
            };
            sim.Run(ckt);
        }

        /// <summary>
        /// Perform a test for AC analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyseAC(AC sim, Circuit ckt, IEnumerable<Func<State, double>> exports, IEnumerable<double[]> references)
        {
            int index = 0;
            sim.OnExportSimulationData += (object sender, SimulationDataEventArgs data) =>
            {
                var exports_it = exports.GetEnumerator();
                var references_it = references.GetEnumerator();

                while (exports_it.MoveNext() && references_it.MoveNext())
                {
                    // Test export
                    double actual = exports_it.Current(data.Circuit.State);
                    double expected = references_it.Current[index];
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * 1e-6 + 1e-12;

                    try
                    {
                        Assert.AreEqual(expected, actual, tol);
                    }
                    catch (Exception ex)
                    {
                        string msg = $"{ex.Message} at {data.GetFrequency()} Hz";
                        throw new Exception(msg, ex);
                    }
                }
                index++;
            };
            sim.Run(ckt);
        }

        /// <summary>
        /// Perform a test for transient analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyseTransient(Transient sim, Circuit ckt, IEnumerable<Func<State, double>> exports, IEnumerable<double[]> references)
        {
            double abstol = sim.CurrentConfig.AbsTol;
            double reltol = sim.CurrentConfig.RelTol;

            int index = 0;
            sim.OnExportSimulationData += (object sender, SimulationDataEventArgs data) =>
            {
                var exports_it = exports.GetEnumerator();
                var references_it = references.GetEnumerator();

                while (exports_it.MoveNext() && references_it.MoveNext())
                {
                    double actual = exports_it.Current(data.Circuit.State);
                    double expected = references_it.Current[index];
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * reltol + abstol;

                    try
                    {
                        Assert.AreEqual(expected, actual, tol);
                    }
                    catch (Exception ex)
                    {
                        string msg = $"{ex.Message} at t={data.GetTime()} s";
                        throw new Exception(msg, ex);
                    }
                }
                index++;
            };
            sim.Run(ckt);
        }

        /// <summary>
        /// Perform a test for transient analysis where the reference is a function in time
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyseTransient(Transient sim, Circuit ckt, IEnumerable<Func<State, double>> exports, IEnumerable<Func<double, double>> references)
        {
            double abstol = sim.CurrentConfig.AbsTol;
            double reltol = sim.CurrentConfig.RelTol;

            int index = 0;
            sim.OnExportSimulationData += (object sender, SimulationDataEventArgs data) =>
            {
                var exports_it = exports.GetEnumerator();
                var references_it = references.GetEnumerator();

                while (exports_it.MoveNext() && references_it.MoveNext())
                {
                    double actual = exports_it.Current(data.Circuit.State);
                    double expected = references_it.Current(data.GetTime());
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * reltol + abstol;

                    try
                    {
                        Assert.AreEqual(expected, actual, tol);
                    }
                    catch (Exception ex)
                    {
                        string msg = $"{ex.Message} at t={data.GetTime()} s";
                        throw new Exception(msg, ex);
                    }
                }
                index++;
            };
            sim.Run(ckt);
        }

        /// <summary>
        /// Perform a test for noise analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyseNoise(Noise sim, Circuit ckt, IEnumerable<Func<State, double>> exports, IEnumerable<double[]> references)
        {
            int index = 0;
            sim.OnExportSimulationData += (object sender, SimulationDataEventArgs data) =>
            {
                var exports_it = exports.GetEnumerator();
                var references_it = references.GetEnumerator();

                while (exports_it.MoveNext() && references_it.MoveNext())
                {
                    // Test export
                    double actual = exports_it.Current(data.Circuit.State);
                    double expected = references_it.Current[index];
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * 1e-6 + 1e-12;

                    try
                    {
                        Assert.AreEqual(expected, actual, tol);
                    }
                    catch (Exception ex)
                    {
                        string msg = $"{ex.Message} at {data.GetFrequency()} Hz";
                        throw new Exception(msg, ex);
                    }
                }
                index++;
            };
            sim.Run(ckt);
        }
    }
}
