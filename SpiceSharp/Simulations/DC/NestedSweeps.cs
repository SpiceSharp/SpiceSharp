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
        /// Get a sweep instance
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public SweepInstance this[int index]
        {
            get => instances[index];
        }

        /// <summary>
        /// Get the amount of nested sweeps
        /// </summary>
        public int Count { get => instances.Count; }

        /// <summary>
        /// Private variables
        /// </summary>
        List<SweepInstance> instances = new List<SweepInstance>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sweeps">Sweeps to be nested (in order)</param>
        public NestedSweeps(IEnumerable<Sweep> sweeps)
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
        void Add(Sweep sweep)
        {
            instances.Add(new SweepInstance(sweep.ComponentName, sweep.Start, sweep.Stop, sweep.Step));
        }

        /// <summary>
        /// Clear
        /// </summary>
        public void Clear()
        {
            instances.Clear();
        }
    }
}
