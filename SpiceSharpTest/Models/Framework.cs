using System;
using System.Numerics;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharp.Entities;
using NUnit.Framework;
using SpiceSharp.Behaviors;
using SpiceSharp.Components;

namespace SpiceSharpTest.Models
{
    /// <summary>
    /// Framework for testing models
    /// </summary>
    public class Framework
    {
        protected class NodeMapper : Entity
        {
            private class Mapper : Behavior, IBiasingBehavior
            {
                private List<string> _nodes;
                public Mapper(List<string> nodes, BindingContext context) : base("Mapper")
                {
                    context.ThrowIfNull(nameof(context));
                    _nodes = nodes;
                    var variables = context.Variables;
                    foreach (var node in _nodes)
                        variables.MapNode(node, VariableType.Voltage);
                }
                void IBiasingBehavior.Load() { }
                bool IBiasingBehavior.IsConvergent() => true;
            }
            readonly List<string> _nodes = new List<string>();
            public NodeMapper(params string[] nodes) : base("Mapper")
            {
                _nodes.AddRange(nodes);
            }
            public NodeMapper(IEnumerable<string> nodes) : base("Mapper")
            {
                _nodes.AddRange(nodes);
            }
            public override void CreateBehaviors(ISimulation simulation, IBehaviorContainer behaviors)
            {
                var context = new ModelBindingContext(simulation, behaviors);
                behaviors.Add(new Mapper(_nodes, context));
            }
        }

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
            var assignments = definition.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var assignment in assignments)
            {
                // Get the name and value
                var parts = assignment.Split('=');
                if (parts.Length != 2)
                    throw new Exception("Invalid assignment");
                var name = parts[0].ToLower();
                var value = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);

