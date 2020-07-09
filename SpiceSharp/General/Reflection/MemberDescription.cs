using SpiceSharp.ParameterSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharp.Reflection
{
    /// <summary>
    /// The description of a member on a type with named parameters.
    /// </summary>
    public class MemberDescription
    {
        /// <summary>
        /// Gets the type for setting the parameter value.
        /// </summary>
        /// <value>
        /// The type that can be used for setting the member value.
        /// </value>
        /// <remarks>
        /// A parameter is a named quantity that can be specified by the user.
        /// </remarks>
        public Type ParameterType { get; }

        /// <summary>
        /// Gets the type for getting the property value.
        /// </summary>
        /// <value>
        /// The type that can be used for getting the member value.
        /// </value>
        /// <remarks>
        /// A property is a named quantity that can be asked by the user.
        /// </remarks>
        public Type PropertyType { get; }

        /// <summary>
        /// Gets the member.
        /// </summary>
        /// <value>
        /// The member.
        /// </value>
        public MemberInfo Member { get; }

        /// <summary>
        /// Gets the names.
        /// </summary>
        /// <value>
        /// The names.
        /// </value>
        public IReadOnlyList<string> Names { get; }

        /// <summary>
        /// Gets the description of the parameter.
        /// </summary>
        /// <value>
        /// The description of the parameter.
        /// </value>
        public string Description
        {
            get
            {
                // Try to find a parameter info attribute
                var attr = Member.GetCustomAttribute<ParameterInfoAttribute>();
                return attr?.Description ?? "";
            }
        }

        /// <summary>
        /// Gets a value indicating whether this member is interesting.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is interesting; otherwise, <c>false</c>.
        /// </value>
        public bool Interesting
        {
            get
            {
                var attr = Member.GetCustomAttribute<ParameterInfoAttribute>();
                return attr?.Interesting ?? false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is principal.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is principal; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrincipal
        {
            get
            {
                var attr = Member.GetCustomAttribute<ParameterInfoAttribute>();
                return attr?.Interesting ?? false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the member is static.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this member is static; otherwise, <c>false</c>.
        /// </value>
        public bool IsStatic { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberDescription"/> class.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="member"/> is <c>null</c>.</exception>
        public MemberDescription(MemberInfo member)
        {
            Member = member.ThrowIfNull(nameof(member));

            // Get all the names of this parameter
            Names = member
                .GetCustomAttributes(typeof(ParameterNameAttribute))
                .Select(attr => ((ParameterNameAttribute)attr).Name).ToArray();

            // Cache the return type
            ParameterType = typeof(void);
            PropertyType = typeof(void);
            switch (member)
            {
                case FieldInfo fi:
                    ParameterType = fi.FieldType;
                    PropertyType = fi.FieldType;
                    IsStatic = fi.IsStatic;
                    break;
                case PropertyInfo pi:
                    ParameterType = pi.PropertyType;
                    PropertyType = pi.PropertyType;
                    IsStatic =
                        pi.GetGetMethod()?.IsStatic ??
                        pi.GetSetMethod()?.IsStatic ??
                        throw new ArgumentException();
                    break;
                case MethodInfo mi:
                    var ps = mi.GetParameters();
                    if (ps.Length == 0)
                        PropertyType = mi.ReturnType;
                    else if (ps.Length == 1)
                        ParameterType = ps[0].ParameterType;
                    break;
            }
        }

        /// <summary>
        /// Tries setting the member to a specified value.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the parameter was set; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="TargetException">Thrown if <paramref name="source" /> is <c>null</c> and the method is not static.</exception>
        /// <exception cref="TargetInvocationException">Thrown if the invoked get method throws an exception.</exception>
        public bool TrySet<P>(object source, P value)
        {
            // The value can't be set here!
            if (typeof(P) != ParameterType)
                return false;
            if (IsStatic && source != null || !IsStatic && source == null)
                return false;

            switch (Member)
            {
                case MethodInfo mi:
                    mi.Invoke(source, new object[] { value });
                    return true;

                case PropertyInfo pi:
                    var setter = pi.GetSetMethod();
                    if (setter != null)
                    {
                        setter.Invoke(source, new object[] { value });
                        return true;
                    }
                    break;

                case FieldInfo fi:
                    fi.SetValue(source, value);
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a setter for the member.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <returns>
        /// The action that can set the parameter, or null if the parameter can't be set.
        /// </returns>
        public Action<P> CreateSetter<P>(object source)
        {
            if (ParameterType != typeof(P))
                return null;
            if (IsStatic && source != null || !IsStatic && source == null)
                return null;

            switch (Member)
            {
                case MethodInfo mi:
                    return (Action<P>)mi.CreateDelegate(typeof(Action<P>), source);

                case PropertyInfo pi:
                    var setter = pi.GetSetMethod();
                    if (setter != null)
                        return (Action<P>)setter.CreateDelegate(typeof(Action<P>), source);
                    break;

                case FieldInfo fi:
                    var paramValue = Expression.Parameter(typeof(P), "value");
                    return Expression.Lambda<Action<P>>(
                        Expression.Assign(
                            Expression.Field(
                                Expression.Constant(source),
                                fi),
                            paramValue
                        ), paramValue).Compile();
            }
            return null;
        }

        /// <summary>
        /// Tries getting the value of the member.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// <c>true</c> if the parameter value was returned; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="TargetException">Thrown if <paramref name="source"/> is <c>null</c> and the method is not static.</exception>
        /// <exception cref="TargetInvocationException">Thrown if the invoked get method throws an exception.</exception>
        public bool TryGet<P>(object source, out P value)
        {
            if (typeof(P) != PropertyType ||
                IsStatic && source != null ||
                !IsStatic && source == null)
            {
                value = default;
                return false;
            }

            switch (Member)
            {
                case MethodInfo mi:
                    value = (P)mi.Invoke(source, null);
                    return true;

                case PropertyInfo pi:
                    var getter = pi.GetGetMethod();
                    if (getter != null)
                    {
                        value = (P)getter.Invoke(source, null);
                        return true;
                    }
                    break;

                case FieldInfo fi:
                    value = (P)fi.GetValue(source);
                    return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Creates a getter for the member.
        /// </summary>
        /// <typeparam name="P">The parameter type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <returns>
        /// The function that will return the parameter value, or null if the parameter can't be returned.
        /// </returns>
        public Func<P> CreateGetter<P>(object source)
        {
            if (typeof(P) != PropertyType ||
                IsStatic && source != null ||
                !IsStatic && source == null)
                return null;

            switch (Member)
            {
                case MethodInfo mi:
                    return (Func<P>)mi.CreateDelegate(typeof(Func<P>), source);

                case PropertyInfo pi:
                    var getter = pi.GetGetMethod();
                    if (getter != null)
                        return (Func<P>)getter.CreateDelegate(typeof(Func<P>), source);
                    break;

                case FieldInfo fi:
                    return Expression.Lambda<Func<P>>(
                        Expression.Label(
                            Expression.Label(typeof(P)),
                            Expression.Field(
                                Expression.Constant(source),
                                fi)
                            )
                        ).Compile();
            }
            return null;
        }

        /// <summary>
        /// Returns a string that represents the current member description.
        /// </summary>
        /// <returns>
        /// A string that represents the current member description.
        /// </returns>
        public override string ToString()
        {
            if (Names.Count > 0)
                return "{0} (\"{1}\" {2})".FormatString(Member.Name, string.Join("\",\"", Names), PropertyType?.Name ?? ParameterType?.Name ?? "void");
            return "{0} ({1})".FormatString(Member.Name, PropertyType?.Name ?? ParameterType?.Name ?? "void");
        }
    }
}
