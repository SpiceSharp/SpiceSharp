using SpiceSharp.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiceSharp.Circuits.ParallelBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="ParallelLoader"/>.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets theset of behavior types that need to be executed in parallel.
        /// </summary>
        /// <value>
        /// The parallel behavior types.
        /// </value>
        [ParameterName("parallel"), ParameterInfo("The behavior types that should be executed in parallel")]
        public HashSet<Type> ParallelBehaviors { get; } = new HashSet<Type>();

        /// <summary>
        /// Enables a behavior type to be executed in parallel.
        /// </summary>
        /// <param name="type">The type.</param>
        [ParameterName("enable"), ParameterInfo("Enables parallel execution of a behavior type", IsPrincipal = true)]
        public void EnableParallel(Type type)
        {
            ParallelBehaviors.Add(type);
        }
    }
}
