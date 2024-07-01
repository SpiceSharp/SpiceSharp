using SpiceSharp.ParameterSets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Class that implements a DC sweep analysis.
    /// </summary>
    /// <seealso cref="BiasingSimulation" />
    public class DC : BiasingSimulation,
        IParameterized<DCParameters>
    {
        private IEnumerator<double>[] _sweepEnumerators;

        /// <summary>
        /// The constant returned when exporting a sweep point.
        /// </summary>
        public const int ExportSweep = 0x01;

        /// <summary>
        /// Gets the dc parameters.
        /// </summary>
        /// <value>
        /// The dc parameters.
        /// </value>
        public DCParameters DCParameters { get; } = new DCParameters();

        /// <inheritdoc/>
        DCParameters IParameterized<DCParameters>.Parameters => DCParameters;

        /// <summary>
        /// Occurs when iterating to a solution has failed.
        /// </summary>
        public event EventHandler<EventArgs> IterationFailed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DC"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public DC(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DC"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <param name="source">The source name.</param>
        /// <param name="start">The starting value.</param>
        /// <param name="stop">The stop value.</param>
        /// <param name="step">The step value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="source"/> is <c>null</c>.</exception>
        public DC(string name, string source, double start, double stop, double step)
            : this(name)
        {
            DCParameters.Sweeps.Add(new ParameterSweep(source, new LinearSweep(start, stop, step)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DC"/> class.
        /// </summary>
        /// <param name="name">The name of the simulation.</param>
        /// <param name="sweeps">The sweeps.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="sweeps"/> is <c>null</c>.</exception>
        public DC(string name, IEnumerable<ISweep> sweeps)
            : this(name)
        {
            sweeps.ThrowIfNull(nameof(sweeps));
            foreach (var sweep in sweeps)
                DCParameters.Sweeps.Add(sweep);
        }

        /// <inheritdoc/>
        protected override IEnumerable<int> Execute(int mask = Exports)
        {
            // Base
            foreach (int exportType in base.Execute(mask))
                yield return exportType;

            // Setup the state
            Iteration.Mode = IterationModes.Junction;

            // Initialize
            var sweeps = DCParameters.Sweeps.ToArray();
            _sweepEnumerators = new IEnumerator<double>[DCParameters.Sweeps.Count];
            for (int i = 0; i < sweeps.Length; i++)
            {
                _sweepEnumerators[i] = sweeps[i].CreatePoints(this);
                if (!_sweepEnumerators[i].MoveNext())
                    throw new SpiceSharpException(Properties.Resources.Simulations_DC_NoSweepPoints.FormatString(sweeps[i].Name));
            }

            // Execute the sweeps
            int level = sweeps.Length - 1;
            while (level >= 0)
            {
                // Fill the values up again by resetting
                while (level < sweeps.Length - 1)
                {
                    level++;
                    _sweepEnumerators[level] = sweeps[level].CreatePoints(this);
                    if (!_sweepEnumerators[level].MoveNext())
                        throw new SpiceSharpException(Properties.Resources.Simulations_DC_NoSweepPoints.FormatString(sweeps[level].Name));
                    Iteration.Mode = IterationModes.Junction;
                }

                // Calculate the solution
                if (!Iterate(DCParameters.SweepMaxIterations))
                {
                    IterationFailed?.Invoke(this, EventArgs.Empty);
                    Op(BiasingParameters.DcMaxIterations);
                }

                // Export data
                if ((mask & ExportSweep) != 0)
                    yield return ExportSweep;

                // Remove all values that are greater or equal to the maximum value
                while (level >= 0 && !_sweepEnumerators[level].MoveNext())
                    level--;
                if (level < 0)
                    break;
            }
        }

        /// <summary>
        /// Gets the current sweep values.
        /// The last element indicates the inner-most sweep value.
        /// </summary>
        /// <returns>The sweep values, or <c>null</c> if there are no sweeps active.</returns>
        public double[] GetCurrentSweepValue()
        {
            if (_sweepEnumerators == null)
                return null;
            double[] result = new double[_sweepEnumerators.Length];
            for (int i = 0; i < _sweepEnumerators.Length; i++)
                result[i] = _sweepEnumerators[i].Current;
            return result;
        }
    }
}
