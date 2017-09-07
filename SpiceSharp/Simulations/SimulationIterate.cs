using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This static class contains common methods for basic simulation.
    /// </summary>
    public static class SimulationIterate
    {
        /// <summary>
        /// Calculate the operating point of the circuit
        /// </summary>
        /// <param name="sim">The simulation</param>
        /// <param name="ckt">The circuit</param>
        /// <param name="maxiter">The maximum number of iterations</param>
        public static void Op(SimulationConfiguration config, Circuit ckt, int maxiter)
        {
            // Create the current SimulationState
            var state = ckt.State;
            state.Init = CircuitState.InitFlags.InitJct;

            if (!config.NoOpIter)
            {
                if (Iterate(config, ckt, maxiter))
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
                    if (!Iterate(config, ckt, maxiter))
                    {
                        state.Gmin = 0.0;
                        CircuitWarning.Warning(ckt, "Gmin step failed");
                        break;
                    }
                    state.Gmin /= 10.0;
                    state.Init = CircuitState.InitFlags.InitFloat;
                }
                state.Gmin = 0.0;
                if (Iterate(config, ckt, maxiter))
                    return;
            }

            // No we'll try source stepping
            if (config.NumSrcSteps > 1)
            {
                state.Init = CircuitState.InitFlags.InitJct;
                CircuitWarning.Warning(ckt, "Starting source stepping");
                for (int i = 0; i <= config.NumSrcSteps; i++)
                {
                    state.SrcFact = i / (double)config.NumSrcSteps;
                    if (!Iterate(config, ckt, maxiter))
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
        /// Iterate to a solution
        /// </summary>
        /// <param name="sim">The simulation</param>
        /// <param name="ckt">The circuit</param>
        /// <param name="maxiter">The maximum number of iterations</param>
        /// <returns></returns>
        public static bool Iterate(SimulationConfiguration config, Circuit ckt, int maxiter)
        {
            var state = ckt.State;
            var rstate = state.Real;
            bool pass = false;
            int iterno = 0;

            // Initialize the state of the circuit
            if (!state.Initialized)
                state.Initialize(ckt);

            // Ignore operating condition point, just use the solution as-is
            if (ckt.State.UseIC && ckt.State.Domain == CircuitState.DomainTypes.Time)
            {
                rstate.StoreSolution();

                // Voltages are set using IC statement on the nodes
                // Internal initial conditions are calculated by the components
                ckt.Load();
                return true;
            }

            // Perform iteration
            while (true)
            {
                // Reset convergence flag
                state.IsCon = true;

                try
                {
                    ckt.Load();
                    iterno++;
                }
                catch (CircuitException)
                {
                    iterno++;
                    ckt.Statistics.NumIter = iterno;
                    throw;
                }

                // Solve the equation (thank you Math.NET)
                ckt.Statistics.SolveTime.Start();
                rstate.Solve();
                ckt.Statistics.SolveTime.Stop();

                // Reset ground nodes
                ckt.State.Real.Solution[0] = 0.0;
                ckt.State.Complex.Solution[0] = 0.0;
                ckt.State.Real.OldSolution[0] = 0.0;

                // Exceeded maximum number of iterations
                if (iterno > maxiter)
                {
                    ckt.Statistics.NumIter += iterno;
                    return false;
                }

                if (state.IsCon && iterno != 1)
                    state.IsCon = IsConvergent(config, ckt);
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
                rstate.StoreSolution();
            }
        }

        /// <summary>
        /// Calculate the solution for AC analysis
        /// </summary>
        /// <param name="sim">The simulation</param>
        /// <param name="ckt">The circuit</param>
        public static void AcIterate(SimulationConfiguration config, Circuit ckt)
        {
            // Initialize the circuit
            if (!ckt.State.Initialized)
                ckt.State.Initialize(ckt);

            ckt.State.IsCon = true;

            // Load AC
            ckt.State.Complex.Clear();
            foreach (var c in ckt.Objects)
                c.AcLoad(ckt);

            // Solve
            ckt.State.Complex.Solve();
        }

        /// <summary>
        /// Check if we are converging during iterations
        /// </summary>
        /// <param name="sim">The simulation</param>
        /// <param name="ckt">The circuit</param>
        /// <returns></returns>
        private static bool IsConvergent(SimulationConfiguration config, Circuit ckt)
        {
            var rstate = ckt.State.Real;

            // Check convergence for each node
            for (int i = 0; i < ckt.Nodes.Count; i++)
            {
                var node = ckt.Nodes[i];
                double n = rstate.Solution[node.Index];
                double o = rstate.OldSolution[node.Index];
                if (node.Type == CircuitNode.NodeType.Voltage)
                {
                    double tol = config.RelTol * Math.Max(Math.Abs(n), Math.Abs(o)) + config.VoltTol;
                    if (Math.Abs(n - o) > tol)
                        return false;
                }
                else
                {
                    double tol = config.RelTol * Math.Max(Math.Abs(n), Math.Abs(o)) + config.AbsTol;
                    if (Math.Abs(n - o) > tol)
                        return false;
                }
            }

            // Convergence succeeded
            return true;
        }
    }
}
