using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Sweeps that can be nested
    /// </summary>
    public class NestedSweeps
    {
        /// <summary>
        /// Gets a sweep instance
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public SweepInstance this[int index]
        {
            get => _instances[index];
        }

        /// <summary>
        /// Gets the amount of nested sweeps
        /// </summary>
        public int Count { get => _instances.Count; }

        /// <summary>
        /// Private variables
        /// </summary>
        List<SweepInstance> _instances = new List<SweepInstance>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sweeps">Sweeps to be nested (in order)</param>
        public NestedSweeps(IEnumerable<SweepConfiguration> sweeps)
        {
            if (sweeps == null)
                throw new ArgumentNullException(nameof(sweeps));

            foreach (var sweep in sweeps)
                Add(sweep);
        }

        /// <summary>
        /// Add a sweep
        /// </summary>
        /// <param name="sweep">Sweep</param>
        void Add(SweepConfiguration sweep)
        {
            _instances.Add(new SweepInstance(sweep.ComponentName, sweep.Start, sweep.Stop, sweep.Step));
        }

        /// <summary>
        /// Clear
        /// </summary>
        public void Clear()
        {
            _instances.Clear();
        }
    }
}
