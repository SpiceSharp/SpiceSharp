using System;
using System.Collections.Generic;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// DC sweep analysis
    /// </summary>
    public class DC : Simulation<DC>
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
        public class Sweep : Parameterized<Sweep>
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
                    else
                        return (int)Math.Floor((Stop - Start) / Step + 0.25);
                }
            }

            /// <summary>
            /// The name of the source being varied
            /// </summary>
            [SpiceName("source"), SpiceInfo("The name of the swept source")]
            public CircuitIdentifier ComponentName { get; set; }

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
            public Sweep(CircuitIdentifier name, double start, double stop, double step) : base()
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

        private List<CircuitObjectBehaviorLoad> loadbehaviours;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The simulation name</param>
        /// <param name="config">The configuration</param>
        public DC(string name) : base(name)
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
        public DC(string name, CircuitIdentifier source, double start, double stop, double step) : base(name)
        {
            Sweep s = new Sweep(source, start, stop, step);
            Sweeps.Add(s);
        }

        /// <summary>
        /// Initialize the circuit
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Initialize(Circuit ckt)
        {
            base.Initialize(ckt);

            // Get behaviours for DC analysis
            loadbehaviours = Behaviors.Behaviors.CreateBehaviors<CircuitObjectBehaviorLoad>(ckt);
        }


        /// <summary>
        /// Execute the DC simulation
        /// </summary>
        /// <param name="ckt">The circuit</param>
        protected override void Execute()
        {
            var ckt = this.Circuit;
            // Setup the state
            var state = ckt.State;
            var rstate = state;
            var config = CurrentConfig;
            state.Init = CircuitState.InitFlags.InitJct;
            state.Initialize(ckt);
            state.UseIC = false; // UseIC is only used in transient simulations
            state.UseDC = true;
            state.UseSmallSignal = false;
            state.Domain = CircuitState.DomainTypes.None;
            state.Gmin = config.Gmin;

            // Initialize
            IParameterized[] components = new IParameterized[Sweeps.Count];
            Parameter[] parameters = new Parameter[Sweeps.Count];

            // Initialize first time
            for (int i = 0; i < Sweeps.Count; i++)
            {
                // Get the component to be swept
                var sweep = Sweeps[i];
                if (!this.Circuit.Objects.Contains(sweep.ComponentName))
                    throw new CircuitException($"Could not find source {sweep.ComponentName}");
                components[i] = (IParameterized)this.Circuit.Objects[sweep.ComponentName];

                // Get the parameter and save it for restoring later
                parameters[i] = (Parameter)GetDcParameter(components[i]).Clone();

                // Start with the original values
                sweep.SetCurrentStep(0);
                components[i].Set("dc", sweep.CurrentValue);
            }

            Initialize(ckt);

            // Execute the sweeps
            int level = Sweeps.Count - 1;
            while (level >= 0)
            {
                // Fill the values with start values
                while (level < Sweeps.Count - 1)
                {
                    level++;
                    Sweeps[level].SetCurrentStep(0);
                    components[level].Set("dc", Sweeps[level].CurrentValue);
                }

                // Calculate the solution
                if (!ckt.Iterate(loadbehaviours, config, config.SweepMaxIterations))
                {
                    IterationFailed?.Invoke(this, ckt);
                    ckt.Op(loadbehaviours, config, config.DcMaxIterations);
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
                    components[level].Set("dc", Sweeps[level].CurrentValue);
                }
            }

            // Restore all the parameters of the swept components
            for (int i = 0; i < Sweeps.Count; i++)
                SetDcParameter(components[i], parameters[i]);

            Finalize(ckt);
        }

        /// <summary>
        /// Get the DC parameter
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private Parameter GetDcParameter(IParameterized obj)
        {
            if (obj is Voltagesource vsource)
                return vsource.VSRCdcValue;
            if (obj is Currentsource isource)
                return isource.ISRCdcValue;
            return null;
        }

        /// <summary>
        /// Copy the DC parameter back to the object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="par"></param>
        private void SetDcParameter(IParameterized obj, Parameter par)
        {
            if (obj is Voltagesource vsource)
                vsource.VSRCdcValue.CopyFrom(par);
            if (obj is Currentsource isource)
                isource.ISRCdcValue.CopyFrom(par);
        }
    }
}
