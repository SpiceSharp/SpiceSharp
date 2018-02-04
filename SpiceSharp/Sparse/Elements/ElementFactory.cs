using System;
using System.Numerics;
using System.Collections.Generic;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Factory for matrix element values
    /// </summary>
    public static class ElementFactory
    {
        /// <summary>
        /// Dictionary with factories
        /// </summary>
        static readonly Dictionary<Type, Func<IElement>> dict = new Dictionary<Type, Func<IElement>>()
        {
            { typeof(double), () => new RealElement() },
            { typeof(Complex), () => new ComplexElement() }
        };

        /// <summary>
        /// Register a new type
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="func">Function</param>
        public static void Register(Type type, Func<IElement> func) => dict.Add(type, func);

        /// <summary>
        /// Remove a type
        /// </summary>
        /// <param name="type"></param>
        public static void Remove(Type type) => dict.Remove(type);

        /// <summary>
        /// Create a new element
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <returns></returns>
        public static Element<T> Create<T>()
        {
            return (Element<T>)dict[typeof(T)]();
        }
    }
}
