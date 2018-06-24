using SpiceSharp.Simulations;
using System;
using System.Linq;
using System.Reflection;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Provides values in function of time. This is an abstract class.
    /// </summary>
    public abstract class Waveform
    {
        /// <summary>
        /// Setup the waveform
        /// </summary>
        public abstract void Setup();

        /// <summary>
        /// Calculate the value of the waveform at a specific value
        /// </summary>
        /// <param name="time">The time point</param>
        /// <returns></returns>
        public abstract double At(double time);

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public abstract void Accept(TimeSimulation simulation);

        /// <summary>
        /// Clones the object.
        /// </summary>
        /// <returns>
        /// A clone of the object.
        /// </returns>
        public Waveform DeepClone()
        {
            //1. Make shallow copy of current object
            var destinationObject = (Waveform)Activator.CreateInstance(this.GetType());

            //2. Convert shallow copy to deep copy of the current object
            var members = GetType().GetTypeInfo().GetMembers(BindingFlags.Instance | BindingFlags.Public);
            foreach (var member in members)
            {
                if (member is PropertyInfo pi)
                {
                    if (pi.PropertyType == typeof(Parameter) || pi.PropertyType.GetTypeInfo().IsSubclassOf(typeof(Parameter)))
                    {
                        var parameter = (Parameter)pi.GetValue(this);

                        if (pi.CanWrite)
                        {
                            // property has a setter
                            var clonedParameter = parameter.Clone();
                            pi.SetValue(destinationObject, clonedParameter);
                        }
                        else
                        {
                            // if parameter is not given, don't deal with it
                            if (parameter is GivenParameter gp && gp.Given == false)
                            {
                                continue;
                            }

                            // property doesn't have a setter
                            var destinationParameter = (Parameter)pi.GetValue(destinationObject);
                            destinationParameter.Value = parameter.Value;
                        }
                    }
                    else
                    {
                        if (pi.PropertyType == typeof(double))
                        {
                            if (pi.CanWrite)
                            {
                                // double property has a setter, so it can be set
                                var propertyValue = (double)pi.GetValue(this);
                                pi.SetValue(destinationObject, propertyValue);
                            }
                        }
                    }
                }
                else if (member is MethodInfo mi)
                {
                    // for properties with only getter and default value there is a method instead of property
                    if (mi.Name.StartsWith("get_", StringComparison.Ordinal))
                    {
                        if (mi.ReturnType == typeof(GivenParameter) && mi.GetParameters().Length == 0)
                        {
                            var parameter = ((GivenParameter)mi.Invoke(this, new object[0]));
                            if (parameter.Given == false)
                            {
                                continue;
                            }

                            var destinationParameter = (GivenParameter)mi.Invoke(destinationObject, new object[0]);
                            destinationParameter.Value = parameter.Value;
                        }

                        if (mi.ReturnType == typeof(double) && mi.GetParameters().Length == 0)
                        {
                            var value = (double)mi.Invoke(this, new object[0]);
                            var setter = (MethodInfo)members.SingleOrDefault(member2 => member2 is MethodInfo mi2 && mi2.Name == "set_" + (mi.Name.Replace("get_", "")));
                            if (setter != null)
                            {
                                setter.Invoke(destinationObject, new object[] { value });
                            }
                        }
                    }
                }
            }

            return destinationObject;
        }
    }
}
