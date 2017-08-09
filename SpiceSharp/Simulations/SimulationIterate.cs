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
        /// <param name="sim">The simulation</param>
        /// <param name="ckt">The circuit</param>
        /// <param name="maxiter">The maximum number of iterations</param>
        public static void Op(this Simulation sim, Circuit ckt, int maxiter)
        {
            // Create the current SimulationState
            var state = ckt.State;
            state.Init = CircuitState.InitFlags.InitJct;

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
        /// Iterate to a solution
        /// </summary>
        /// <param name="sim">The simulation</param>
        /// <param name="ckt">The circuit</param>
        /// <param name="maxiter">The maximum number of iterations</param>
        /// <returns></returns>
        public static bool Iterate(this Simulation sim, Circuit ckt, int maxiter)
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
        public static void AcIterate(this Simulation sim, Circuit ckt)
        {
            // Initialize the circuit
            if (!ckt.State.Initialized)
                ckt.State.Initialize(ckt);

            ckt.State.IsCon = true;

            // Load AC
            ckt.State.Complex.Clear();
            foreach (var c in ckt.Components)
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
        private static bool IsConvergent(this Simulation sim, Circuit ckt)
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

            // Convergence succeeded
            return true;
        }
    }
}
