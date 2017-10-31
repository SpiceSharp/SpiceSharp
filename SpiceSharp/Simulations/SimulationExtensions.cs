using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;
using SpiceSharp.Sparse;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Provides static methods for basic simulations involving the circuit
    /// </summary>
    public static class SimulationExtensions
    {
        /// <summary>
        /// Calculate the operating point of the circuit
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="loaders">Loaders</param>
        /// <param name="config">Simulation configuration</param>
        /// <param name="maxiter">Maximum iterations</param>
        public static void Op(this Circuit ckt, List<CircuitObjectBehaviorLoad> loaders, SimulationConfiguration config, int maxiter)
        {
            // Create the current SimulationState
            var state = ckt.State;
            state.Init = CircuitState.InitFlags.InitJct;

            if (!config.NoOpIter)
            {
                if (ckt.Iterate(loaders, config, maxiter))
                    return;
            }

            // No convergence
            // try Gmin stepping
            if (config.NumGminSteps > 1)
            {
                state.Init = CircuitState.InitFlags.InitJct;
                CircuitWarning.Warning(ckt, "Starting Gmin stepping");
                state.Gmin = config.Gmin;
                for (int i = 0; i < config.NumGminSteps; i++)
                    state.Gmin *= 10.0;
                for (int i = 0; i <= config.NumGminSteps; i++)
                {
                    state.IsCon = true;
                    if (!ckt.Iterate(loaders, config, maxiter))
                    {
                        state.Gmin = 0.0;
                        CircuitWarning.Warning(ckt, "Gmin step failed");
                        break;
                    }
                    state.Gmin /= 10.0;
                    state.Init = CircuitState.InitFlags.InitFloat;
                }
                state.Gmin = 0.0;
                if (ckt.Iterate(loaders, config, maxiter))
                    return;
            }

            // No we'll try source stepping
            if (config.NumSrcSteps > 1)
            {
                state.Init = CircuitState.InitFlags.InitJct;
                CircuitWarning.Warning(ckt, "Starting source stepping");
                for (int i = 0; i <= config.NumSrcSteps; i++)
                {
                    state.SrcFact = i / (double) config.NumSrcSteps;
                    if (!ckt.Iterate(loaders, config, maxiter))
                    {
                        state.SrcFact = 1.0;
                        // ckt.CurrentAnalysis = AnalysisType.DoingTran;
                        CircuitWarning.Warning(ckt, "Source stepping failed");
                        return;
                    }
                }
                state.SrcFact = 1.0;
                return;
            }

            // Failed
            throw new CircuitException("Could not determine operating point");
        }

        /// <summary>
        /// Solve iteratively for <see cref="OP"/>, <see cref="DC"/> or <see cref="Transient"/> simulations
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="loaders">Loaders</param>
        /// <param name="config">Simulation configuration</param>
        /// <param name="maxiter">Maximum number of iterations</param>
        /// <returns></returns>
        public static bool Iterate(this Circuit ckt, List<CircuitObjectBehaviorLoad> loaders, SimulationConfiguration config, int maxiter)
        {
            var state = ckt.State;
            var matrix = state.Matrix;
            bool pass = false;
            int iterno = 0;

            // Make sure we're using real numbers!
            matrix.SetReal();

            // Initialize the state of the circuit
            if (!state.Initialized)
                state.Initialize(ckt);

            // Ignore operating condition point, just use the solution as-is
            if (ckt.State.UseIC && ckt.State.Domain == CircuitState.DomainTypes.Time)
            {
                state.StoreSolution();

                // Voltages are set using IC statement on the nodes
                // Internal initial conditions are calculated by the components
                ckt.Load(loaders);
                return true;
            }

            // Perform iteration
            while (true)
            {
                // Reset convergence flag
                state.IsCon = true;

                try
                {
                    ckt.Load(loaders);
                    iterno++;
                }
                catch (CircuitException)
                {
                    iterno++;
                    ckt.Statistics.NumIter = iterno;
                    throw;
                }

                // Preorder matrix
                if (!state.Sparse.HasFlag(CircuitState.SparseFlags.NIDIDPREORDER))
                {
                    matrix.SMPpreOrder();
                    state.Sparse |= CircuitState.SparseFlags.NIDIDPREORDER;
                }
                if (state.Init == CircuitState.InitFlags.InitJct || (ckt.Method != null && ckt.Method.SavedTime == 0.0))
                {
                    state.Sparse |= CircuitState.SparseFlags.NISHOULDREORDER;
                }

                // Reorder
                if (state.Sparse.HasFlag(CircuitState.SparseFlags.NISHOULDREORDER))
                {
                    ckt.Statistics.ReorderTime.Start();

                    // Add diagonal GMIN, then order and factor using pivots on the diagonal
                    matrix.LoadGmin(ckt.Simulation?.Config?.DiagGmin ?? 0.0);
                    matrix.OrderAndFactor(state.Rhs, state.PivotRelTol, state.PivotAbsTol, true);

                    ckt.Statistics.ReorderTime.Stop();
                    state.Sparse &= ~CircuitState.SparseFlags.NISHOULDREORDER;
                }
                else
                {
                    // Decompose
                    ckt.Statistics.DecompositionTime.Start();

                    matrix.LoadGmin(ckt.Simulation?.Config?.DiagGmin ?? 0.0);
                    matrix.Factor();
                    ckt.Statistics.DecompositionTime.Stop();
                }

                // Solve the equation
                ckt.Statistics.SolveTime.Start();
                matrix.SMPsolve(state.Rhs, null);
                ckt.Statistics.SolveTime.Stop();

                // Reset ground nodes
                ckt.State.Rhs[0] = 0.0;
                ckt.State.Solution[0] = 0.0;
                ckt.State.OldSolution[0] = 0.0;

                // Exceeded maximum number of iterations
                if (iterno > maxiter)
                {
                    ckt.Statistics.NumIter += iterno;
                    return false;
                }

                if (state.IsCon && iterno != 1)
                    state.IsCon = ckt.IsConvergent(config);
                else
                    state.IsCon = false;

                switch (state.Init)
                {
                    case CircuitState.InitFlags.InitFloat:
                        if (state.UseDC && state.HadNodeset)
                        {
                            if (pass)
                                state.IsCon = false;
                            pass = false;
                        }
                        if (state.IsCon)
                        {
                            ckt.Statistics.NumIter += iterno;
                            return true;
                        }
                        break;

                    case CircuitState.InitFlags.InitJct:
                        state.Init = CircuitState.InitFlags.InitFix;
                        state.Sparse |= CircuitState.SparseFlags.NISHOULDREORDER;
                        break;

                    case CircuitState.InitFlags.InitFix:
                        if (state.IsCon)
                            state.Init = CircuitState.InitFlags.InitFloat;
                        pass = true;
                        break;

                    case CircuitState.InitFlags.Init:
                        state.Init = CircuitState.InitFlags.InitFloat;
                        break;

                    default:
                        ckt.Statistics.NumIter += iterno;
                        throw new CircuitException("Could not find flag");
                }

                // We need to do another iteration, swap solutions with the old solution
                state.StoreSolution();
            }
        }

        /// <summary>
        /// Calculate the solution for <see cref="AC"/> analysis
        /// </summary>
        /// <param name="sim">The simulation</param>
        /// <param name="ckt">The circuit</param>
        public static void AcIterate(this Circuit ckt, List<CircuitObjectBehaviorAcLoad> loaders, SimulationConfiguration config)
        {
            var state = ckt.State;
            var matrix = state.Matrix;

            // Initialize the circuit
            if (!state.Initialized)
                state.Initialize(ckt);

            retry:
            state.IsCon = true;

            // Load AC
            state.Clear();
            foreach (var behaviour in loaders)
                behaviour.Execute(ckt);

            if (state.Sparse.HasFlag(CircuitState.SparseFlags.NIACSHOULDREORDER))
            {
                matrix.SMPcReorder(state.PivotAbsTol, state.PivotRelTol, out _);
                state.Sparse &= ~CircuitState.SparseFlags.NIACSHOULDREORDER;
            }
            else
            {
                var error = matrix.SMPcLUfac(state.PivotAbsTol);
                if (error != 0)
                {
                    if (error == SparseError.Singular)
                    {
                        state.Sparse |= CircuitState.SparseFlags.NIACSHOULDREORDER;
                        goto retry;
                    }
                }
            }

            // Solve
            matrix.SMPcSolve(state.Rhs, state.iRhs, null, null);

            // Reset values
            state.Rhs[0] = 0.0;
            state.iRhs[0] = 0.0;
            state.Solution[0] = 0.0;
            state.iSolution[0] = 0.0;

            // Store them in the solution
            state.StoreSolution();
        }

        /// <summary>
        /// Calculate the solution for <see cref="Noise"/> analysis
        /// This routine solves the adjoint system. It assumes that the matrix has
        /// already been loaded by a call to <see cref="AcIterate(SimulationConfiguration, Circuit)"/>, so it only alters the right
        /// hand side vector. The unit-valued current excitation is applied between
        /// nodes posDrive and negDrive.
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <param name="posDrive">The positive driving node</param>
        /// <param name="negDrive">The negative driving node</param>
        public static void NzIterate(this Circuit ckt, int posDrive, int negDrive)
        {
            var state = ckt.State;

            // Clear out the right hand side vector
            for (int i = 0; i < state.Rhs.Length; i++)
            {
                state.Rhs[i] = 0.0;
                state.iRhs[i] = 0.0;
            }

            // Apply unit current excitation
            state.Rhs[posDrive] = 1.0;
            state.Rhs[negDrive] = -1.0;

            state.Matrix.SMPcaSolve(state.Rhs, state.iRhs, null, null);

            state.Solution[0] = 0.0;
            state.iSolution[0] = 0.0;
        }

        /// <summary>
        /// Check if we are converging during iterations
        /// </summary>
        /// <param name="sim">The simulation</param>
        /// <param name="ckt">The circuit</param>
        /// <returns></returns>
        private static bool IsConvergent(this Circuit ckt, SimulationConfiguration config)
        {
            var rstate = ckt.State;

            // Check convergence for each node
            for (int i = 0; i < ckt.Nodes.Count; i++)
            {
                var node = ckt.Nodes[i];
                double n = rstate.Solution[node.Index];
                double o = rstate.OldSolution[node.Index];

                if (double.IsNaN(n))
                    throw new CircuitException($"Non-convergence, node {node} is not a number.");

                if (node.Type == CircuitNode.NodeType.Voltage)
                {
                    double tol = config.RelTol * Math.Max(Math.Abs(n), Math.Abs(o)) + config.VoltTol;
                    if (Math.Abs(n - o) > tol)
                    {
                        ProblemNode = node;
                        return false;
                    }
                }
                else
                {
                    double tol = config.RelTol * Math.Max(Math.Abs(n), Math.Abs(o)) + config.AbsTol;
                    if (Math.Abs(n - o) > tol)
                    {
                        ProblemNode = node;
                        return false;
                    }
                }
            }

            // Convergence succeeded
            return true;
        }

        /// <summary>
        /// Access to the problem node for convergence
        /// </summary>
        public static CircuitNode ProblemNode { get; set; }

        /// <summary>
        /// Load the circuit for <see cref="OP"/>, <see cref="DC"/> or <see cref="Transient"/> analysis
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="loaders">Loaders</param>
        public static void Load(this Circuit ckt, List<CircuitObjectBehaviorLoad> loaders)
        {
            var state = ckt.State;
            var rstate = state;
            var nodes = ckt.Nodes;

            // Start the stopwatch
            ckt.Statistics.LoadTime.Start();

            // Clear rhs and matrix
            rstate.Clear();

            // Load all devices
            foreach (var loader in loaders)
                loader.Execute(ckt);

            // Check modes
            if (state.UseDC)
            {
                // Consider doing nodeset & ic assignments
                if ((state.Init & (CircuitState.InitFlags.InitJct | CircuitState.InitFlags.InitFix)) != 0)
                {
                    // Do nodesets
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        if (nodes.Nodeset.ContainsKey(node.Name))
                        {
                            double ns = nodes.Nodeset[node.Name];
                            if (ZeroNoncurRow(rstate.Matrix, nodes, node.Index))
                            {
                                rstate.Rhs[node.Index] = 1.0e10 * ns;
                                node.Diagonal.Value.Real = 1e10;
                            }
                            else
                            {
                                rstate.Rhs[node.Index] = ns;
                                node.Diagonal.Value.Real = 1.0;
                            }
                        }
                    }
                }

                if (state.Domain == CircuitState.DomainTypes.Time && !state.UseIC)
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        if (nodes.IC.ContainsKey(node.Name))
                        {
                            double ic = nodes.IC[node.Name];
                            if (ZeroNoncurRow(rstate.Matrix, nodes, node.Index))
                            {
                                rstate.Rhs[node.Index] = 1.0e10 * ic;
                                node.Diagonal.Value.Real = 1e10;
                            }
                            else
                            {
                                rstate.Rhs[node.Index] = ic;
                                node.Diagonal.Value.Real = 1.0;
                            }
                        }
                    }
                }
            }

            // Keep statistics
            ckt.Statistics.LoadTime.Stop();
        }

        /// <summary>
        /// Set the initial conditions
        /// </summary>
        /// <param name="ckt"></param>
        public static void Ic(this ISimulation simulation)
        {
            var ckt = simulation.Circuit;
            var state = ckt.State;
            var rstate = state;
            var nodes = ckt.Nodes;

            // Clear the current solution
            for (int i = 0; i < rstate.Solution.Length; i++)
            {
                rstate.Rhs[i] = 0.0;
            }

            // Go over all nodes
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (nodes.Nodeset.ContainsKey(node.Name))
                {
                    node.Diagonal = rstate.Matrix.GetElement(node.Index, node.Index);
                    state.HadNodeset = true;
                    rstate.Rhs[node.Index] = nodes.Nodeset[node.Name];
                }
                if (nodes.IC.ContainsKey(node.Name))
                {
                    node.Diagonal = rstate.Matrix.GetElement(node.Index, node.Index);
                    rstate.Rhs[node.Index] = nodes.IC[node.Name];
                }
            }

            // Use initial conditions
            if (state.UseIC)
            {
                foreach (var c in ckt.Objects)
                    c.SetIc(ckt);
            }
        }

        /// <summary>
        /// Reset the row to 0.0 and return true if the row is a current equation
        /// </summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="nodes">The list of nodes</param>
        /// <param name="rownum">The row number</param>
        /// <returns></returns>
        private static bool ZeroNoncurRow(Matrix matrix, CircuitNodes nodes, int rownum)
        {
            bool currents = false;
            for (int n = 0; n < nodes.Count; n++)
            {
                var node = nodes[n];
                MatrixElement x = matrix.FindElement(rownum, node.Index);
                if (x != null && x.Value.Real != 0.0)
                {
                    if (node.Type == CircuitNode.NodeType.Current)
                        currents = true;
                    else
                        x.Value.Real = 0.0;
                }
            }
            return currents;
        }
    }
}
