using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    /// <summary>
    /// A configuration for an integration method that has a variable timestep.
    /// </summary>
    /// <seealso cref="TimeParameters" />
    public abstract class VariableTimestepConfiguration : TimeParameters
    {
        /// <summary>
        /// Gets or sets the maximum step.
        /// If the maximum timestep is 0.0, a maximum timestep is chosen of 1/50 the time range.
        /// </summary>
        /// <value>
        /// The maximum timestep.
        /// </value>
        /// <exception cref="ArgumentException">Thrown if the specified timestep is negative.</exception>
        [ParameterName("maxstep"), ParameterName("deltamax"), ParameterInfo("The maximum timestep.")]
        public virtual double MaxStep
        {
            get
            {
                if (_maxStep == 0.0)
                    return (StopTime - StartTime) / 50.0;
                return _maxStep;
            }
            set
            {
                if (value < 0.0)
                    throw new ArgumentException(Properties.Resources.Simulations_Time_TimestepInvalid);
                _maxStep = value;
            }
        }
        private double _maxStep = 0.0;

        /// <summary>
        /// Gets or sets the minimum step.
        /// If the minimum timestep is 0.0, a minimum timestep is chosen of 1e-9 * <see cref="MaxStep"/>.
        /// </summary>
        /// <value>
        /// The minimum timestep.
        /// </value>
        /// <exception cref="ArgumentException">Thrown if the specified timestep is negative.</exception>
        [ParameterName("minstep"), ParameterName("deltamin"), ParameterInfo("The minimum timestep.")]
        public virtual double MinStep
        {
            get
            {
                if (_minStep == 0.0)
                    return MaxStep * 1e-9;
                return _minStep;
            }
            set
            {
                if (value < 0.0)
                    throw new ArgumentException(Properties.Resources.Simulations_Time_TimestepInvalid);
                _minStep = value;
            }
        }
        private double _minStep = 0.0;

        /// <summary>
        /// Gets or sets the maximum timestep expansion factor.
        /// </summary>
        /// <value>
        /// The maximum timestep expansion factor.
        /// </value>
        /// <exception cref="ArgumentException">Thrown if the expansion factor is less than 1.</exception>
        [ParameterName("expansion"), ParameterInfo("The maximum timestep expansion factor.")]
        public virtual double MaximumExpansion
        {
            get => _maxExpansion;
            set
            {
                if (value < 1)
                    throw new ArgumentException(Properties.Resources.Simulations_Time_MaximumExpansionTooSmall);
                _maxExpansion = value;
            }
        }
        private double _maxExpansion = 2.0;
    }
}
