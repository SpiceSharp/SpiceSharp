namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when the timestep is too small.
    /// </summary>
    /// <seealso cref="CircuitException" />
    public class TimestepTooSmallException : CircuitException
    {
        /// <summary>
        /// Gets the timestep that was too small.
        /// </summary>
        /// <value>
        /// The timestep.
        /// </value>
        public double Timestep { get; }

        /// <summary>
        /// Gets the time where the timestep became too small.
        /// </summary>
        /// <value>
        /// The time.
        /// </value>
        public double Time { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestepTooSmallException"/> class.
        /// </summary>
        /// <param name="timestep">The timestep.</param>
        /// <param name="time">The time point.</param>
        public TimestepTooSmallException(double timestep, double time)
            : base("The timestep of {0:e5} s is too small at time {1:e5} s.".FormatString(timestep, time))
        {
            Timestep = timestep;
            Time = time;
        }
    }
}
