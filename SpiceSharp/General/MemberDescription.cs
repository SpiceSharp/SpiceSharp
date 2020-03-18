using SpiceSharp.Attributes;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SpiceSharp.General
{
    /// <summary>
    /// A description of a member
    /// </summary>
    public class MemberDescription
    {
        /// <summary>
        /// Gets the type of the return.
        /// </summary>
        /// <value>
        /// The type of the return.
        /// </value>
        public Type ReturnType { get; }

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
        public string[] Names { get; }

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
        /// Initializes a new instance of the <see cref="MemberDescription"/> class.
        /// </summary>
        /// <param name="member">The member.</param>
        public MemberDescription(MemberInfo member)
        {
            Member = member;

            // Get all the names of this parameter
            Names = member
                .GetCustomAttributes(typeof(ParameterNameAttribute))
                .Select(attr => ((ParameterNameAttribute)attr).Name).ToArray();

            // Cache the return type
            switch (member)
            {
                case FieldInfo fi:
                    ReturnType = fi.FieldType;
                    break;
                case PropertyInfo pi:
                    ReturnType = pi.PropertyType;
                    break;
                case MethodInfo mi:
                    ReturnType = mi.ReturnType;
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
        /// <c>true</c> if the parameter was set; otherwise <c>false</c>.
        /// </returns>
        public bool TrySet<P>(object source, P value)
        {
            if (ReturnType == typeof(void))
            {
                if (Member is MethodInfo mi)
                {
                    var ps = mi.GetParameters();
                    if (ps.Length == 1 && typeof(P).GetTypeInfo().IsAssignableFrom(ps[0].ParameterType))
                    {
                        mi.Invoke(source, new object[] { value });
                        return true;
                    }
                }
            }
            else
            {
                if (ReturnType.GetTypeInfo().IsAssignableFrom(typeof(P)))
                {
                    switch (Member)
                    {
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
                }
                else if (TrySet<GivenParameter<P>>(source, value))
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
            if (ReturnType == typeof(void))
            {
                if (Member is MethodInfo mi)
                {
                    var ps = mi.GetParameters();
                    if (ps.Length == 1 && ps[0].ParameterType == typeof(P))
                        return (Action<P>)mi.CreateDelegate(typeof(Action<P>), source);
                }
            }
            else if (ReturnType.GetTypeInfo().IsAssignableFrom(typeof(P)))
            {
                switch (Member)
                {
                    case PropertyInfo pi:
                        var setter = pi.GetSetMethod();
                        if (setter != null)
                            return (Action<P>)setter.CreateDelegate(typeof(Action<P>), source);
                        break;
                    case FieldInfo fi:
                        if (fi.FieldType == typeof(P))
                        {
                            var constThis = Expression.Constant(source);
                            var constField = Expression.Field(constThis, fi);
                            var paramValue = Expression.Parameter(typeof(P), "value");
                            var assignField = Expression.Assign(constField, paramValue);
                            return Expression.Lambda<Action<P>>(assignField, paramValue).Compile();
                        }
                        break;
                }
            }
            else
            {
                var setter = CreateSetter<GivenParameter<P>>(source);
                if (setter != null)
                    return new Action<P>(value => setter(value));
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
        public bool TryGet<P>(object source, out P value)
        {
            if (typeof(P).GetTypeInfo().IsAssignableFrom(ReturnType))
            {
                switch (Member)
                {
                    case PropertyInfo pi:
                        var getter = pi.GetGetMethod();
                        if (getter != null)
                        {
                            value = (P)getter.Invoke(source, new object[] { });
                            return true;
                        }
                        break;
                    case FieldInfo fi:
                        value = (P)fi.GetValue(source);
                        return true;
                    case MethodInfo mi:
                        if (mi.GetParameters().Length == 0)
                        {
                            value = (P)mi.Invoke(source, new object[] { });
                            return true;
                        }
                        break;
                }
            }
            else if (TryGet(source, out GivenParameter<P> gp))
            {
                value = gp.Value;
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
            if (typeof(P).GetTypeInfo().IsAssignableFrom(ReturnType))
            {
                switch (Member)
                {
                    case PropertyInfo pi:
                        if (pi.CanRead)
                            return (Func<P>)pi.GetGetMethod().CreateDelegate(typeof(Func<P>), source);
                        break;
                    case FieldInfo fi:
                        if (fi.FieldType == typeof(P))
                        {
                            var constThis = Expression.Constant(source);
                            var constField = Expression.Field(constThis, fi);
                            var returnLabel = Expression.Label(typeof(P));
                            return Expression.Lambda<Func<P>>(Expression.Label(returnLabel, constField)).Compile();
                        }
                        break;
                    case MethodInfo mi:
                        if (mi.GetParameters().Length == 0)
                            return (Func<P>)mi.CreateDelegate(typeof(Func<P>), source);
                        break;
                }
            }
            else
            {
                var getter = CreateGetter<GivenParameter<P>>(source);
                if (getter != null)
                    return new Func<P>(() => getter());
            }

            return null;
        }
    }
}
