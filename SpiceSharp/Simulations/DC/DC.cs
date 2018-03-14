using System;
using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// DC sweep analysis
    /// </summary>
    public class DC : BaseSimulation
    {
        /// <summary>
        /// Gets the currently active DC configuration
        /// </summary>
        public DcConfiguration DcConfiguration { get; protected set; }

        /// <summary>
        /// Gets the currently active sweeps
        /// </summary>
        public NestedSweeps Sweeps { get; protected set; }

        /// <summary>
        /// Event that is called when normal iteration failed
        /// </summary>
        public event EventHandler<EventArgs> IterationFailed;

        /// <summary>
        /// Event that is called when a parameter is searched for sweeping
        /// </summary>
        public event EventHandler<DCParameterSearchEventArgs> OnParameterSearch; 

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The simulation name</param>
        public DC(Identifier name) : base(name)
        {
            var config = new DcConfiguration();
            ParameterSets.Add(config);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the simulation</param>
        /// <param name="source">The name of the swept source</param>
        /// <param name="start">The starting value</param>
        /// <param name="stop">The stopping value</param>
        /// <param name="step">The step value</param>
        public DC(Identifier name, Identifier source, double start, double stop, double step) : base(name)
        {
            var config = new DcConfiguration();
            SweepConfiguration s = new SweepConfiguration(source, start, stop, step);
            config.Sweeps.Add(s);
            ParameterSets.Add(config);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="sweeps">Sweeps</param>
        public DC(Identifier name, IEnumerable<SweepConfiguration> sweeps) : base(name)
        {
            if (sweeps == null)
                throw new ArgumentNullException(nameof(sweeps));

            var dcconfig = new DcConfiguration();
            foreach (var sweep in sweeps)
                dcconfig.Sweeps.Add(sweep);
            ParameterSets.Add(dcconfig);
        }

        /// <summary>
        /// Setup simulation
        /// </summary>
        protected override void Setup()
        {
            base.Setup();

            // Get DC configuration
            DcConfiguration = ParameterSets.Get<DcConfiguration>();

            // Get sweeps
            Sweeps = new NestedSweeps(DcConfiguration.Sweeps);
        }

        /// <summary>
        /// Execute the DC analysis
        /// </summary>
        protected override void Execute()
        {
            // Base
            base.Execute();

            var exportargs = new ExportDataEventArgs(this);

            // Setup the state
            var state = RealState;
            var dcconfig = DcConfiguration;
            var baseconfig = BaseConfiguration;
            state.Init = RealState.InitializationStates.InitJunction;
            state.UseIc = false; // UseIC is only used in transient simulations
            state.UseDc = true;
            state.Domain = RealState.DomainType.None;
            state.Gmin = baseconfig.Gmin;

            // Initialize
            Sweeps = new NestedSweeps(dcconfig.Sweeps);
            Parameter[] swept = new Parameter[Sweeps.Count];
            Parameter[] original = new Parameter[Sweeps.Count];
            int levelNeedsTemperature = -1;

            // Initialize first time
            for (int i = 0; i < dcconfig.Sweeps.Count; i++)
            {
                // Get the component to be swept
                var sweep = Sweeps[i];

                // Try finding the parameter to sweep
                var args = new DCParameterSearchEventArgs(sweep.Parameter, i);
                OnParameterSearch?.Invoke(this, args);

                // Keep track of the highest level that needs to recalculate temperature
                if (args.TemperatureNeeded)
                    levelNeedsTemperature = Math.Max(levelNeedsTemperature, i);

                if (args.Result != null)
                    swept[i] = args.Result;
                else
                {
                    if (!Circuit.Objects.Contains(sweep.Parameter))
                        throw new CircuitException("Could not find source {0}".FormatString(sweep.Parameter));
                    var component = Circuit.Objects[sweep.Parameter];

                    // Get the parameter and save it for restoring later
                    if (component is VoltageSource vsrc)
                        swept[i] = vsrc.ParameterSets.Get<Components.VoltagesourceBehaviors.BaseParameters>().DcValue;
                    else if (component is CurrentSource isrc)
                        swept[i] = isrc.ParameterSets.Get<Components.CurrentsourceBehaviors.BaseParameters>().DcValue;
                    else
                        throw new CircuitException("Invalid sweep object");
                }

                original[i] = (Parameter)swept[i].Clone();
                swept[i].Set(sweep.Initial);
            }

            // Execute temperature behaviors if necessary the first time
            if (levelNeedsTemperature >= 0)
            {
                foreach (var behavior in TemperatureBehaviors)
                    behavior.Temperature(this);
            }

            // Execute the sweeps
            int level = Sweeps.Count - 1;
            while (level >= 0)
            {
                // Fill the values with start values
                while (level < Sweeps.Count - 1)
                {
                    level++;
                    Sweeps[level].Reset();
                    swept[level].Set(Sweeps[level].CurrentValue);
                    state.Init = RealState.InitializationStates.InitJunction;
                }

                // Calculate the solution
                if (!Iterate(dcconfig.SweepMaxIterations))
                {
                    IterationFailed?.Invoke(this, EventArgs.Empty);
                    Op(baseconfig.DcMaxIterations);
                }

                // Export data
                Export(exportargs);

                // Remove all values that are greater or equal to the maximum value
                while (level >= 0 && Sweeps[level].CurrentStep >= Sweeps[level].Limit)
                    level--;

                // Go to the next step for the top level
                if (level >= 0)
                {
                    Sweeps[level].Increment();
                    swept[level].Set(Sweeps[level].CurrentValue);

                    // If temperature behavior is needed for this level or higher, run behaviors
                    if (levelNeedsTemperature >= level)
                    {
                        foreach (var behavior in TemperatureBehaviors)
                            behavior.Temperature(this);
                    }
                }
            }

            // Restore all the parameters of the swept components
            for (int i = 0; i < Sweeps.Count; i++)
                swept[i].CopyFrom(original[i]);
        }

        /// <summary>
        /// Unsetup simulation
        /// </summary>
        protected override void Unsetup()
        {
            // Clear sweeps
            Sweeps?.Clear();
            Sweeps = null;

            // Clear configuration
            DcConfiguration = null;
            base.Unsetup();
        }
    }
}
