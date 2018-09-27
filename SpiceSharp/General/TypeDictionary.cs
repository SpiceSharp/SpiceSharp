using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SpiceSharp
{
    /// <summary>
    /// A base template for storing objects that can be retrieved by their type. It also tracks inheritance,
    /// so you can retrieve objects by their base class.
    /// </summary>
    /// <typeparam name="T">The base type.</typeparam>
    /// <seealso cref="IDictionary{Type, T}" />
    public abstract class TypeDictionary<T> : IDictionary<Type, T> where T : class
    {
        /// <summary>
        /// Gets the dictionary with all types.
        /// </summary>
        protected Dictionary<Type, T> Dictionary { get; }

        /// <summary>
        /// Gets the base class type.
        /// </summary>
        /// <value>
        /// The base class type.
        /// </value>
        /// <remarks>
        /// This type allows us to apply constraints to the types of classes that can be added to the dictionary.
        /// </remarks>
        protected Type BaseClass { get; }

        /// <summary>
        /// Gets an <see cref="ICollection{T}" /> containing the keys of the <see cref="TypeDictionary{T}" />.
        /// </summary>
        public ICollection<Type> Keys => Dictionary.Keys;

        /// <summary>
        /// Gets an <see cref="ICollection{T}" /> containing the values in the <see cref="TypeDictionary{T}" />.
        /// </summary>
        public ICollection<T> Values => Dictionary.Values;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="TypeDictionary{T}" />.
        /// </summary>
        public int Count => Dictionary.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="TypeDictionary{T}" /> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets the value with the specified key.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="key">The type.</param>
        /// <returns>The object of the specified type.</returns>
        /// <exception cref="ArgumentException">Type {0} is not derived from {1}".FormatString(key, BaseClass)</exception>
        public T this[Type key]
        {
            get => Dictionary[key];
            set
            {
                var currentType = key;
                while (currentType != BaseClass)
                {
                    Dictionary[currentType] = value;
                    if (currentType == typeof(object))
                        throw new ArgumentException("Type {0} is not derived from {1}".FormatString(key, BaseClass));
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDictionary{T}" /> class.
        /// </summary>
        /// <param name="baseClass">The base class type.</param>
        /// <remarks>
        /// Only objects that implement the <paramref name="baseClass" /> type are allowed in the dictionary.
        /// </remarks>
        protected TypeDictionary(Type baseClass)
        {
            BaseClass = baseClass;
            Dictionary = new Dictionary<Type, T>();
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="TypeDictionary{T}"/>.
        /// </summary>
        /// <param name="key">The type of the added value.</param>
        /// <param name="value">The added value.</param>
        /// <exception cref="ArgumentNullException">key</exception>
        /// <exception cref="CircuitException">Type {0} is not derived from {1}".FormatString(key, BaseClass)</exception>
        public virtual void Add(Type key, T value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var currentType = key;
            while (currentType != null && currentType != BaseClass)
            {
                Dictionary.Add(currentType, value);
                currentType = currentType.GetTypeInfo().BaseType;
                if (currentType == typeof(object))
                    throw new CircuitException("Type {0} is not derived from {1}".FormatString(key, BaseClass));
            }
        }

        /// <summary>
        /// Gets a strongly typed object from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>The requested object.</returns>
        public TResult Get<TResult>() where TResult : T => (TResult) Dictionary[typeof(TResult)];

        /// <summary>
        /// Tries to get a strongly typed object from the dictionary.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="value">The requested object.</param>
        /// <returns>
        ///   <c>true</c> if the object was found; otherwise <c>false</c>.
        /// </returns>
        public bool TryGet<TResult>(out TResult value) where TResult : T
        {
            if (Dictionary.TryGetValue(typeof(TResult), out var result))
            {
                value = (TResult) result;
                return true;
            }

            value = default(TResult);
            return false;
        }

        /// <summary>
        /// Determines whether the <see cref="TypeDictionary{T}" /> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="TypeDictionary{T}" />.</param>
        /// <returns>
        ///   <c>true</c> if the <see cref="TypeDictionary{T}" /> contains an element with the key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(Type key) => Dictionary.ContainsKey(key);

        /// <summary>
        /// Removes the element with the specified key from the <see cref="TypeDictionary{T}" />.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        ///   <c>true</c> if the element is successfully removed; otherwise, <c>false</c>.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="TypeDictionary{T}" />.
        /// </returns>
        public bool Remove(Type key) => Dictionary.Remove(key);

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key,
        /// if the key is found; otherwise, the default value for the type of the
        /// <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        ///   <c>true</c> if the object that implements <see cref="TypeDictionary{T}" /> contains an element with the specified key; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetValue(Type key, out T value) => Dictionary.TryGetValue(key, out value);

        /// <summary>
        /// Adds an item to the <see cref="TypeDictionary{T}"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="TypeDictionary{T}" />.</param>
        public void Add(KeyValuePair<Type, T> item) => Add(item.Key, item.Value);

        /// <summary>
        /// Removes all items from the <see cref="TypeDictionary{T}"/>.
        /// </summary>
        public void Clear() => Dictionary.Clear();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<Type, T>> GetEnumerator() => Dictionary.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="TypeDictionary{T}" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="TypeDictionary{T}" />; otherwise, false.
        /// </returns>
        bool ICollection<KeyValuePair<Type, T>>.Contains(KeyValuePair<Type, T> item) => ((ICollection<KeyValuePair<Type, T>>)Dictionary).Contains(item);

        /// <summary>
        /// Copies the elements of the <see cref="TypeDictionary{T}" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="TypeDictionary{T}" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        void ICollection<KeyValuePair<Type, T>>.CopyTo(KeyValuePair<Type, T>[] array, int arrayIndex) => ((ICollection<KeyValuePair<Type, T>>)Dictionary).CopyTo(array, arrayIndex);

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="TypeDictionary{T}" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="TypeDictionary{T}" />.</param>
        /// <returns>
        ///   <c>true</c> if <paramref name="item" /> was successfully removed from the <see cref="TypeDictionary{T}" />; otherwise, <c>false</c>. This method also returns false if <paramref name="item" /> is not found in the original <see cref="TypeDictionary{T}" />.
        /// </returns>
        bool ICollection<KeyValuePair<Type, T>>.Remove(KeyValuePair<Type, T> item) => ((ICollection<KeyValuePair<Type, T>>)Dictionary).Remove(item);
    }
}
