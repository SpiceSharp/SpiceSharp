using System;

namespace SpiceSharp
{
    /// <summary>
    /// Describes a class that can contain one or more parameters, in combination with the <see cref="SpiceSharp.Attributes.ParameterNameAttribute"/>,
    /// and/or <see cref="SpiceSharp.Attributes.ParameterInfoAttribute"/> attributes.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <remarks>
    /// A thorough implementation that uses reflection is defined in <see cref="Reflection"/>. Use this
    /// interface if the default behavior of these extension methods is not the right one.
    /// </remarks>
    public interface IParameterSet<T>
    {
        #region Principal members
        /// <summary>
        /// Sets the value of the principal parameter.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if a principal parameter was set; otherwise <c>false</c>.
        /// </returns>
        bool TrySetParameter<P>(P value);

        /// <summary>
        /// Sets the value of the principal parameters.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The source object (can be used for chaining).</returns>
        T SetParameter<P>(P value);

        /// <summary>
        /// Tries to get the value of the principal parameter.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if a principal parameter was set; otherwise <c>false</c>.
        /// </returns>
        bool TryGetParameter<P>(out P value);

        /// <summary>
        /// Gets the value of the principal parameter.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <returns>The value of the principal parameter.</returns>
        P GetParameter<P>();

        /// <summary>
        /// Creates a setter for the principal parameter.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <returns>
        /// An action that can set the value of the principal parameter, or <c>null</c> if there is no principal parameter.
        /// </returns>
        Action<P> CreateSetter<P>();

        /// <summary>
        /// Creates a getter for the principal parameter.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <returns>
        /// A function returning the value of the principal parameter, or <c>null</c> if there is no principal parameter.
        /// </returns>
        Func<P> CreateGetter<P>();
        #endregion

        #region Named value parameters
        /// <summary>
        /// Tries setting a parameter with a specified name.
        /// If multiple parameters have the same name, they will all be set.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if there was one or more parameters set; otherwise <c>false</c>.
        /// </returns>
        bool TrySetParameter<P>(string name, P value);

        /// <summary>
        /// Sets a parameter with a specified name. If multiple parameters have the same name, they will all be set.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>The source object (can be used for chaining).</returns>
        T SetParameter<P>(string name, P value);

        /// <summary>
        /// Tries getting a parameter value. Only the first found parameter with the specified name is returned.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///     <c>true</c> if the parameter exists and the value was read; otherwise <c>false</c>.
        /// </returns>
        bool TryGetParameter<P>(string name, out P value);

        /// <summary>
        /// Gets a parameter value. Only the first found parameter with the specified name is returned.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// The parameter value.
        /// </returns>
        P GetParameter<P>(string name);

        /// <summary>
        /// Returns a setter for the first eligible parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A function returning the value of the parameter, or <c>null</c> if there is no parameter with the specified name.
        /// </returns>
        Action<P> CreateSetter<P>(string name);

        /// <summary>
        /// Returns a getter for the first found parameter with the specified name.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        /// A function returning the value of the parameter, or <c>null</c> if there is no parameter with the specified name.
        /// </returns>
        Func<P> CreateGetter<P>(string name);
        #endregion

        #region Named methods
        /// <summary>
        /// Tries to call a method by name without arguments.
        /// If multiple parameters by this name exist, all of them will be called.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <returns>
        ///   <c>true</c> if there was one or more methods called; otherwise <c>false</c>.
        /// </returns>
        bool TryCall(string name);

        /// <summary>
        /// Calls a method by name without arguments.
        /// If multiple parameters by this name exist, all of them will be called.
        /// </summary>
        /// <param name="name">The name of the method.</param>
        /// <returns>The source object (can be used for chaining).</returns>
        T Call(string name);
        #endregion
    }
}
