using System;
using System.Numerics;
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
        /// Absolute tolerance used
        /// </summary>
        public double AbsTol = 1e-12;

        /// <summary>
        /// Relative tolerance used
        /// </summary>
        public double RelTol = 1e-3;

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
                entity.Parameters.SetProperty(name, value);
            }
        }

        /// <summary>
        /// Perform a test for OP analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyzeOp(OP sim, Circuit ckt, IEnumerable<Func<State, double>> exports, IEnumerable<double> references)
        {
            sim.OnExportSimulationData += (object sender, ExportDataEventArgs data) =>
            {
                var export_it = exports.GetEnumerator();
                var references_it = references.GetEnumerator();

                while (export_it.MoveNext() && references_it.MoveNext())
                {
                    double actual = export_it.Current(data.State);
                    double expected = references_it.Current;
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;
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
            int index = 0;
            sim.OnExportSimulationData += (object sender, ExportDataEventArgs data) =>
            {
                var export_it = exports.GetEnumerator();
                var references_it = references.GetEnumerator();

                while (export_it.MoveNext() && references_it.MoveNext())
                {
                    double actual = export_it.Current(data.State);
                    double expected = references_it.Current[index];
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;

                    try
                    {
                        Assert.AreEqual(expected, actual, tol);
                    }
                    catch (Exception ex)
                    {
                        string[] sweeps = new string[sim.Sweeps.Count];
                        for (int k = 0; k < sim.Sweeps.Count; k++)
                            sweeps[k] += $"{sim.Sweeps[k].Parameter}={sim.Sweeps[k].CurrentValue}";
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
        protected void AnalyzeAC(AC sim, Circuit ckt, IEnumerable<Func<State, double>> exports, IEnumerable<double[]> references)
        {
            int index = 0;
            sim.OnExportSimulationData += (object sender, ExportDataEventArgs data) =>
            {
                var exports_it = exports.GetEnumerator();
                var references_it = references.GetEnumerator();

                while (exports_it.MoveNext() && references_it.MoveNext())
                {
                    // Test export
                    double actual = exports_it.Current(data.State);
                    double expected = references_it.Current[index];
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;

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
        /// Perform a test for AC analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyzeAC(AC sim, Circuit ckt, IEnumerable<Func<State, Complex>> exports, IEnumerable<Complex[]> references)
        {
            int index = 0;
            sim.OnExportSimulationData += (object sender, ExportDataEventArgs data) =>
            {
                var exports_it = exports.GetEnumerator();
                var references_it = references.GetEnumerator();

                while (exports_it.MoveNext() && references_it.MoveNext())
                {
                    // Test export
                    Complex actual = exports_it.Current(data.State);
                    Complex expected = references_it.Current[index];

                    // Test real part
                    double rtol = Math.Max(Math.Abs(actual.Real), Math.Abs(expected.Real)) * RelTol + AbsTol;
                    double itol = Math.Max(Math.Abs(actual.Imaginary), Math.Abs(expected.Imaginary)) * RelTol + AbsTol;

                    try
                    {
                        Assert.AreEqual(expected.Real, actual.Real, rtol);
                        Assert.AreEqual(expected.Imaginary, actual.Imaginary, itol);
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
        /// Perform a test for AC analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyzeAC(AC sim, Circuit ckt, IEnumerable<Func<State, Complex>> exports, IEnumerable<Func<double, Complex>> references)
        {
            int index = 0;
            sim.OnExportSimulationData += (object sender, ExportDataEventArgs data) =>
            {
                var exports_it = exports.GetEnumerator();
                var references_it = references.GetEnumerator();

                while (exports_it.MoveNext() && references_it.MoveNext())
                {
                    // Test export
                    Complex actual = exports_it.Current(data.State);
                    Complex expected = references_it.Current(data.GetFrequency());

                    // Test real part
                    double rtol = Math.Max(Math.Abs(actual.Real), Math.Abs(expected.Real)) * RelTol + AbsTol;
                    double itol = Math.Max(Math.Abs(actual.Imaginary), Math.Abs(expected.Imaginary)) * RelTol + AbsTol;

                    try
                    {
                        Assert.AreEqual(expected.Real, actual.Real, rtol);
                        Assert.AreEqual(expected.Imaginary, actual.Imaginary, itol);
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
        protected void AnalyzeTransient(Transient sim, Circuit ckt, IEnumerable<Func<State, double>> exports, IEnumerable<double[]> references)
        {
            int index = 0;
            sim.OnExportSimulationData += (object sender, ExportDataEventArgs data) =>
            {
                var exports_it = exports.GetEnumerator();
                var references_it = references.GetEnumerator();

                while (exports_it.MoveNext() && references_it.MoveNext())
                {
                    double actual = exports_it.Current(data.State);
                    double expected = references_it.Current[index];
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;

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
        protected void AnalyzeTransient(Transient sim, Circuit ckt, IEnumerable<Func<State, double>> exports, IEnumerable<Func<double, double>> references)
        {
            int index = 0;
            sim.OnExportSimulationData += (object sender, ExportDataEventArgs data) =>
            {
                var exports_it = exports.GetEnumerator();
                var references_it = references.GetEnumerator();

                while (exports_it.MoveNext() && references_it.MoveNext())
                {
                    double t = data.GetTime();
                    double actual = exports_it.Current(data.State);
                    double expected = references_it.Current(t);
                    double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;

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
        protected void AnalyzeNoise(Noise sim, Circuit ckt, IEnumerable<Func<State, double>> exports, IEnumerable<double[]> references)
        {
            int index = 0;
            sim.OnExportSimulationData += (object sender, ExportDataEventArgs data) =>
            {
                var exports_it = exports.GetEnumerator();
                var references_it = references.GetEnumerator();

                while (exports_it.MoveNext() && references_it.MoveNext())
                {
                    // Test export
                    double actual = exports_it.Current(data.State);
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
