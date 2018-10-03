﻿using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A base class for time-domain analysis.
    /// </summary>
    /// <seealso cref="SpiceSharp.Simulations.BaseSimulation" />
    public abstract class TimeSimulation : BaseSimulation
    {
        /// <summary>
        /// Gets the currently time configuration.
        /// </summary>
        /// <value>
        /// The time configuration.
        /// </value>
        public TimeConfiguration TimeConfiguration
        {
            get
            {
                if (Configurations.TryGet<TimeConfiguration>(out var config))
                {
                    return config;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the active integration method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        public IntegrationMethod Method { get; protected set; }

        /// <summary>
        /// Time-domain behaviors.
        /// </summary>
        private BehaviorList<BaseTransientBehavior> _transientBehaviors;
        private readonly List<ConvergenceAid> _initialConditions = new List<ConvergenceAid>();
        private bool _shouldReorder = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        protected TimeSimulation(string name) : base(name)
        {
            Configurations.Add(new TimeConfiguration());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        /// <param name="step">The step size.</param>
        /// <param name="final">The final time.</param>
        protected TimeSimulation(string name, double step, double final)
            : base(name)
        {
            Configurations.Add(new TimeConfiguration(step, final));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSimulation"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        /// <param name="step">The step size.</param>
        /// <param name="final">The final time.</param>
        /// <param name="maxStep">The maximum step.</param>
        protected TimeSimulation(string name, double step, double final, double maxStep)
            : base(name)
        {
            Configurations.Add(new TimeConfiguration(step, final, maxStep));
        }

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="circuit">The circuit that will be used.</param>
        /// <exception cref="ArgumentNullException">circuit</exception>
        /// <exception cref="SpiceSharp.CircuitException">
        /// {0}: No time configuration".FormatString(Name)
        /// or
        /// {0}: No integration method specified".FormatString(Name)
        /// </exception>
        protected override void Setup(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));

            // Get base behaviors
            base.Setup(circuit);

            // Get behaviors and configurations
            Method = TimeConfiguration?.Method ?? throw new CircuitException("{0}: No integration method specified".FormatString(Name));
            _transientBehaviors = SetupBehaviors<BaseTransientBehavior>(circuit.Entities);

            // Allow all transient behaviors to allocate equation elements and create states
            for (var i = 0; i < _transientBehaviors.Count; i++)
            {
                _transientBehaviors[i].GetEquationPointers(RealState.Solver);
                _transientBehaviors[i].CreateStates(Method);
            }
            Method.Setup(this);

            // TODO: Compatibility - initial conditions from nodes instead of configuration should be removed eventually
            if (BaseConfiguration.Nodesets.Count == 0)
            {
                foreach (var ns in Variables.InitialConditions)
                    _initialConditions.Add(new ConvergenceAid(ns.Key, ns.Value));
            }

            // Set up initial conditions
            foreach (var ic in TimeConfiguration.InitialConditions)
                _initialConditions.Add(new ConvergenceAid(ic.Key, ic.Value));
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        protected override void Execute()
        {
            base.Execute();

            // Apply initial conditions if they are not set for the devices (UseIc).
            if (_initialConditions.Count > 0 && !RealState.UseIc)
            {
                // Initialize initial conditions
                foreach (var ic in _initialConditions)
                    ic.Initialize(this);
                AfterLoad += LoadInitialConditions;
            }

            // Calculate the operating point of the circuit
            var state = RealState;
            state.UseIc = TimeConfiguration.UseIc;
            state.UseDc = true;
            Op(BaseConfiguration.DcMaxIterations);
            Statistics.TimePoints++;

            // Stop calculating the operating point
            state.UseIc = false;
            state.UseDc = false;
            GetDcStates();
            AfterLoad -= LoadInitialConditions;
        }

        /// <summary>
        /// Destroys the simulation.
        /// </summary>
        protected override void Unsetup()
        {
            // Remove references
            for (var i = 0; i < _transientBehaviors.Count; i++)
                _transientBehaviors[i].Unsetup(this);
            _transientBehaviors = null;

            // Destroy the integration method
            Method.Unsetup();
            Method = null;

            // Destroy the initial conditions
            AfterLoad -= LoadInitialConditions;
            foreach (var ic in _initialConditions)
                ic.Unsetup();
            _initialConditions.Clear();

            base.Unsetup();
        }

        /// <summary>
        /// Iterates to a solution for time simulations.
        /// </summary>
        /// <param name="maxIterations">The maximum number of iterations.</param>
        /// <returns>
        ///   <c>true</c> if the iterations converged to a solution; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="SpiceSharp.CircuitException">Could not find flag</exception>
        protected bool TimeIterate(int maxIterations)
        {
            var state = RealState;
            var solver = state.Solver;
            // var pass = false;
            var iterno = 0;
            var initTransient = Method.BaseTime.Equals(0.0);

            // Ignore operating condition point, just use the solution as-is
            if (state.UseIc)
            {
                state.StoreSolution();

                // Voltages are set using IC statement on the nodes
                // Internal initial conditions are calculated by the components
                Load();
                return true;
            }

            // Perform iteration
            while (true)
            {
                // Reset convergence flag
                state.IsConvergent = true;

                try
                {
                    // Load the Y-matrix and Rhs-vector for DC and transients
                    Load();
                    iterno++;
                }
                catch (CircuitException)
                {
                    iterno++;
                    Statistics.Iterations = iterno;
                    throw;
                }

                // Preordering is already done in the operating point calculation

                if (state.Init == InitializationModes.Junction || initTransient)
                    _shouldReorder = true;

                // Reorder
                if (_shouldReorder)
                {
                    Statistics.ReorderTime.Start();
                    solver.OrderAndFactor();
                    Statistics.ReorderTime.Stop();
                    _shouldReorder = false;
                }
                else
                {
                    // Decompose
                    Statistics.DecompositionTime.Start();
                    var success = solver.Factor();
                    Statistics.DecompositionTime.Stop();

                    if (!success)
                    {
                        _shouldReorder = true;
                        continue;
                    }
                }

                // The current solution becomes the old solution
                state.StoreSolution();

                // Solve the equation
                Statistics.SolveTime.Start();
                solver.Solve(state.Solution);
                Statistics.SolveTime.Stop();

                // Reset ground nodes
                state.Solution[0] = 0.0;
                state.OldSolution[0] = 0.0;

                // Exceeded maximum number of iterations
                if (iterno > maxIterations)
                {
                    Statistics.Iterations += iterno;
                    return false;
                }

                if (state.IsConvergent && iterno != 1)
                    state.IsConvergent = IsConvergent();
                else
                    state.IsConvergent = false;

                if (initTransient)
                {
                    initTransient = false;
                    if (iterno <= 1)
                        _shouldReorder = true;
                    state.Init = InitializationModes.Float;
                }
                else
                {
                    switch (state.Init)
                    {
                        case InitializationModes.Float:
                            // TimeIterate is only used during simulation, so the next part is never reached
                            /* if (state.UseDc && state.HadNodeSet)
                            {
                                if (pass)
                                    state.IsConvergent = false;
                                pass = false;
                            } */

                            if (state.IsConvergent)
                            {
                                Statistics.Iterations += iterno;
                                return true;
                            }

                            break;

                        case InitializationModes.Junction:
                            state.Init = InitializationModes.Fix;
                            _shouldReorder = true;
                            break;

                        case InitializationModes.Fix:
                            if (state.IsConvergent)
                                state.Init = InitializationModes.Float;
                            // pass = true;
                            break;

                        case InitializationModes.None:
                            state.Init = InitializationModes.Float;
                            break;

                        default:
                            Statistics.Iterations += iterno;
                            throw new CircuitException("Could not find flag");
                    }
                }
            }
        }

        /// <summary>
        /// Initializes all transient behaviors to assume that the current solution is the DC solution.
        /// </summary>
        protected virtual void GetDcStates()
        {
            for (var i = 0; i < _transientBehaviors.Count; i++)
                _transientBehaviors[i].GetDcState(this);
            Method.Initialize(this);
        }

        /// <summary>
        /// Applies nodesets.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected void LoadInitialConditions(object sender, LoadStateEventArgs e)
        {
            foreach (var ic in _initialConditions)
                ic.Aid();
        }

        /// <summary>
        /// Load all behaviors for time simulation.
        /// </summary>
        protected override void LoadBehaviors()
        {
            base.LoadBehaviors();

            // Not calculating DC behavior
            if (!RealState.UseDc)
            {
                for (var i = 0; i < _transientBehaviors.Count; i++)
                    _transientBehaviors[i].Transient(this);
            }
        }
    }
}
