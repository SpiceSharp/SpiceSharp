using System;
using System.Collections.Generic;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components;

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
        public DCConfiguration DCConfiguration { get; protected set; }

        /// <summary>
        /// Gets the currently active sweeps
        /// </summary>
        public NestedSweeps Sweeps { get; protected set; } = null;

        /// <summary>
        /// Event that is called when normal iteration failed
        /// </summary>
        public event IterationFailedEventHandler IterationFailed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The simulation name</param>
        public DC(Identifier name) : base(name)
        {
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
            var config = new DCConfiguration();
            Sweep s = new Sweep(source, start, stop, step);
            config.Sweeps.Add(s);
            Parameters.Add(config);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="sweeps">Sweeps</param>
        public DC(Identifier name, IEnumerable<Sweep> sweeps) : base(name)
        {
            if (sweeps == null)
                throw new ArgumentNullException(nameof(sweeps));

            var dcconfig = new DCConfiguration();
            foreach (var sweep in sweeps)
                dcconfig.Sweeps.Add(sweep);
            Parameters.Add(dcconfig);
        }

        /// <summary>
        /// Setup simulation
        /// </summary>
        protected override void Setup()
        {
            base.Setup();

            // Get DC configuration
            DCConfiguration = Parameters.Get<DCConfiguration>();

            // Get sweeps
            Sweeps = new NestedSweeps(DCConfiguration.Sweeps);
        }

        /// <summary>
        /// Execute the DC analysis
        /// </summary>
        protected override void Execute()
        {
            // Base
            base.Execute();

            var exportargs = new ExportDataEventArgs(State);
            var circuit = Circuit;

            // Setup the state
            var state = State;
            var dcconfig = DCConfiguration;
            var baseconfig = BaseConfiguration;
            state.Init = State.InitFlags.InitJct;
            state.Initialize(circuit);
            state.UseIC = false; // UseIC is only used in transient simulations
            state.UseDC = true;
            state.UseSmallSignal = false;
            state.Domain = State.DomainTypes.None;
            state.Gmin = baseconfig.Gmin;

            // Initialize
            Sweeps = new NestedSweeps(dcconfig.Sweeps);
            Parameter[] swept = new Parameter[Sweeps.Count];
            Parameter[] original = new Parameter[Sweeps.Count];

            // Initialize first time
            for (int i = 0; i < dcconfig.Sweeps.Count; i++)
            {
                // Get the component to be swept
                var sweep = Sweeps[i];
                if (!Circuit.Objects.Contains(sweep.Parameter))
                    throw new CircuitException($"Could not find source {sweep.Parameter}");
                var component = Circuit.Objects[sweep.Parameter];

                // Get the parameter and save it for restoring later
                if (component is Voltagesource vsrc)
                    swept[i] = vsrc.Parameters.Get<Components.VoltagesourceModel.BaseParameters>().VSRCdcValue;
                else if (component is Currentsource isrc)
                    swept[i] = isrc.Parameters.Get<Components.CurrentsourceBehaviors.BaseParameters>().ISRCdcValue;
                else
                    throw new CircuitException("Invalid sweep object");
                original[i] = (Parameter)swept[i].Clone();
                swept[i].Set(sweep.Initial);
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
                    state.Init = State.InitFlags.InitJct;
                }

                // Calculate the solution
                if (!Iterate(dcconfig.SweepMaxIterations))
                {
                    IterationFailedEventArgs args = new IterationFailedEventArgs();
                    IterationFailed?.Invoke(this, args);
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
            DCConfiguration = null;
            base.Unsetup();
        }
    }
}
