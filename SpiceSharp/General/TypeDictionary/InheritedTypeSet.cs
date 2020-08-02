using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.General
{
    /// <summary>
    /// An <see cref="InheritedTypeDictionary{V}"/> that can add values by their own types.
    /// </summary>
    /// <typeparam name="V">The base value type.</typeparam>
    public class InheritedTypeSet<V> : ITypeSet<V>
    {
        private readonly InheritedTypeDictionary<V> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="InheritedTypeSet{V}"/> class.
        /// </summary>
        public InheritedTypeSet()
        {
            _dictionary = new InheritedTypeDictionary<V>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InheritedTypeSet{V}"/> class.
        /// </summary>
        /// <param name="original">The original.</param>
        protected InheritedTypeSet(InheritedTypeSet<V> original)
        {
            _dictionary = (InheritedTypeDictionary<V>)((ICloneable)original._dictionary).Clone();
        }


        /// <inheritdoc/>
        public int Count => _dictionary.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public void Add(V value) => _dictionary.Add(value.GetType(), value);

        /// <inheritdoc/>
        public void Clear() => _dictionary.Clear();

        /// <inheritdoc/>
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
            for (var i = 0; i < arr.Length; i++)
                array[i + arrayIndex] = arr[i];
        }

        /// <inheritdoc/>
        public TResult GetValue<TResult>() where TResult : V
        {
            try
            {
                return (TResult)_dictionary[typeof(TResult)];
            }
            catch (KeyNotFoundException ex)
            {
                throw new TypeNotFoundException(Properties.Resources.TypeDictionary_TypeNotFound.FormatString(typeof(TResult).FullName), ex);
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public ICloneable Clone() => new InheritedTypeSet<V>(this);

        /// <inheritdoc/>
        public void CopyFrom(ICloneable source)
        {
            var src = (InheritedTypeSet<V>)source;
            ((ICloneable)_dictionary).CopyFrom(src._dictionary);
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
