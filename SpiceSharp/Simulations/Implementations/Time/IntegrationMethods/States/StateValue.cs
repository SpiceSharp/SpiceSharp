using SpiceSharp.Simulations.Histories;
using System;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    /// <summary>
    /// An integration state that can track a value of a specified type.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <seealso cref="IIntegrationState" />
    public class StateValue<T> : IIntegrationState
    {
        private readonly IHistory<T> _history;

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        /// <value>
        /// The current value.
        /// </value>
        public T Value
        {
            get => _history.Value;
            set => _history.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateValue{T}"/> class.
        /// </summary>
        /// <param name="length">The number of points to be tracked.</param>
        /// <exception cref="ArgumentException">Thrown if the number of points is smaller than 1.</exception>
        public StateValue(int length)
        {
            if (length < 1)
                throw new ArgumentException(Properties.Resources.Simulations_History_InvalidLength);
            if (length <= 3)
                _history = new ArrayHistory<T>(length);
            else
                _history = new NodeHistory<T>(length);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateValue{T}"/> class.
        /// </summary>
        /// <param name="history">The history.</param>
        public StateValue(IHistory<T> history)
        {
            _history = history.ThrowIfNull(nameof(history));
        }

        /// <summary>
        /// Accepts the current point and moves on to the next.
        /// </summary>
        public void Accept() => _history.Accept();

        /// <summary>
        /// Gets a previous value of the state. An index of 0 indicates the current value.
        /// </summary>
        /// <param name="index">The number of points to go back in time.</param>
        /// <returns>
        /// The previous value.
        /// </returns>
        public T GetPreviousValue(int index) => _history.GetPreviousValue(index);
    }
}
