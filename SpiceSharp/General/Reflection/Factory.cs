using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharp.Reflection
{
    /// <summary>
    /// A factory helper class for creating instances using a constructor without arguments.
    /// </summary>
    public static class Factory
    {
        private static readonly ConcurrentDictionary<Type, Method> _factories = new ConcurrentDictionary<Type, Method>();

        /// <summary>
        /// A delegate for creating parameters.
        /// </summary>
        /// <returns>The parameter set.</returns>
        public delegate object Method();

        /// <summary>
        /// Creates the factory for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The factory for the specified type.</returns>
        /// <exception cref="ArgumentException">Thrown if the constructor cannot be resolved.</exception>
        private static Method CreateFactory(Type type)
        {
            var ctors = type.GetTypeInfo().GetConstructors();
            ConstructorInfo ctor;
            if (ctors == null)
                ctor = null;
            else if (ctors.Length > 0)
                ctor = ctors[0];
            else
                throw new ArgumentException(Properties.Resources.DI_CannotResolveConstructor.FormatString(type.FullName));
            return Expression.Lambda<Method>(Expression.New(ctor)).Compile();
        }

        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>The instance of the specified type.</returns>
        public static T Get<T>()
        {
            var factory = _factories.GetOrAdd(typeof(T), CreateFactory);
            return (T)factory();
        }

        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The instance of the specified type.</returns>
        public static object Get(Type type)
        {
            type.ThrowIfNull(nameof(type));
            var factory = _factories.GetOrAdd(type, CreateFactory);
            return factory();
        }
    }

    /// <summary>
    /// A factory helper class for creating instances using a constructor with a single strongly typed argument.
    /// </summary>
    /// <typeparam name="T1">The type of the constructor argument.</typeparam>
    public static class Factory<T1>
    {
        private static readonly ConcurrentDictionary<Type, Method> _factories = new ConcurrentDictionary<Type, Method>();

        /// <summary>
        /// A delegate for a factory
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <returns>The resulting object.</returns>
        public delegate object Method(T1 arg);

        /// <summary>
        /// Creates the factory for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The factory for the specified type.</returns>
        /// <exception cref="ArgumentException">Thrown if the constructor cannot be resolved.</exception>
        private static Method CreateFactory(Type type)
        {
            var info = type.GetTypeInfo();
            if (info.IsAbstract || info.IsInterface || !info.IsPublic)
                throw new ArgumentException(Properties.Resources.DI_CannotResolveConstructor.FormatString(type.FullName));
            var ctors = type.GetTypeInfo().GetConstructors();
            var pInput = Expression.Parameter(typeof(T1), "arg");
            var arguments = new Expression[1];

            // Find a valid constructor
            var ctor = ctors.FirstOrDefault(ctor =>
            {
                var parameters = ctor.GetParameters();
                if (parameters == null || parameters.Length != 1)
                    return false;
                if (typeof(T1) == parameters[0].ParameterType)
                    arguments[0] = pInput;
                else if (typeof(T1).GetTypeInfo().IsAssignableFrom(parameters[0].ParameterType))
                    arguments[0] = Expression.Convert(pInput, typeof(T1));
                else
                    return false;
                return true;
            }) ?? throw new ArgumentException(Properties.Resources.DI_CannotResolveConstructor.FormatString(type.FullName));

            // Compile a factory method for the given type
            return Expression.Lambda<Method>(Expression.New(ctor, arguments), pInput).Compile();
        }

        /// <summary>
        /// Gets an instance of the specified type.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="argument">The argument.</param>
        /// <returns>An instance of the specified type.</returns>
        public static TResult Get<TResult>(T1 argument)
        {
            var factory = _factories.GetOrAdd(typeof(TResult), CreateFactory);
            return (TResult)factory(argument);
        }

        /// <summary>
        /// Gets an instance of the specified type.
        /// </summary>
        /// <param name="type">The type of the result.</param>
        /// <param name="argument">The argument.</param>
        /// <returns>An instance of the specified type.</returns>
        public static object Get(Type type, T1 argument)
        {
            type.ThrowIfNull(nameof(type));
            var factory = _factories.GetOrAdd(type, CreateFactory);
            return factory(argument);
        }
    }
}
