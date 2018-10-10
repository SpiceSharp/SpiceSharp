using System;
using System.Collections.Generic;
using System.Text;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Accept behavior for a <see cref="DelayModel" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseAcceptBehavior" />
    public class ModelAcceptBehavior : BaseAcceptBehavior
    {
        // Private variables
        private double _minTrackTime = 0.0;
        private Dictionary<Variable, int> _variableIndices = new Dictionary<Variable, int>();
        private SortedSet<double> _timePoints = new SortedSet<double>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelAcceptBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public ModelAcceptBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Setup the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="provider">The data provider.</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            // Clear all tracked variables
            _variableIndices.Clear();
        }

        /// <summary>
        /// Destroy the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void Unsetup(Simulation simulation)
        {
            // Clear all tracked variables
            _variableIndices.Clear();
        }

        /// <summary>
        /// Requests the voltage at a specified timepoint. Will interpolate if necessary.
        /// </summary>
        /// <param name="index">The index of the delay.</param>
        /// <param name="time">The time-point.</param>
        /// <returns></returns>
        public double RequestValue(int index, double time)
        {

        }

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        /// <param name="simulation">The time-based simulation</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Accept(TimeSimulation simulation)
        {
            
        }
    }
}
