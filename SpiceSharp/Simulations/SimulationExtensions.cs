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
            var state = ckt.State;
            state.Init = CircuitState.InitFlags.InitJct;
            state.Matrix.Complex = false;

            // First, let's try finding an operating point by using normal iterations
            if (!config.NoOpIter)
            {
                if (ckt.Iterate(loaders, config, maxiter))
                    return;
            }

            // No convergence, try Gmin stepping
            if (config.NumGminSteps > 1)
            {
                state.Init = CircuitState.InitFlags.InitJct;
                CircuitWarning.Warning(ckt, "Starting Gmin stepping");
                state.DiagGmin = config.Gmin;
                for (int i = 0; i < config.NumGminSteps; i++)
                    state.DiagGmin *= 10.0;
                for (int i = 0; i <= config.NumGminSteps; i++)
                {
                    state.IsCon = false;
                    if (!ckt.Iterate(loaders, config, maxiter))
                    {
                        state.DiagGmin = 0.0;
                        CircuitWarning.Warning(ckt, "Gmin step failed");
                        break;
                    }
                    state.DiagGmin /= 10.0;
                    state.Init = CircuitState.InitFlags.InitFloat;
                }
                state.DiagGmin = 0.0;
                if (ckt.Iterate(loaders, config, maxiter))
                    return;
            }

            // Nope, still not converging, let's try source stepping
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
            matrix.Complex = false;

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
                    matrix.PreOrder();
                    state.Sparse |= CircuitState.SparseFlags.NIDIDPREORDER;
                }
                if (state.Init == CircuitState.InitFlags.InitJct || state.Init == CircuitState.InitFlags.InitTransient)
                {
                    state.Sparse |= CircuitState.SparseFlags.NISHOULDREORDER;
                }

                // Reorder
                if (state.Sparse.HasFlag(CircuitState.SparseFlags.NISHOULDREORDER))
                {
                    ckt.Statistics.ReorderTime.Start();
                    matrix.Reorder(state.PivotRelTol, state.PivotAbsTol, state.DiagGmin);
                    ckt.Statistics.ReorderTime.Stop();
                    state.Sparse &= ~CircuitState.SparseFlags.NISHOULDREORDER;
                }
                else
                {
                    // Decompose
                    ckt.Statistics.DecompositionTime.Start();
                    matrix.Factor(state.DiagGmin);
                    ckt.Statistics.DecompositionTime.Stop();
                }

                // Solve the equation
                ckt.Statistics.SolveTime.Start();
                matrix.Solve(state.Rhs);
                ckt.Statistics.SolveTime.Stop();

                // The result is now stored in the RHS vector, let's move it to the current solution vector
                state.StoreSolution();

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
                    state.IsCon = ckt.IsConvergent(loaders, config);
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

                    case CircuitState.InitFlags.InitTransient:
                        if (iterno <= 1)
                            state.Sparse = CircuitState.SparseFlags.NISHOULDREORDER;
                        state.Init = CircuitState.InitFlags.InitFloat;
                        break;

                    case CircuitState.InitFlags.Init:
                        state.Init = CircuitState.InitFlags.InitFloat;
                        break;

                    default:
                        ckt.Statistics.NumIter += iterno;
                        throw new CircuitException("Could not find flag");
                }
            }
        }

        /// <summary>
        /// Calculate the solution for <see cref="AC"/> analysis
        /// </summary>
        /// <param name="sim">The simulation</param>
        /// <param name="ckt">The circuit</param>
        public static void AcIterate(this Circuit ckt, List<CircuitObjectBehaviorAcLoad> acloaders, SimulationConfiguration config)
        {
            var state = ckt.State;
            var matrix = state.Matrix;
            matrix.Complex = true;

            // Initialize the circuit
            if (!state.Initialized)
                state.Initialize(ckt);

            retry:
            state.IsCon = true;

            // Load AC
            state.Clear();
            foreach (var behaviour in acloaders)
                behaviour.Load(ckt);

            if (state.Sparse.HasFlag(CircuitState.SparseFlags.NIACSHOULDREORDER))
            {
                var error = matrix.Reorder(state.PivotAbsTol, state.PivotRelTol);
                state.Sparse &= ~CircuitState.SparseFlags.NIACSHOULDREORDER;
                if (error != SparseError.Okay)
                    throw new CircuitException("Sparse matrix exception: " + SparseUtilities.ErrorMessage(state.Matrix, "AC"));
            }
            else
            {
                var error = matrix.Factor();
                if (error != 0)
                {
                    if (error == SparseError.Singular)
                    {
                        state.Sparse |= CircuitState.SparseFlags.NIACSHOULDREORDER;
                        goto retry;
                    }
                    throw new CircuitException("Sparse matrix exception: " + SparseUtilities.ErrorMessage(state.Matrix, "AC"));
                }
            }

            // Solve
            matrix.Solve(state.Rhs, state.iRhs);

            // Reset values
            state.Rhs[0] = 0.0;
            state.iRhs[0] = 0.0;
            state.Solution[0] = 0.0;
            state.iSolution[0] = 0.0;

            // Store them in the solution
            state.StoreSolution(true);
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

            state.Matrix.SolveTransposed(state.Rhs, state.iRhs);

            state.StoreSolution(true);

            state.Solution[0] = 0.0;
            state.iSolution[0] = 0.0;
        }

        /// <summary>
        /// Check if we are converging during iterations
        /// </summary>
        /// <param name="sim">The simulation</param>
        /// <param name="ckt">The circuit</param>
        /// <returns></returns>
        private static bool IsConvergent(this Circuit ckt, List<CircuitObjectBehaviorLoad> loaders, SimulationConfiguration config)
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

            // Device-level convergence tests
            foreach (var loader in loaders)
            {
                if (!loader.IsConvergent(ckt))
                {
                    // I believe this should be false, but Spice 3f5 doesn't...

                    /*
                     * Each device that checks convergence returns (OK) = 0 regardless
                     * of convergence (eg. Dev2/mos2conv.c). Not being convergent is communicated
                     * through the CKTnoncon variable (state.IsCon for Spice#).
                     * 
                     * The convergence methods are called in CKT/cktop.c at line 121. If an error
                     * occurs, it is returned. If non-convergence is detected through CKTnoncon,
                     * (OK) is returned anyway, so it doesn't make a difference. Remember that 
                     * our devices aren't returning anything else but (OK) anyway.
                     * 
                     * CKTconvTest in turn is called in NI/niconv.c at line 65. The result is
                     * therefore always (OK) too, and so the returned value by NIconvTest()
                     * is always (OK) if each device has tested convergence. Note that 1 is
                     * returned in the case of non-convergence for nodes!
                     * 
                     * Finally, in NI/niiter.c at line 184, when convergence is tested, the result
                     * is used to overwrite CKTnoncon, so there is no way we can still find out if
                     * any device detected non-convergence.
                     */
                    return true;
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
                loader.Load(ckt);

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
        public static void Ic(this ISimulation simulation, List<CircuitObjectBehaviorIc> icbehaviors)
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
                foreach (var behavior in icbehaviors)
                    behavior.SetIc(ckt);
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