                // Set the entity parameter
                entity.Set(name, value);
            }
        }

        /// <summary>
        /// Perform a test for OP analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyzeOp(OP sim, Circuit ckt, IEnumerable<IExport<double>> exports, IEnumerable<double> references)
        {
            if (exports == null)
                throw new ArgumentNullException(nameof(exports));
            if (references == null)
                throw new ArgumentNullException(nameof(references));

            sim.ExportSimulationData += (sender, data) =>
            {
                using (var exportIt = exports.GetEnumerator())
                using (var referencesIt = references.GetEnumerator())
                {
                    while (exportIt.MoveNext() && referencesIt.MoveNext())
                    {
                        var actual = exportIt.Current?.Value ?? throw new ArgumentNullException();
                        var expected = referencesIt.Current;
                        var tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;
                        Assert.AreEqual(expected, actual, tol);
                    }
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
        protected void AnalyzeDC(DC sim, Circuit ckt, IEnumerable<IExport<double>> exports, IEnumerable<double[]> references)
        {
            if (exports == null)
                throw new ArgumentNullException(nameof(exports));
            if (references == null)
                throw new ArgumentNullException(nameof(references));

            var index = 0;
            sim.ExportSimulationData += (sender, data) =>
            {
                using (var exportIt = exports.GetEnumerator())
                using (var referencesIt = references.GetEnumerator())
                {
                    while (exportIt.MoveNext() && referencesIt.MoveNext())
                    {
                        var actual = exportIt.Current?.Value ?? double.NaN;
                        var expected = referencesIt.Current?[index] ?? double.NaN;
                        var tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;

                        try
                        {
                            Assert.AreEqual(expected, actual, tol);
                        }
                        catch (Exception ex)
                        {
                            var sweeps = new string[sim.Sweeps.Count];
                            for (var k = 0; k < sim.Sweeps.Count; k++)
                                sweeps[k] += $"{sim.Sweeps[k].Parameter}={sim.Sweeps[k].CurrentValue}";
                            var msg = ex.Message + " at " + string.Join(" ", sweeps);
                            throw new Exception(msg, ex);
                        }
                    }

                    index++;
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
        protected void AnalyzeDC(DC sim, Circuit ckt, IEnumerable<IExport<double>> exports, IEnumerable<Func<double, double>> references)
        {
            sim.ExportSimulationData += (sender, data) =>
            {
                using (var exportIt = exports.GetEnumerator())
                using (var referencesIt = references.GetEnumerator())
                {
                    while (exportIt.MoveNext() && referencesIt.MoveNext())
                    {
                        var actual = exportIt.Current?.Value ?? throw new ArgumentNullException();
                        var expected = referencesIt.Current?.Invoke(sim.Sweeps[0].CurrentValue) ?? throw new ArgumentNullException();
                        var tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;

                        try
                        {
                            Assert.AreEqual(expected, actual, tol);
                        }
                        catch (Exception ex)
                        {
                            var sweeps = new string[sim.Sweeps.Count];
                            for (var k = 0; k < sim.Sweeps.Count; k++)
                                sweeps[k] += $"{sim.Sweeps[k].Parameter}={sim.Sweeps[k].CurrentValue}";
                            var msg = ex.Message + " at " + string.Join(" ", sweeps);
                            throw new Exception(msg, ex);
                        }
                    }
                }
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
        protected void AnalyzeAC(AC sim, Circuit ckt, IEnumerable<IExport<double>> exports, IEnumerable<double[]> references)
        {
            var index = 0;
            sim.ExportSimulationData += (sender, data) =>
            {
                using (var exportsIt = exports.GetEnumerator())
                using (var referencesIt = references.GetEnumerator())
                {
                    while (exportsIt.MoveNext() && referencesIt.MoveNext())
                    {
                        // Test export
                        var actual = exportsIt.Current?.Value ?? throw new ArgumentNullException();
                        var expected = referencesIt.Current?[index] ?? throw new ArgumentNullException();
                        var tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;

                        try
                        {
                            Assert.AreEqual(expected, actual, tol);
                        }
                        catch (Exception ex)
                        {
                            var msg = $"{ex.Message} at {data.Frequency} Hz";
                            throw new Exception(msg, ex);
                        }
                    }

                    index++;
                }
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
        protected void AnalyzeAC(AC sim, Circuit ckt, IEnumerable<IExport<Complex>> exports, IEnumerable<Complex[]> references)
        {
            var index = 0;
            sim.ExportSimulationData += (sender, data) =>
            {
                using (var exportsIt = exports.GetEnumerator())
                using (var referencesIt = references.GetEnumerator())
                {
                    while (exportsIt.MoveNext() && referencesIt.MoveNext())
                    {
                        // Test export
                        var actual = exportsIt.Current?.Value ?? throw new ArgumentNullException();
                        var expected = referencesIt.Current?[index] ?? throw new ArgumentNullException();

                        // Test real part
                        var rtol = Math.Max(Math.Abs(actual.Real), Math.Abs(expected.Real)) * RelTol + AbsTol;
                        var itol = Math.Max(Math.Abs(actual.Imaginary), Math.Abs(expected.Imaginary)) * RelTol +
                                      AbsTol;

                        try
                        {
                            Assert.AreEqual(expected.Real, actual.Real, rtol);
                            Assert.AreEqual(expected.Imaginary, actual.Imaginary, itol);
                        }
                        catch (Exception ex)
                        {
                            var msg = $"{ex.Message} at {data.Frequency} Hz";
                            throw new Exception(msg, ex);
                        }
                    }

                    index++;
                }
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
        protected void AnalyzeAC(AC sim, Circuit ckt, IEnumerable<IExport<Complex>> exports, IEnumerable<Func<double, Complex>> references)
        {
            sim.ExportSimulationData += (sender, data) =>
            {
                using (var exportsIt = exports.GetEnumerator())
                using (var referencesIt = references.GetEnumerator())
                {
                    while (exportsIt.MoveNext() && referencesIt.MoveNext())
                    {
                        // Test export
                        var actual = exportsIt.Current?.Value ?? throw new ArgumentNullException();
                        var expected = referencesIt.Current?.Invoke(data.Frequency) ?? throw new ArgumentNullException();

                        // Test real part
                        var rtol = Math.Max(Math.Abs(actual.Real), Math.Abs(expected.Real)) * RelTol + AbsTol;
                        var itol = Math.Max(Math.Abs(actual.Imaginary), Math.Abs(expected.Imaginary)) * RelTol +
                                      AbsTol;

                        try
                        {
                            Assert.AreEqual(expected.Real, actual.Real, rtol);
                            Assert.AreEqual(expected.Imaginary, actual.Imaginary, itol);
                        }
                        catch (Exception ex)
                        {
                            var msg = $"{ex.Message} at {data.Frequency} Hz";
                            throw new Exception(msg, ex);
                        }
                    }
                }
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
        protected void AnalyzeTransient(Transient sim, Circuit ckt, IEnumerable<IExport<double>> exports, IEnumerable<double[]> references)
        {
            var index = 0;
            sim.ExportSimulationData += (sender, data) =>
            {
                using (var exportsIt = exports.GetEnumerator())
                using (var referencesIt = references.GetEnumerator())
                {
                    while (exportsIt.MoveNext() && referencesIt.MoveNext())
                    {
                        var actual = exportsIt.Current?.Value ?? throw new ArgumentNullException();
                        var expected = referencesIt.Current?[index] ?? throw new ArgumentNullException();
                        var tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;

                        try
                        {
                            Assert.AreEqual(expected, actual, tol);
                        }
                        catch (Exception ex)
                        {
                            var msg = $"{ex.Message} at t={data.Time} s";
                            throw new Exception(msg, ex);
                        }
                    }

                    index++;
                }
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
        protected void AnalyzeTransient(Transient sim, Circuit ckt, IEnumerable<IExport<double>> exports, IEnumerable<Func<double, double>> references)
        {
            sim.ExportSimulationData += (sender, data) =>
            {
                using (var exportsIt = exports.GetEnumerator())
                using (var referencesIt = references.GetEnumerator())
                {
                    while (exportsIt.MoveNext() && referencesIt.MoveNext())
                    {
                        var t = data.Time;
                        var actual = exportsIt.Current?.Value ?? throw new ArgumentNullException();
                        var expected = referencesIt.Current?.Invoke(t) ?? throw new ArgumentNullException();
                        var tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * RelTol + AbsTol;

                        try
                        {
                            Assert.AreEqual(expected, actual, tol);
                        }
                        catch (Exception ex)
                        {
                            var msg = $"{ex.Message} at t={data.Time} s";
                            throw new Exception(msg, ex);
                        }
                    }
                }
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
        protected void AnalyzeNoise(Noise sim, Circuit ckt, IEnumerable<IExport<double>> exports, IEnumerable<double[]> references)
        {
            var index = 0;
            sim.ExportSimulationData += (sender, data) =>
            {
                using (var exportsIt = exports.GetEnumerator())
                using (var referencesIt = references.GetEnumerator())
                {
                    while (exportsIt.MoveNext() && referencesIt.MoveNext())
                    {
                        // Test export
                        var actual = exportsIt.Current?.Value ?? throw new ArgumentNullException();
                        var expected = referencesIt.Current?[index] ?? throw new ArgumentNullException();
                        var tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * 1e-6 + 1e-12;

                        try
                        {
                            Assert.AreEqual(expected, actual, tol);
                        }
                        catch (Exception ex)
                        {
                            var msg = $"{ex.Message} at {data.Frequency} Hz";
                            throw new Exception(msg, ex);
                        }
                    }

                    index++;
                }
            };
            sim.Run(ckt);
        }

        /// <summary>
        /// Writes the exports to the console window.
        /// Can be used for debugging. The output is in the format:
        /// v0 = [ ... ];
        /// v1 = [ ... ];
        /// ...
        /// </summary>
        /// <param name="sim">The simulation.</param>
        /// <param name="ckt">The circuit.</param>
        /// <param name="exports">The exports.</param>
        protected void WriteExportsToConsole(Simulation sim, Circuit ckt, IEnumerable<IExport<double>> exports)
        {
            var arr = exports.ToArray();
            var output = new List<string>[arr.Length];
            for (var i = 0; i < arr.Length; i++)
                output[i] = new List<string>();

            sim.ExportSimulationData += (sender, args) =>
            {
                for (var i = 0; i < arr.Length; i++)
                    output[i].Add(arr[i].Value.ToString(CultureInfo.InvariantCulture));
            };

            sim.Run(ckt);

            for (var i = 0; i < arr.Length; i++)
                Console.WriteLine($"v{i} = [{string.Join(", ", output[i])} ];");
        }

        /// <summary>
        /// Destroy all exports.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <param name="exports">The exports.</param>
        protected void DestroyExports<T>(IEnumerable<IExport<T>> exports)
        {
            foreach (var export in exports)
                export.Destroy();
        }

        /// <summary>
        /// Dump transient information in the console (used for debugging).
        /// </summary>
        /// <param name="tran">The transient analysis.</param>
        /// <param name="ckt">The circuit.</param>
        protected void DumpTransientState(Transient tran, Circuit ckt)
        {
            var state = tran.State;
            tran.GetState(out IBiasingSimulationState rstate);

            Console.WriteLine("----------- Dumping transient information -------------");
            Console.WriteLine($"Base time: {state.Method.BaseTime}");
            Console.WriteLine($"Target time: {state.Method.Time}");
            Console.Write($"Last timesteps (current first):");
            for (var i = 0; i <= state.Method.MaxOrder; i++)
                Console.Write("{0}{1}", i > 0 ? ", " : "", state.Method.GetTimestep(i));
            Console.WriteLine();
            Console.WriteLine("Problem variable: {0}", tran.ProblemVariable);
            Console.WriteLine("Problem variable value: {0}", rstate.Solution[rstate.Map[tran.ProblemVariable]]);
            Console.WriteLine();

            // Dump the circuit contents
            Console.WriteLine("- Circuit contents");
            foreach (var entity in ckt)
            {
                Console.Write(entity.Name);
                if (entity is Component c)
                {
                    for (var i = 0; i < c.PinCount; i++)
                        Console.Write($"{c.GetNode(i)} ");
                    Console.Write($"({string.Join(", ", c.MapNodes(tran.Variables))})");
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            // Dump the current iteration solution
            Console.WriteLine("- Solutions");
            Dictionary<int, string> variables = new Dictionary<int, string>();
            foreach (var variable in rstate.Map)
                variables.Add(variable.Value, $"{variable.Value} - {variable.Key.Name} ({variable.Key.UnknownType}): {rstate.Solution[variable.Value]}");
            for (var i = 0; i <= state.Method.MaxOrder; i++)
            {
                var oldsolution = state.Method.GetSolution(i);
                for (var k = 1; k <= variables.Count; k++)
                    variables[k] += $", {oldsolution[k]}";
            }
            for (var i = 0; i <= variables.Count; i++)
            {
                if (variables.TryGetValue(i, out var value))
                    Console.WriteLine(value);
                else
                    Console.WriteLine($"Could not find variable for index {i}");
            }
            Console.WriteLine();

            // Dump the states used by the transient
            #if DEBUG
            Console.WriteLine("- States");
            var intstate = state.Method.GetStates(0);
            string[] output = new string[intstate.Length];
            for (var i = 0; i < intstate.Length; i++)
                output[i] = $"{intstate[i]}";
            for (var k = 1; k <= state.Method.MaxOrder; k++)
            {
                intstate = state.Method.GetStates(k);
                for (var i = 0; i < intstate.Length; i++)
                    output[i] += $", {intstate[i]}";
            }
            for (var i = 0; i < output.Length; i++)
                Console.WriteLine(output[i]);
            Console.WriteLine();
            #endif

            Console.WriteLine("------------------------ End of information ------------------------");
        }
    }
}
