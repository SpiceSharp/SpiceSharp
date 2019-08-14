using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class describing sweeps that can be nested
    /// </summary>
    public class NestedSweeps
    {
        /// <summary>
        /// Gets a sweep instance.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>The sweep index.</returns>
        public SweepInstance this[int index] => _instances[index];

        /// <summary>
        /// Gets the sweep count.
        /// </summary>
        public int Count => _instances.Count;

        /// <summary>
        /// Gets the top-most sweep.
        /// </summary>
        public SweepInstance Top => _instances.Last();

        /// <summary>
        /// Private variables
        /// </summary>
        private readonly List<SweepInstance> _instances = new List<SweepInstance>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedSweeps"/> class.
        /// </summary>
        /// <param name="sweeps">The sweeps.</param>
        public NestedSweeps(IEnumerable<SweepConfiguration> sweeps)
        {
            sweeps.ThrowIfNull(nameof(sweeps));

            foreach (var sweep in sweeps)
                Add(sweep);
        }

        /// <summary>
        /// Adds the specified sweep.
        /// </summary>
        /// <param name="sweep">The sweep.</param>
        private void Add(SweepConfiguration sweep)
        {
            _instances.Add(new SweepInstance(sweep.ComponentName, sweep.Start, sweep.Stop, sweep.Step));
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _instances.Clear();
        }
    }
}
