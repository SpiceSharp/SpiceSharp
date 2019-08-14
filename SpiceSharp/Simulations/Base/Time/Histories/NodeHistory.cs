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
        protected class Node
        {
            /// <summary>
            /// Gets or sets the node value.
            /// </summary>
            public T Value { get; set; }

            /// <summary>
            /// Gets or sets the previous node.
            /// </summary>
            public Node Previous { get; set; }

            /// <summary>
            /// Gets or sets the next node.
            /// </summary>
            public Node Next { get; set; }
        }

        /// <summary>
        /// The current point
        /// </summary>
        private Node _currentPoint;

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public override T Current
        {
            get => _currentPoint.Value;
            set => _currentPoint.Value = value;
        }

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
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
        {
            Length = length;

            // Create a cycle
            var first = new Node();
            var current = first;
            for (var i = 1; i < length; i++)
            {
                current.Next = new Node
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
        {
            // Create a cycle
            var first = new Node();
            var current = first;
            for (var i = 1; i < length; i++)
            {
                current.Next = new Node
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
        public NodeHistory(int length, Func<int, T> generator)
        {
            generator.ThrowIfNull(nameof(generator));
            Length = length;

            // Create a cycle
            var first = new Node
            {
                Value = generator(0)
            };
            var current = first;
            for (var i = 1; i < length; i++)
            {
                current.Next = new Node
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
        /// Expand the history length.
        /// </summary>
        /// <param name="newLength">The new number of points in history to track.</param>
        public override void Expand(int newLength)
        {
            if (newLength < Length)
                return;

            var last = _currentPoint.Next;
            for (var i = Length; i < newLength; i++)
            {
                // Create elements between current and last
                var newElt = new Node
                {
                    Next = last
                };
                last.Previous = newElt;
                last = newElt;
            }

            // Close links
            _currentPoint.Next = last;
            last.Previous = _currentPoint;

            Length = newLength;
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
        public override void Clear(Func<int, T> generator)
        {
            generator.ThrowIfNull(nameof(generator));

            var current = _currentPoint;
            for (var i = 0; i < Length; i++)
            {
                current.Value = generator(i);
                current = current.Next;
            }
        }
    }
}
