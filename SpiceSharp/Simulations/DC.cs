using System;
using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Describes a DC simulation
    /// </summary>
    public class DC : Simulation
    {
        /// <summary>
        /// Extended configuration for DC analysis
        /// </summary>
        public class Configuration : SimulationConfiguration
        {
            /// <summary>
            /// The maximum number of iterations used each step
            /// </summary>
            public int MaxIterations = 50;
        }

        /// <summary>
        /// A delegate for
        /// </summary>
        /// <param name="sender">The object sending the event</param>
        /// <param name="ckt">The circuit</param>
        public delegate void IterationFailedEventHandler(object sender, Circuit ckt);

        /// <summary>
        /// Event that is called when normal iteration failed
        /// </summary>
        public event IterationFailedEventHandler IterationFailed;

        /// <summary>
        /// Easy access to the configuration
        /// </summary>
        protected Configuration MyConfig { get { return (Configuration)Config; } }

        /// <summary>
        /// A class that describes a job
        /// </summary>
        public class Sweep : Parameterized
        {
            /// <summary>
            /// Starting value
            /// </summary>
            [SpiceName("start"), SpiceInfo("The starting value")]
            public double Start { get; private set; }

            /// <summary>
            /// Ending value
            /// </summary>
            [SpiceName("stop"), SpiceInfo("The stopping value")]
            public double Stop { get; private set; }

            /// <summary>
            /// Value step
            /// </summary>
            [SpiceName("step"), SpiceInfo("The step")]
            public double Step { get; private set; }

            /// <summary>
            /// The number of steps
            /// </summary>
            [SpiceName("steps"), SpiceName("n"), SpiceInfo("The number of steps")]
            public int Limit { get; private set; }

            /// <summary>
            /// The name of the source being varied
            /// </summary>
            [SpiceName("source"), SpiceInfo("The name of the swept source")]
            public string ComponentName { get; private set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="name">The name of the source to sweep</param>
            /// <param name="start">The starting value</param>
            /// <param name="stop">The stopping value</param>
            /// <param name="step">The step value</param>
            public Sweep(string name, object start, object stop, object step)
                : base(null)
            {
                Set("start", start);
                Set("stop", stop);
                Set("step", step);

                ComponentName = name;
                if (Math.Sign(Step) * (Stop - Start) < 0)
                {
                    // Only do single point
                    Limit = 0;
                    Stop = Start;
                }
                else
                    Limit = (int)Math.Floor((Stop - Start) / Step + 0.25);
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
        /// <param name="config">The configuration</param>
        public DC(string name, Configuration config = null)
            : base(name, config ?? new Configuration())
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
        public DC(string name, string source, object start, object stop, object step)
            : base(name, new Configuration())
        {
            Sweep s = new Sweep(source, start, stop, step);
            Sweeps.Add(s);
        }

        /// <summary>
        /// Execute the DC simulation
        /// </summary>
        /// <param name="ckt">The circuit</param>
        /// <param name="reset">Restart the circuit if true</param>
        public override void Execute(Circuit ckt)
        {
            // Setup the state
            var state = ckt.State;
            var rstate = state.Real;
            state.UseIC = false; // UseIC is only used in transient simulations
            state.UseDC = true;
            state.UseSmallSignal = false;
            state.Domain = CircuitState.DomainTypes.None;

            // Initialize
            CircuitComponent[] components = new CircuitComponent[Sweeps.Count];
            Parameter<double>[] parameters = new Parameter<double>[Sweeps.Count];
            int[] values = new int[Sweeps.Count];

            // Initialize first time
            for (int i = 0; i < Sweeps.Count; i++)
            {
                // Get the component to be swept
                var sweep = Sweeps[i];
                if (!ckt.Components.Contains(sweep.ComponentName))
                    throw new CircuitException($"Could not find source {sweep.ComponentName}");
                components[i] = ckt.Components[sweep.ComponentName];

                // Get the parameter and save it for restoring later
                parameters[i] = (Parameter<double>)components[i].GetParameter<double>("dc").Clone();

                // Start with the original values
                components[i].Set("dc", sweep.Start);
                values[i] = 0;
            }

            // Execute the sweeps
            int level = Sweeps.Count - 1;
            while (level >= 0)
            {
                // Fill the values with start values
                while (level < Sweeps.Count - 1)
                {
                    level++;
                    values[level] = 0;
                    components[level].Set("dc", Sweeps[level].Start);
                }

                // Calculate the solution
                if (!this.Iterate(ckt, MyConfig.MaxIterations))
                {
                    IterationFailed?.Invoke(this, ckt);
                    this.Op(ckt, MyConfig.MaxIterations);
                }

                // Export data
                Export(ckt);

                // Remove all values that are greater or equal to the maximum value
                while (level >= 0 && values[level] >= Sweeps[level].Limit)
                    level--;

                // Go to the next step for the top level
                if (level >= 0)
                {
                    values[level]++;
                    double newvalue = Sweeps[level].Start + values[level] * Sweeps[level].Step;
                    components[level].Set("dc", newvalue);
                }
            }
            
            // Restore all the parameters of the swept components
            for (int i = 0; i < Sweeps.Count; i++)
            {
                components[i].GetParameter<double>("dc").CopyFrom(parameters[i]);
            }
        }
    }
}
