using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.General
{
    /// <summary>
    /// An <see cref="ITypeSet{V}"/> that tracks interfaces.
    /// </summary>
    /// <typeparam name="V">The base value type.</typeparam>
    public class InterfaceTypeSet<V> : ITypeSet<V>
    {
        private readonly InterfaceTypeDictionary<V> _dictionary;

        /// <inheritdoc/>
        public event EventHandler<TypeNotFoundEventArgs<V>> TypeNotFound;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceTypeSet{V}"/> class.
        /// </summary>
        public InterfaceTypeSet()
        {
            _dictionary = new InterfaceTypeDictionary<V>();
        }

        /// <summary>
        /// Gets the number of elements contained in the set.
        /// </summary>
        public int Count => _dictionary.Count;

        /// <summary>
        /// Gets a value indicating whether the set is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Add(V value) => _dictionary.Add(value.GetType(), value);

        /// <summary>
        /// Removes all items from the set.
        /// </summary>
        public void Clear() => _dictionary.Clear();

        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        /// <param name="item">The object to locate in the set.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item" /> is found in the set; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(V item) => _dictionary.Contains(item);

        /// <inheritdoc/>
        public bool ContainsType<TResult>() where TResult : V => _dictionary.ContainsKey(typeof(TResult));

        /// <inheritdoc/>
        public bool ContainsType(Type key) => _dictionary.ContainsKey(key.ThrowIfNull(nameof(key)));

        /// <summary>
        /// Copies the elements of the <see cref="ICollection{T}" /> to an <see cref="System.Array" />, starting at a particular <see cref="System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="System.Array" /> that is the destination of the elements copied from <see cref="System.Collections.Generic.ICollection{T}" />. The <see cref="System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(V[] array, int arrayIndex)
        {
            var arr = _dictionary.Values.ToArray();
            for (int i = 0; i < arr.Length; i++)
                array[i + arrayIndex] = arr[i];
        }

        /// <inheritdoc/>
        public TResult GetValue<TResult>() where TResult : V
        {
            try
            {
                if (!_dictionary.TryGetValue(typeof(TResult), out V result))
                {
                    var args = new TypeNotFoundEventArgs<V>(typeof(TResult));
                    TypeNotFound?.Invoke(this, args);
                    if (args.Value is TResult newResult)
                    {
                        Add(newResult);
                        return newResult;
                    }
                }
                return (TResult)result;
            }
            catch (KeyNotFoundException ex)
            {
                throw new TypeNotFoundException(Properties.Resources.TypeDictionary_TypeNotFound.FormatString(typeof(TResult).FullName), ex);
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the set.
        /// </summary>
        /// <param name="item">The object to remove from the set.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the set; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original set.
        /// </returns>
        public bool Remove(V item) => _dictionary.Remove(item.GetType(), item);

        /// <inheritdoc/>
        public bool TryGetValue<TResult>(out TResult value) where TResult : V
        {
            if (_dictionary.TryGetValue(typeof(TResult), out var result))
            {
                value = (TResult)result;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<V> GetEnumerator() => _dictionary.Values.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => _dictionary.Values.GetEnumerator();
    }
}
