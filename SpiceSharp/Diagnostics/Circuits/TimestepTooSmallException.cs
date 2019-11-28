namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when the timestep is too small.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    public class TimestepTooSmallException : SpiceSharpException
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
            : base(Properties.Resources.TimestepTooSmall.FormatString(timestep, time))
        {
            Timestep = timestep;
            Time = time;
        }
    }
}
