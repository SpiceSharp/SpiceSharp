using System;
using System.Numerics;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharp.Circuits;
using NUnit.Framework;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations.Behaviors;
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
            static NodeMapper()
            {
                RegisterBehaviorFactory(typeof(NodeMapper), new BehaviorFactoryDictionary
                {
                    {typeof(Mapper), e => new Mapper(((NodeMapper)e)._nodes)}
                });
            }
            private class Mapper : ExportingBehavior, IBiasingBehavior
            {
                private List<string> _nodes;
                public Mapper(List<string> nodes) : base("Mapper")
                {
                    _nodes = nodes;
                }

                public override void Setup(Simulation simulation, SetupDataProvider provider)
                {
                }
                public void GetEquationPointers(VariableSet variables, Solver<double> solver)
                {
                    foreach (var node in _nodes)
                        variables.MapNode(node);
                }
                public void Load(BaseSimulation simulation)
                {
                }
                public bool IsConvergent(BaseSimulation simulation) => true;
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
                entity.SetParameter(name, value);
            }
        }

        /// <summary>
        /// Perform a test for OP analysis
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="ckt">Circuit</param>
        /// <param name="exports">Exports</param>
        /// <param name="references">References</param>
        protected void AnalyzeOp(OP sim, Circuit ckt, IEnumerable<Export<double>> exports, IEnumerable<double> references)
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
        protected void AnalyzeDC(DC sim, Circuit ckt, IEnumerable<Export<double>> exports, IEnumerable<double[]> references)
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
        protected void AnalyzeDC(DC sim, Circuit ckt, IEnumerable<Export<double>> exports, IEnumerable<Func<double, double>> references)
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
        protected void AnalyzeAC(AC sim, Circuit ckt, IEnumerable<Export<double>> exports, IEnumerable<double[]> references)
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
        protected void AnalyzeAC(AC sim, Circuit ckt, IEnumerable<Export<Complex>> exports, IEnumerable<Complex[]> references)
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
        protected void AnalyzeAC(AC sim, Circuit ckt, IEnumerable<Export<Complex>> exports, IEnumerable<Func<double, Complex>> references)
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
        protected void AnalyzeTransient(Transient sim, Circuit ckt, IEnumerable<Export<double>> exports, IEnumerable<double[]> references)
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
        protected void AnalyzeTransient(Transient sim, Circuit ckt, IEnumerable<Export<double>> exports, IEnumerable<Func<double, double>> references)
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
        protected void AnalyzeNoise(Noise sim, Circuit ckt, IEnumerable<Export<double>> exports, IEnumerable<double[]> references)
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
        protected void WriteExportsToConsole(Simulation sim, Circuit ckt, IEnumerable<Export<double>> exports)
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
        protected void DestroyExports<T>(IEnumerable<Export<T>> exports)
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
            Console.WriteLine("----------- Dumping transient information -------------");
            Console.WriteLine($"Base time: {tran.Method.BaseTime}");
            Console.WriteLine($"Target time: {tran.Method.Time}");
            Console.Write($"Last timesteps (current first):");
            for (var i = 0; i <= tran.Method.MaxOrder; i++)
                Console.Write("{0}{1}", i > 0 ? ", " : "", tran.Method.GetTimestep(i));
            Console.WriteLine();
            Console.WriteLine("Problem variable: {0}", tran.ProblemVariable);
            Console.WriteLine("Problem variable value: {0}", tran.RealState.Solution[tran.ProblemVariable.Index]);
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
                    Console.Write($"({string.Join(", ", c.GetNodeIndexes(tran.Variables))})");
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            // Dump the current iteration solution
            Console.WriteLine("- Solutions");
            Dictionary<int, string> variables = new Dictionary<int, string>();
            foreach (var variable in tran.Variables)
                variables.Add(variable.Index, $"{variable.Index} - {variable.Name} ({variable.UnknownType}): {tran.RealState.Solution[variable.Index]}");
            for (var i = 0; i <= tran.Method.MaxOrder; i++)
            {
                var oldsolution = tran.Method.GetSolution(i);
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
            var state = tran.Method.GetStates(0);
            string[] output = new string[state.Length];
            for (var i = 0; i < state.Length; i++)
                output[i] = $"{state[i]}";
            for (var k = 1; k <= tran.Method.MaxOrder; k++)
            {
                state = tran.Method.GetStates(k);
                for (var i = 0; i < state.Length; i++)
                    output[i] += $", {state[i]}";
            }
            for (var i = 0; i < output.Length; i++)
                Console.WriteLine(output[i]);
            Console.WriteLine();
            #endif

            Console.WriteLine("------------------------ End of information ------------------------");
        }
    }
}
