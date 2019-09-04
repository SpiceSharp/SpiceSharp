using System;
using System.Collections.Generic;

namespace SpiceSharp
{
    /// <summary>
    /// Describes a class that can contain one or more parameters, in combination with the <see cref="SpiceSharp.Attributes.ParameterNameAttribute"/>,
    /// and/or <see cref="SpiceSharp.Attributes.ParameterInfoAttribute"/> attributes.
    /// </summary>
    /// <remarks>
    /// This is a quite lengthy interface to implement. Remember that there are extension methods in
    /// <see cref="ParameterHelper"/> that will use reflection if the interface is not implemented. Only
    /// implement this interface if performance is an issue, or if the default behavior of the extension
    /// methods is incorrect.
    /// </remarks>
    public interface IParameterSet
    {
        #region Principal members
        /// <summary>
        /// Sets the value of the principal parameter.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if a principal parameter was set; otherwise <c>false</c>.
        /// </returns>
        bool TrySetPrincipalParameter<T>(T value);

        /// <summary>
        /// Sets the value of the principal parameters.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The source object (can be used for chaining).</returns>
        object SetPrincipalParameter<T>(T value);

        /// <summary>
        /// Tries to get the value of the principal parameter.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if a principal parameter was set; otherwise <c>false</c>.
        /// </returns>
        bool TryGetPrincipalParameter<T>(out T value);

        /// <summary>
        /// Gets the value of the principal parameter.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>The value of the principal parameter.</returns>
        T GetPrincipalParameter<T>();

        /// <summary>
        /// Creates a setter for the principal parameter.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <returns>
        /// An action that can set the value of the principal parameter, or <c>null</c> if there is no principal parameter.
        /// </returns>
        Action<T> CreatePrincipalSetter<T>();

        /// <summary>
        /// Creates a getter for the principal parameter.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <returns>
        /// A function returning the value of the principal parameter, or <c>null</c> if there is no principal parameter.
        /// </returns>
        Func<T> CreatePrincipalGetter<T>();
        #endregion

        #region Named value parameters
        /// <summary>
        /// Tries setting a parameter with a specified name.
        /// If multiple parameters have the same name, they will all be set.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        ///   <c>true</c> if there was one or more parameters set; otherwise <c>false</c>.
        /// </returns>
        bool TrySetParameter<T>(string name, T value, IEqualityComparer<string> comparer = null);

        /// <summary>
        /// Sets a parameter with a specified name. If multiple parameters have the same name, they will all be set.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>The source object (can be used for chaining).</returns>
        object SetParameter<T>(string name, T value, IEqualityComparer<string> comparer = null);

        /// <summary>
        /// Tries getting a parameter value. Only the first found parameter with the specified name is returned.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        ///     <c>true</c> if the parameter exists and the value was read; otherwise <c>false</c>.
        /// </returns>
        bool TryGetParameter<T>(string name, out T value, IEqualityComparer<string> comparer = null);

        /// <summary>
        /// Gets a parameter value. Only the first found parameter with the specified name is returned.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        /// The parameter value.
        /// </returns>
        T GetParameter<T>(string name, IEqualityComparer<string> comparer = null);

        /// <summary>
        /// Returns a setter for the first eligible parameter with the specified name.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        /// A function returning the value of the parameter, or <c>null</c> if there is no parameter with the specified name.
        /// </returns>
        Action<T> CreateSetter<T>(string name, IEqualityComparer<string> comparer = null);

        /// <summary>
        /// Returns a getter for the first found parameter with the specified name.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The string comparer used for identifying the parameter name.</param>
        /// <returns>
        /// A function returning the value of the parameter, or <c>null</c> if there is no parameter with the specified name.
        /// </returns>
        Func<T> CreateGetter<T>(string name, IEqualityComparer<string> comparer = null);
        #endregion

        #region Named methods
        /// <summary>
        /// Tries to call a method by name without arguments.
        /// If multiple parameters by this name exist, all of them will be called.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>
        ///   <c>true</c> if there was one or more methods called; otherwise <c>false</c>.
        /// </returns>
        bool TrySetParameter(string name, IEqualityComparer<string> comparer = null);

        /// <summary>
        /// Calls a method by name without arguments.
        /// If multiple parameters by this name exist, all of them will be called.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}" /> implementation to use when comparing parameter names, or <c>null</c> to use the default <see cref="EqualityComparer{T}"/>.</param>
        /// <returns>The source object (can be used for chaining).</returns>
        object SetParameter(string name, IEqualityComparer<string> comparer = null);
        #endregion
    }
}
