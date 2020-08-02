using System.Collections.Generic;

namespace SpiceSharp.General
{
    /// <summary>
    /// A class that stores information about the inheritance tree stored in a type dictionary.
    /// </summary>
    public class TypeValues<T>
    {
        private class Node
        {
            public T Value;
            public bool IsIndirect;
            public Node Next;
        }
        private Node _first;

        /// <summary>
        /// Gets the associated value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value
        {
            get
            {
                if (_first != null)
                    return _first.Value;
                return default;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is ambiguous.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is ambiguous; otherwise, <c>false</c>.
        /// </value>
        public bool IsAmbiguous => _first.IsIndirect && _first.Next != null;

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty => _first == null;

        /// <summary>
        /// Gets a value indicating whether the value is a direct type reference.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the value is a direct type reference; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirect
        {
            get
            {
                if (_first == null)
                    return false;
                return !_first.IsIndirect;
            }
        }

        /// <summary>
        /// Enumerates all values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public IEnumerable<T> Values
        {
            get
            {
                var elt = _first;
                while (elt != null)
                {
                    yield return elt.Value;
                    elt = elt.Next;
                }
            }
        }

        /// <summary>
        /// Gets the number of values.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeValues{T}" /> class.
        /// </summary>
        public TypeValues()
        {
            _first = null;
        }

        /// <summary>
        /// Adds the specified value to the type values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="isDirect">if set to <c>true</c>, the value type is a direct reference (ie. not a child).</param>
        public void Add(T value, bool isDirect = false)
        {
            if (_first == null)
            {
                _first = new Node()
                {
                    Value = value,
                    IsIndirect = !isDirect
                };
            }
            else if (isDirect)
            {
                var node = new Node()
                {
                    Value = value,
                    IsIndirect = false,
                    Next = _first
                };
                _first = node;
            }
            else
            {
                var node = new Node()
                {
                    Value = value,
                    IsIndirect = true,
                    Next = _first.Next
                };
                _first.Next = node;
            }
            Count++;
        }

        /// <summary>
        /// Removes the specified value from the type values.
        /// </summary>
        /// <param name="value">The value.</param>
        public bool Remove(T value)
        {
            if (_first.Value.Equals(value))
            {
                _first = _first.Next;
                Count--;
                return true;
            }
            else
            {
                var previous = _first;
                var elt = _first.Next;
                while (elt != null)
                {
                    if (elt.Value.Equals(value))
                    {
                        previous.Next = elt.Next;
                        Count--;
                        return true;
                    }
                    previous = elt;
                    elt = elt.Next;
                }
            }
            return false;
        }
    }
}
