using System;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This static class contains the methods for:
    /// - Op(): Finds the operating point for the circuit
    /// - Iterate(): Tries to converge to a solution using the current circuit
    /// </summary>
    public static class SimulationIterate
    {
        /// <summary>
        /// Calculate the operating point of the circuit
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <param name="mode">The mode</param>
        /// <param name="iterlim">Maximum number of iterations</param>
        public static void Op(this Simulation sim, Circuit ckt, int mode, int maxiter)
        {
            // Create the current SimulationState
            var state = ckt.State;
            state.Init = CircuitState.InitFlags.InitJct;

            state.Mode = mode;
            if (!sim.Config.NoOpIter)
            {
                if (sim.Iterate(ckt, maxiter))
                    return;
            }

            // No convergence
            // try Gmin stepping
            if (sim.Config.NumGminSteps > 1)
            {
                state.Init = CircuitState.InitFlags.InitJct;
                CircuitWarning.Warning(sim, "Starting Gmin stepping");
                state.Gmin = sim.Config.Gmin;
                for (int i = 0; i < sim.Config.NumGminSteps; i++)
                    state.Gmin *= 10.0;
                for (int i = 0; i <= sim.Config.NumGminSteps; i++)
                {
                    state.IsCon = true;
                    if (!sim.Iterate(ckt, maxiter))
                    {
                        state.Gmin = 0.0;
                        CircuitWarning.Warning(sim, "Gmin step failed");
                        break;
                    }
                    state.Gmin /= 10.0;
                    state.Init = CircuitState.InitFlags.InitFloat;
                }
                state.Gmin = 0.0;
                if (sim.Iterate(ckt, maxiter))
                    return;
            }

            // No we'll try source stepping
            if (sim.Config.NumSrcSteps > 1)
            {
                state.Init = CircuitState.InitFlags.InitJct;
                CircuitWarning.Warning(sim, "Starting source stepping");
                for (int i = 0; i <= sim.Config.NumSrcSteps; i++)
                {
                    state.SrcFact = i / (double)sim.Config.NumSrcSteps;
                    if (!sim.Iterate(ckt, maxiter))
                    {
                        state.SrcFact = 1.0;
                        // ckt.CurrentAnalysis = AnalysisType.DoingTran;
                        CircuitWarning.Warning(sim, "Source stepping failed");
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
        /// Iterate the circuit and try to converge to a solution
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <returns></returns>
        public static bool Iterate(this Simulation sim, Circuit ckt, int maxiter)
        {
            var state = ckt.State;
            bool pass = false;
            int iterno = 0;

            // Initialize the state of the circuit
            if (!state.Initialized)
                state.Initialize(ckt);

            // Ignore operating condition point, just use the solution as-is
            if (ckt.State.UseIC && ckt.State.Domain == CircuitState.DomainTypes.Time)
            {
                state.StoreSolution();

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

                if (state.Init != CircuitState.InitFlags.InitPred)
                {
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
                    ckt.State.Solve();
                    ckt.Statistics.SolveTime.Stop();

                    // Exceeded maximum number of iterations
                    if (iterno > maxiter)
                    {
                        ckt.Statistics.NumIter += iterno;
                        return false;
                    }

                    if (state.IsCon && iterno != 1)
                        state.IsCon = sim.IsConvergent(ckt);
                    else
                        state.IsCon = false;
                }

                switch (state.Init)
                {
                    case CircuitState.InitFlags.InitFloat:
                        if (state.IsDc && state.HadNodeset)
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
                    case CircuitState.InitFlags.InitPred:
                        state.Init = CircuitState.InitFlags.InitFloat;
                        break;

                    default:
                        ckt.Statistics.NumIter += iterno;
                        throw new CircuitException("Could not find flag");
                }

                // We need to do another iteration, swap solutions with the old solution
                ckt.State.StoreSolution();
            }
        }

        /// <summary>
        /// Check if the current iteration is converging
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <returns></returns>
        private static bool IsConvergent(this Simulation sim, Circuit ckt)
        {
            // Check convergence for each node
            for (int i = 0; i < ckt.Nodes.Count; i++)
            {
                var node = ckt.Nodes[i];
                double n = ckt.State.Solution[node.Index];
                double o = ckt.State.OldSolution[node.Index];
                if (node.Type == CircuitNode.NodeType.Voltage)
                {
                    double tol = sim.Config.RelTol * Math.Max(Math.Abs(n), Math.Abs(o)) + sim.Config.VoltTol;
                    if (Math.Abs(n - o) > tol)
                    {
                        return false;
                    }
                }
                else
                {
                    double tol = sim.Config.RelTol * Math.Max(Math.Abs(n), Math.Abs(o)) + sim.Config.AbsTol;
                    if (Math.Abs(n - o) > tol)
                    {
                        // Convergence failed
                        // ckt.TroubleNode = i;
                        return false;
                    }
                }
            }

            // Give each component the chance to check convergence
            foreach (var c in ckt.Components)
            {
                if (!c.IsConvergent(ckt))
                    return false;
            }

            // Convergence succeeded
            return true;
        }
    }
}
