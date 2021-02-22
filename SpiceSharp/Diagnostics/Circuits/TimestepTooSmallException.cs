using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace SpiceSharp
{
    /// <summary>
    /// Exception thrown when the timestep is too small.
    /// </summary>
    /// <seealso cref="SpiceSharpException" />
    [Serializable]
    public class TimestepTooSmallException : SpiceSharpException
    {
        /// <summary>
        /// Gets the timestep that was too small.
        /// </summary>
        /// <value>
        /// The timestep.
        /// </value>
        public virtual double Timestep { get; } = double.NaN;

        /// <summary>
        /// Gets the time where the timestep became too small.
        /// </summary>
        /// <value>
        /// The time.
        /// </value>
        public virtual double Time { get; } = double.NaN;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestepTooSmallException"/> class.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected TimestepTooSmallException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Timestep = info.GetDouble(nameof(Timestep));
            Time = info.GetDouble(nameof(Time));
        }

        /// <summary>
        /// Populates a SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The serialization info</param>
        /// <param name="context">The streaming context</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.ThrowIfNull(nameof(info));
            info.AddValue(nameof(Timestep), Timestep);
            info.AddValue(nameof(Time), Time);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestepTooSmallException"/> class.
        /// </summary>
        /// <param name="timestep">The timestep.</param>
        /// <param name="time">The time point.</param>
        public TimestepTooSmallException(double timestep, double time)
            : base(Properties.Resources.Simulations_Time_TimestepTooSmall.FormatString(timestep, time))
        {
            Timestep = timestep;
            Time = time;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestepTooSmallException"/> class.
        /// </summary>
        public TimestepTooSmallException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestepTooSmallException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TimestepTooSmallException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestepTooSmallException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public TimestepTooSmallException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
