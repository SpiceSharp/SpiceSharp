using System;
using System.Collections.Generic;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// History of objects using a linked list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NodeHistory<T> : History<T>
    {
        /// <summary>
        /// Represents a single point in history
        /// </summary>
        protected class NodeHistoryPoint
        {
            /// <summary>
            /// The value at this point
            /// </summary>
            public T Value { get; set; }

            /// <summary>
            /// The previous point
            /// </summary>
            public NodeHistoryPoint Previous { get; set; }

            /// <summary>
            /// The next point
            /// </summary>
            public NodeHistoryPoint Next { get; set; }
        }

        /// <summary>
        /// The current point
        /// </summary>
        NodeHistoryPoint currentPoint;

        /// <summary>
        /// Gets or sets the current point
        /// </summary>
        public override T Current
        {
            get => currentPoint.Value;
            set => currentPoint.Value = value;
        }

        /// <summary>
        /// Gets a point in history
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns></returns>
        public override T this[int index]
        {
            get
            {
                // Find the matching node
                var point = currentPoint;
                for (int i = 0; i < index; i++)
                    point = point.Previous;
                return point.Value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length</param>
        public NodeHistory(int length)
            : base(length)
        {
            // Create a cycle
            NodeHistoryPoint first = new NodeHistoryPoint();
            NodeHistoryPoint current = first;
            for (int i = 1; i < length; i++)
            {
                current.Next = new NodeHistoryPoint()
                {
                    Previous = current
                };
                current = current.Next;
            }
            current.Next = first;
            first.Previous = current;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="defaultValue">Default value</param>
        public NodeHistory(int length, T defaultValue)
            : base(length)
        {
            // Create a cycle
            NodeHistoryPoint first = new NodeHistoryPoint();
            NodeHistoryPoint current = first;
            for (int i = 1; i < length; i++)
            {
                current.Next = new NodeHistoryPoint()
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
        /// Constructor
        /// </summary>
        /// <param name="length">Length</param>
        /// <param name="generator">Generator</param>
        public NodeHistory(int length, Func<int, T> generator)
            : base(length)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            // Create a cycle
            NodeHistoryPoint first = new NodeHistoryPoint()
            {
                Value = generator(0)
            };
            NodeHistoryPoint current = first;
            for (int i = 1; i < length; i++)
            {
                current.Next = new NodeHistoryPoint()
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
        /// Cycle
        /// </summary>
        public override void Cycle()
        {
            currentPoint = currentPoint.Next;
        }

        /// <summary>
        /// Store
        /// </summary>
        /// <param name="newValue">New value</param>
        public override void Store(T newValue)
        {
            currentPoint = currentPoint.Next;
            currentPoint.Value = newValue;
        }

        /// <summary>
        /// Clear with a value
        /// </summary>
        /// <param name="value"></param>
        public override void Clear(T value)
        {
            var current = currentPoint;
            for (int i = 0; i < Length; i++)
            {
                current.Value = value;
                current = current.Next;
            }
        }

        /// <summary>
        /// Clear with a generator
        /// </summary>
        /// <param name="generator">Generator</param>
        public override void Clear(Func<int, T> generator)
        {
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));

            var current = currentPoint;
            for (int i = 0; i < Length; i++)
            {
                current.Value = generator(i);
                current = current.Next;
            }
        }

        /// <summary>
        /// Gets all points in history
        /// </summary>
        protected override IEnumerable<T> Points
        {
            get
            {
                var current = currentPoint;
                for (int i = 0; i < Length; i++)
                {
                    yield return current.Value;
                    current = current.Next;
                }
            }
        }
    }
}
