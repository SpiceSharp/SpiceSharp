﻿using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Class that implements a DC sweep analysis.
    /// </summary>
    /// <seealso cref="SpiceSharp.Simulations.BaseSimulation" />
    public class DC : BaseSimulation
    {
        /// <summary>
        /// Gets the currently active DC configuration.
        /// </summary>
        /// <value>
        /// The dc configuration.
        /// </value>
        public DcConfiguration DcConfiguration
        {
            get
            {
                if (Configurations.TryGet<DcConfiguration>(out var config))
                {
                    return config;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the currently active sweeps.
        /// </summary>
        /// <value>
        /// The sweeps.
        /// </value>
        public NestedSweeps Sweeps { get; protected set; }

        /// <summary>
        /// Occurs when iterating to a solution has failed.
        /// </summary>
        public event EventHandler<EventArgs> IterationFailed;

        /// <summary>
        /// Occurs when a parameter for sweeping is searched.
        /// </summary>
        public event EventHandler<DCParameterSearchEventArgs> OnParameterSearch;

        /// <summary>
        /// Initializes a new instance of the <see cref="DC"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        public DC(string name) : base(name)
        {
            Configurations.Add(new DcConfiguration());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DC"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        /// <param name="source">The source identifier.</param>
        /// <param name="start">The starting value.</param>
        /// <param name="stop">The stop value.</param>
        /// <param name="step">The step value.</param>
        public DC(string name, string source, double start, double stop, double step) : base(name)
        {
            var config = new DcConfiguration();
            var s = new SweepConfiguration(source, start, stop, step);
            config.Sweeps.Add(s);
            Configurations.Add(config);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DC"/> class.
        /// </summary>
        /// <param name="name">The identifier of the simulation.</param>
        /// <param name="sweeps">The sweeps.</param>
        /// <exception cref="ArgumentNullException">sweeps</exception>
        public DC(string name, IEnumerable<SweepConfiguration> sweeps) : base(name)
        {
            if (sweeps == null)
                throw new ArgumentNullException(nameof(sweeps));

            var dcconfig = new DcConfiguration();
            foreach (var sweep in sweeps)
                dcconfig.Sweeps.Add(sweep);
            Configurations.Add(dcconfig);
        }

        /// <summary>
        /// Set up the simulation.
        /// </summary>
        /// <param name="circuit">The circuit that will be used.</param>
        protected override void Setup(Circuit circuit)
        {
            base.Setup(circuit);

            // Get sweeps
            Sweeps = new NestedSweeps(DcConfiguration.Sweeps);
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        /// <exception cref="SpiceSharp.CircuitException">
        /// Could not find source {0}".FormatString(sweep.Parameter)
        /// or
        /// Invalid sweep object
        /// </exception>
        protected override void Execute()
        {
            // Base
            base.Execute();

            var exportargs = new ExportDataEventArgs(this);

            // Setup the state
            var state = RealState;
            var dcconfig = DcConfiguration;
            var baseconfig = BaseConfiguration;
            state.Init = InitializationModes.Junction;
            state.UseIc = false; // UseIC is only used in transient simulations
            state.UseDc = true;

            // Initialize
            Sweeps = new NestedSweeps(dcconfig.Sweeps);
            var swept = new Parameter<double>[Sweeps.Count];
            var original = new Parameter<double>[Sweeps.Count];
            var levelNeedsTemperature = -1;

            // Initialize first time
            for (var i = 0; i < dcconfig.Sweeps.Count; i++)
            {
                // Get the component to be swept
                var sweep = Sweeps[i];

                // Try finding the parameter to sweep
                var args = new DCParameterSearchEventArgs(sweep.Parameter, i);
                OnParameterSearch?.Invoke(this, args);

                if (args.Result != null)
                {
                    swept[i] = args.Result;

                    // Keep track of the highest level that needs to recalculate temperature
                    if (args.TemperatureNeeded)
                        levelNeedsTemperature = Math.Max(levelNeedsTemperature, i);
                }
                else
                {
                    // Get entity parameters
                    if (!EntityBehaviors.ContainsKey(sweep.Parameter))
                        throw new CircuitException("Could not find source {0}".FormatString(sweep.Parameter));
                    var eb = EntityParameters[sweep.Parameter];

                    // Check for a Voltage source or Current source parameters
                    if (eb.TryGet<Components.VoltageSourceBehaviors.BaseParameters>(out var pvsrc))
                        swept[i] = pvsrc.DcValue;
                    else if (eb.TryGet<Components.CurrentSourceBehaviors.BaseParameters>(out var pisrc))
                        swept[i] = pisrc.DcValue;
                    else
                        throw new CircuitException("Invalid sweep object");
                }

                original[i] = (Parameter<double>) swept[i].Clone();
                swept[i].Value = sweep.Initial;
            }

            // Execute temperature behaviors if necessary the first time
            if (levelNeedsTemperature >= 0)
                Temperature();

            // Execute the sweeps
            var level = Sweeps.Count - 1;
            while (level >= 0)
            {
                // Fill the values with start values
                while (level < Sweeps.Count - 1)
                {
                    level++;
                    Sweeps[level].Reset();
                    swept[level].Value = Sweeps[level].CurrentValue;
                    state.Init = InitializationModes.Junction;
                }

                // Calculate the solution
                if (!Iterate(dcconfig.SweepMaxIterations))
                {
                    IterationFailed?.Invoke(this, EventArgs.Empty);
                    Op(baseconfig.DcMaxIterations);
                }

                // Export data
                OnExport(exportargs);

                // Remove all values that are greater or equal to the maximum value
                while (level >= 0 && Sweeps[level].CurrentStep >= Sweeps[level].Limit)
                    level--;

                // Go to the next step for the top level
                if (level >= 0)
                {
                    Sweeps[level].Increment();
                    swept[level].Value = Sweeps[level].CurrentValue;

                    // If temperature behavior is needed for this level or higher, run behaviors
                    if (levelNeedsTemperature >= level)
                        Temperature();
                }
            }

            // Restore all the parameters of the swept components
            for (var i = 0; i < Sweeps.Count; i++)
                swept[i].CopyFrom(original[i]);
        }

        /// <summary>
        /// Destroys the simulation.
        /// </summary>
        protected override void Unsetup()
        {
            // Clear sweeps
            Sweeps?.Clear();
            Sweeps = null;

            // Clear configuration
            base.Unsetup();
        }
    }
}
