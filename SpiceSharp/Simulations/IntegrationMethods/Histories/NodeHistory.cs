using System;
using System.Collections.Generic;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// A class that implements a history using a linked list.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="History{T}" />
    public class NodeHistory<T> : History<T>
    {
        /// <summary>
        /// A class that represents a node in the history.
        /// </summary>
        protected class NodeHistoryPoint
        {
            /// <summary>
            /// Gets the value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public T Value { get; set; }

            /// <summary>
            /// Gets or sets the previous node.
            /// </summary>
            /// <value>
            /// The previous node.
            /// </value>
            public NodeHistoryPoint Previous { get; set; }

            /// <summary>
            /// Gets or sets the next node.
            /// </summary>
            /// <value>
            /// The next node.
            /// </value>
            public NodeHistoryPoint Next { get; set; }
        }

        /// <summary>
        /// The current point
        /// </summary>
        private NodeHistoryPoint _currentPoint;

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        /// <value>
        /// The current value.
        /// </value>
        public override T Current
        {
            get => _currentPoint.Value;
            set => _currentPoint.Value = value;
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <value>
        /// The value at the specified index.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The value at the specified index.
        /// </returns>
        public override T this[int index]
        {
            get
            {
                // Find the matching node
                var point = _currentPoint;
                for (var i = 0; i < index; i++)
                    point = point.Previous;
                return point.Value;
            }
        }

        /// <summary>
        /// Gets all points in the history.
        /// </summary>
        /// <value>
        /// The points.
        /// </value>
        protected override IEnumerable<T> Points
        {
            get
            {
                var current = _currentPoint;
                for (var i = 0; i < Length; i++)
                {
                    yield return current.Value;
                    current = current.Next;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeHistory{T}"/> class.
        /// </summary>
        /// <param name="length">The number of points to store.</param>
        public NodeHistory(int length)
            : base(length)
        {
            // Create a cycle
            var first = new NodeHistoryPoint();
            var current = first;
            for (var i = 1; i < length; i++)
            {
                current.Next = new NodeHistoryPoint
                {
                    Previous = current
                };
                current = current.Next;
            }
            current.Next = first;
            first.Previous = current;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeHistory{T}"/> class.
        /// </summary>
        /// <param name="length">The number of points to store.</param>
        /// <param name="defaultValue">The default value.</param>
        public NodeHistory(int length, T defaultValue)
            : base(length)
        {
            // Create a cycle
            var first = new NodeHistoryPoint();
            var current = first;
            for (var i = 1; i < length; i++)
            {
                current.Next = new NodeHistoryPoint
                {
                    Previous = current,
                    Value = defaultValue
                };
                current = current.Next;
            }
            current.Next = first;
            first.Previous = current;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeHistory{T}"/> class.
        /// </summary>
        /// <param name="length">The number of points to store.</param>
        /// <param name="generator">The function that generates the initial values.</param>
        /// <exception cref="ArgumentNullException">generator</exception>
        public NodeHistory(int length, Func<int, T> generator)
            : base(length)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            // Create a cycle
            var first = new NodeHistoryPoint
            {
                Value = generator(0)
            };
            var current = first;
            for (var i = 1; i < length; i++)
            {
                current.Next = new NodeHistoryPoint
                {
                    Previous = current,
                    Value = generator(i)
                };
                current = current.Next;
            }
            current.Next = first;
            first.Previous = current;
        }

        /// <summary>
        /// Cycles through history.
        /// </summary>
        public override void Cycle()
        {
            _currentPoint = _currentPoint.Next;
        }

        /// <summary>
        /// Store a new value in the history.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public override void Store(T newValue)
        {
            _currentPoint = _currentPoint.Next;
            _currentPoint.Value = newValue;
        }

        /// <summary>
        /// Clear the whole history with the same value.
        /// </summary>
        /// <param name="value">The value to be cleared with.</param>
        public override void Clear(T value)
        {
            var current = _currentPoint;
            for (var i = 0; i < Length; i++)
            {
                current.Value = value;
                current = current.Next;
            }
        }

        /// <summary>
        /// Clear the history using a function by index.
        /// </summary>
        /// <param name="generator">The function generating the values.</param>
        /// <exception cref="ArgumentNullException">generator</exception>
        public override void Clear(Func<int, T> generator)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            var current = _currentPoint;
            for (var i = 0; i < Length; i++)
            {
                current.Value = generator(i);
                current = current.Next;
            }
        }
    }
}
