using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// DC sweep analysis
    /// </summary>
    public class DC : BaseSimulation
    {
        /// <summary>
        /// A delegate for when an iteration failed
        /// </summary>
        /// <param name="sender">The object sending the event</param>
        /// <param name="ckt">The circuit</param>
        public delegate void IterationFailedEventHandler(object sender, Circuit ckt);

        /// <summary>
        /// Event that is called when normal iteration failed
        /// </summary>
        public event IterationFailedEventHandler IterationFailed;

        /// <summary>
        /// A class that describes a job
        /// </summary>
        public class Sweep
        {
            /// <summary>
            /// Starting value
            /// </summary>
            [SpiceName("start"), SpiceInfo("The starting value")]
            public double Start { get; set; }

            /// <summary>
            /// Ending value
            /// </summary>
            [SpiceName("stop"), SpiceInfo("The stopping value")]
            public double Stop { get; set; }

            /// <summary>
            /// Value step
            /// </summary>
            [SpiceName("step"), SpiceInfo("The step")]
            public double Step { get; set; }

            /// <summary>
            /// The number of steps
            /// </summary>
            [SpiceName("steps"), SpiceName("n"), SpiceInfo("The number of steps")]
            public int Limit
            {
                get
                {
                    if (Math.Sign(Step) * (Stop - Start) < 0)
                        return 0;
                    return (int)Math.Floor((Stop - Start) / Step + 0.25);
                }
            }

            /// <summary>
            /// The name of the source being varied
            /// </summary>
            [SpiceName("source"), SpiceInfo("The name of the swept source")]
            public Identifier ComponentName { get; set; }

            /// <summary>
            /// Get the current value
            /// </summary>
            public double CurrentValue { get; private set; }

            /// <summary>
            /// Get the current step index
            /// </summary>
            public int CurrentStep { get; private set; }

            /// <summary>
            /// Calculate the new step value
            /// </summary>
            /// <param name="index">The step index</param>
            public void SetCurrentStep(int index)
            {
                CurrentStep = index;
                CurrentValue = Start + index * Step;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="name">The name of the source to sweep</param>
            /// <param name="start">The starting value</param>
            /// <param name="stop">The stopping value</param>
            /// <param name="step">The step value</param>
            public Sweep(Identifier name, double start, double stop, double step) : base()
            {
                ComponentName = name;
                Start = start;
                Stop = stop;
                Step = step;
            }
        }

        /// <summary>
        /// Gets the list of sweeps that need to be executed
        /// </summary>
        public List<Sweep> Sweeps { get; } = new List<Sweep>();

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
            Sweep s = new Sweep(source, start, stop, step);
            Sweeps.Add(s);
        }

        /// <summary>
        /// Execute the DC analysis
        /// </summary>
        protected override void Execute()
        {
            // Base
            base.Execute();

            var ckt = Circuit;

            // Setup the state
            var state = ckt.State;
            var rstate = state;
            var config = CurrentConfig;
            state.Init = State.InitFlags.InitJct;
            state.Initialize(ckt);
            state.UseIC = false; // UseIC is only used in transient simulations
            state.UseDC = true;
            state.UseSmallSignal = false;
            state.Domain = State.DomainTypes.None;
            state.Gmin = config.Gmin;

            // Initialize
            Parameter[] swept = new Parameter[Sweeps.Count];
            Parameter[] original = new Parameter[Sweeps.Count];

            // Initialize first time
            for (int i = 0; i < Sweeps.Count; i++)
            {
                // Get the component to be swept
                var sweep = Sweeps[i];
                if (!Circuit.Objects.Contains(sweep.ComponentName))
                    throw new CircuitException($"Could not find source {sweep.ComponentName}");
                var component = Circuit.Objects[sweep.ComponentName];

                // Get the parameter and save it for restoring later
                if (component is Voltagesource vsrc)
                    swept[i] = vsrc.Parameters.Get<Components.VSRC.BaseParameters>().VSRCdcValue;
                else if (component is Currentsource isrc)
                    swept[i] = isrc.Parameters.Get<Components.ISRC.BaseParameters>().ISRCdcValue;
                else
                    throw new CircuitException("Invalid sweep object");
                original[i] = (Parameter)swept[i].Clone();
                swept[i].Set(sweep.Start);

                // Start with the original values
                sweep.SetCurrentStep(0);
            }

            // Execute the sweeps
            int level = Sweeps.Count - 1;
            while (level >= 0)
            {
                // Fill the values with start values
                while (level < Sweeps.Count - 1)
                {
                    level++;
                    Sweeps[level].SetCurrentStep(0);
                    swept[level].Set(Sweeps[level].CurrentValue);
                    state.Init = State.InitFlags.InitJct;
                }

                // Calculate the solution
                if (!Iterate(ckt, config.SweepMaxIterations))
                {
                    IterationFailed?.Invoke(this, ckt);
                    Op(ckt, config.DcMaxIterations);
                }

                // Export data
                Export(ckt);

                // Remove all values that are greater or equal to the maximum value
                while (level >= 0 && Sweeps[level].CurrentStep >= Sweeps[level].Limit)
                    level--;

                // Go to the next step for the top level
                if (level >= 0)
                {
                    Sweeps[level].SetCurrentStep(Sweeps[level].CurrentStep + 1);
                    swept[level].Set(Sweeps[level].CurrentValue);
                }
            }

            // Restore all the parameters of the swept components
            for (int i = 0; i < Sweeps.Count; i++)
                swept[i].CopyFrom(original[i]);
        }

        /// <summary>
        /// Create a getter for this type of simulation
        /// The simulation will determine which getter is returned if multiple behaviors implement a getter by the same name
        /// </summary>
        /// <param name="name">The identifier of the entity</param>
        /// <param name="parameter">The parameter name</param>
        /// <returns></returns>
        public override Func<double> CreateGetter(Identifier name, string parameter)
        {
            var eb = pool.GetEntityBehaviors(name) ?? throw new CircuitException($"{Name}: Could not find behaviors of {name}");

            // Most logical place to look for AC analysis: AC behaviors
            Func<double> getter = eb.Get<LoadBehavior>().CreateGetter(Circuit.State, parameter);
            return getter;
        }
    }
}
