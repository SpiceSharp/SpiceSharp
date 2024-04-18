using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Simulations.Histories
{
    /// <summary>
    /// A class that implements a history using a linked list.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IHistory{T}" />
    public class NodeHistory<T> : IHistory<T>
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
        public T Value
        {
            get => _currentPoint.Value;
            set => _currentPoint.Value = value;
        }

        /// <summary>
        /// Gets the number of points tracked by the history.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length { get; }

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
            for (int i = 1; i < length; i++)
            {
                current.Next = new Node
                {
                    Previous = current
                };
                current = current.Next;
            }
            current.Next = first;
            first.Previous = current;
            _currentPoint = first;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeHistory{T}"/> class.
        /// </summary>
        /// <param name="length">The number of points to store.</param>
        /// <param name="defaultValue">The default value.</param>
        public NodeHistory(int length, T defaultValue)
        {
            Length = length;

            // Create a cycle
            var first = new Node();
            var current = first;
            for (int i = 1; i < length; i++)
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
            _currentPoint = first;
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
            for (int i = 1; i < length; i++)
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
            _currentPoint = first;
        }

        /// <summary>
        /// Accepts the current point and moves on to the next.
        /// </summary>
        public void Accept()
        {
            _currentPoint = _currentPoint.Next;
        }

        /// <summary>
        /// Gets the previous value. An index of 0 means the current value.
        /// </summary>
        /// <param name="index">The number of points to go back.</param>
        /// <returns>
        /// The previous value.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
        public T GetPreviousValue(int index)
        {
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            // Find the matching node
            var point = _currentPoint;
            for (int i = 0; i < index; i++)
                point = point.Previous;
            return point.Value;
        }

        /// <summary>
        /// Sets all values in the history to the same value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Set(T value)
        {
            var current = _currentPoint;
            for (int i = 0; i < Length; i++)
            {
                current.Value = value;
                current = current.Previous;
            }
        }

        /// <summary>
        /// Sets all values in the history to the value returned by the specified method.
        /// </summary>
        /// <param name="method">The method for creating the value. The index indicates the current point.</param>
        public void Set(Func<int, T> method)
        {
            method.ThrowIfNull(nameof(method));
            var current = _currentPoint;
            for (int i = 0; i < Length; i++)
            {
                current.Value = method(i);
                current = current.Previous;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            var elt = _currentPoint;
            for (int i = 0; i < Length; i++)
            {
                yield return elt.Value;
                elt = elt.Previous;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
