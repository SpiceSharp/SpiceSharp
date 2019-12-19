using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Class that implements a DC sweep analysis.
    /// </summary>
    /// <seealso cref="BiasingSimulation" />
    public class DC : BiasingSimulation
    {
        /// <summary>
        /// Occurs when iterating to a solution has failed.
        /// </summary>
        public event EventHandler<EventArgs> IterationFailed;

        /// <summary>
        /// The enumerators
        /// </summary>
        private IEnumerator<double>[] _enumerators;

        /// <summary>
        /// Initializes a new instance of the <see cref="DC"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        public DC(string name) : base(name)
        {
            Configurations.Add(new DCConfiguration());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DC"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <param name="source">The source name.</param>
        /// <param name="start">The starting value.</param>
        /// <param name="stop">The stop value.</param>
        /// <param name="step">The step value.</param>
        public DC(string name, string source, double start, double stop, double step) : base(name)
        {
            var config = new DCConfiguration();
            config.Sweeps.Add(new SourceSweep(source, new LinearSweep(start, stop, step)));
            Configurations.Add(config);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DC"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <param name="sweeps">The sweeps.</param>
        public DC(string name, IEnumerable<ISweep> sweeps) : base(name)
        {
            sweeps.ThrowIfNull(nameof(sweeps));

            var dcconfig = new DCConfiguration();
            foreach (var sweep in sweeps)
                dcconfig.Sweeps.Add(sweep);
            Configurations.Add(dcconfig);
        }

        /// <summary>
        /// Executes the simulation.
        /// </summary>
        protected override void Execute()
        {
            // Base
            base.Execute();

            var exportargs = new ExportDataEventArgs(this);

            // Setup the state
            var dcconfig = Configurations.GetValue<DCConfiguration>().ThrowIfNull("dc configuration");
            BiasingState.Init = InitializationModes.Junction;
            BiasingState.UseIc = false; // UseIC is only used in transient simulations
            BiasingState.UseDc = true;

            // Initialize
            var sweeps = dcconfig.Sweeps.ToArray();
            _enumerators = new IEnumerator<double>[dcconfig.Sweeps.Count];
            for (var i = 0; i < sweeps.Length; i++)
            {
                sweeps[i].CalculateDefaults();
                _enumerators[i] = sweeps[i].CreatePoints(this);
                if (!_enumerators[i].MoveNext())
                    throw new SpiceSharpException(Properties.Resources.Simulations_DC_NoSweepPoints.FormatString(sweeps[i].Name));
            }

            // Execute the sweeps
            var level = sweeps.Length - 1;
            while (level >= 0)
            {
                // Fill the values up again by resetting
                while (level < sweeps.Length - 1)
                {
                    level++;
                    _enumerators[level] = sweeps[level].CreatePoints(this);
                    if (!_enumerators[level].MoveNext())
                        throw new SpiceSharpException(Properties.Resources.Simulations_DC_NoSweepPoints.FormatString(sweeps[level].Name));
                    BiasingState.Init = InitializationModes.Junction;
                }

                // Calculate the solution
                if (!Iterate(dcconfig.SweepMaxIterations))
                {
                    IterationFailed?.Invoke(this, EventArgs.Empty);
                    Op(DcMaxIterations);
                }

                // Export data
                OnExport(exportargs);

                // Remove all values that are greater or equal to the maximum value
                while (level >= 0 && !_enumerators[level].MoveNext())
                    level--;
                if (level < 0)
                    break;
            }
        }

        /// <summary>
        /// Destroys the simulation.
        /// </summary>
        protected override void Unsetup()
        {
            // Clear configuration
            base.Unsetup();
        }

        /// <summary>
        /// Gets the sweep values.
        /// </summary>
        /// <returns>The sweep values.</returns>
        public double[] GetSweepValues()
        {
            if (_enumerators == null)
                return null;
            var result = new double[_enumerators.Length];
            for (var i = 0; i < _enumerators.Length; i++)
                result[i] = _enumerators[i].Current;
            return result;
        }
    }
}
