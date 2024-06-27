using System;
using System.Collections.Generic;
using System.Reflection;
using SpiceSharp.Attributes;

namespace SpiceSharp.Documentation
{
    /// <summary>
    /// Describes documentation about a member.
    /// </summary>
    public class MemberDocumentation
    {
        /// <summary>
        /// Gets a value indicating whether this member is interesting.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is interesting; otherwise, <c>false</c>.
        /// </value>
        public bool Interesting { get; } = false;

        /// <summary>
        /// Gets a value indicating whether this instance is principal.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is principal; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrincipal { get; } = false;

        /// <summary>
        /// Gets a value indicating whether the member is static.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this member is static; otherwise, <c>false</c>.
        /// </value>
        public bool IsStatic { get; }

        /// <summary>
        /// Determines whether the member describes a parameter (can set a value).
        /// </summary>
        /// <value>
        ///     <c>true</c> if the member describes a parameter; otherwise, <c>false</c>.
        /// </value>
        public bool IsParameter { get; }

        /// <summary>
        /// Determines whether the member describes a property (can return a value).
        /// </summary>
        /// <value>
        ///     <c>true</c> if the member describes a property; otherwise, <c>false</c>.
        /// </value>
        public bool IsProperty { get; }

        /// <summary>
        /// Gets the names of the member.
        /// </summary>
        /// <value>
        /// The names of the member.
        /// </value>
        public IReadOnlyList<string> Names { get; }

        /// <summary>
        /// Gets the description of the property and/or parameter.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; }

        /// <summary>
        /// Gets the type of the parameter and/or property.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public Type MemberType { get; }

        /// <summary>
        /// Gets the base member type of the parameter and/or property.
        /// </summary>
        /// <value>
        /// The base type.
        /// </value>
        public Type BaseType { get; }

        /// <summary>
        /// Gets the member.
        /// </summary>
        /// <value>
        /// The member.
        /// </value>
        public MemberInfo Member { get; }

        /// <summary>
        /// Gets the units of the member.
        /// </summary>
        /// <value>
        /// The units.
        /// </value>
        public string Units { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberDocumentation"/> class.
        /// </summary>
        /// <param name="member">The member info.</param>
        public MemberDocumentation(MemberInfo member)
        {
            Member = member.ThrowIfNull(nameof(member));
            switch (member)
            {
                case PropertyInfo propertyInfo:
                    if (propertyInfo.GetMethod.IsPublic)
                        IsProperty = true;
                    if (propertyInfo.SetMethod.IsPublic)
                        IsParameter = true;
                    MemberType = propertyInfo.PropertyType;
                    BaseType = GetBaseType(propertyInfo.PropertyType);
                    break;

                case FieldInfo fieldInfo:
                    IsProperty = true;
                    IsParameter = true;
                    MemberType = fieldInfo.FieldType;
                    BaseType = GetBaseType(fieldInfo.FieldType);
                    break;

                case MethodInfo methodInfo:
                    var parameters = methodInfo.GetParameters();
                    if (parameters.Length == 0)
                    {
                        IsProperty = true;
                        MemberType = methodInfo.ReturnType;
                        BaseType = GetBaseType(methodInfo.ReturnType);
                    }
                    else if (parameters.Length == 1)
                    {
                        IsParameter = true;
                        MemberType = parameters[0].ParameterType;
                        BaseType = GetBaseType(parameters[0].ParameterType);
                    }
                    else
                        throw new SpiceSharpException($"The method {methodInfo.Name} of type {member.DeclaringType} does not describe a parameter or property.");
                    break;
            }

            // Go through the attributes and figure our what its properties are
            var names = new List<string>();
            foreach (var attribute in Member.GetCustomAttributes())
            {
                switch (attribute)
                {
                    case ParameterNameAttribute nameAttribute:
                        names.Add(nameAttribute.Name);
                        break;

                    case ParameterInfoAttribute infoAttribute:
                        IsPrincipal = infoAttribute.IsPrincipal;
                        Description = infoAttribute.Description;
                        Units = infoAttribute.Units;
                        Interesting = infoAttribute.Interesting;
                        break;
                }
            }
            Names = names.ToArray();
        }

        private Type GetBaseType(Type type)
        {
            if (type.IsGenericType)
            {
                if (type.BaseType == typeof(GivenParameter<>))
                    return type.GenericTypeArguments[0];
            }
            return type;
        }
    }
}
