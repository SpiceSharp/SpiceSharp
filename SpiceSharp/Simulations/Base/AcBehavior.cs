using System;
using System.Numerics;
using SpiceSharp.Sparse;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharp.Attributes;

namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// AC behavior for circuit objects
    /// </summary>
    public abstract class AcBehavior : Behavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public AcBehavior(Identifier name = null) : base(name) { }

        /// <summary>
        /// Create an export method for AC analysis
        /// </summary>
        /// <param name="property">Property name</param>
        /// <returns></returns>
        public virtual Func<State, Complex> CreateAcExport(string property)
        {
            // Find methods to create the export
            var members = GetType().GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            foreach (var member in members)
            {
                // Check the return type (needs to be a double)
                if (member.ReturnType != typeof(Complex))
                    continue;

                // Check the name
                var names = (SpiceName[])member.GetCustomAttributes(typeof(SpiceName), true);
                bool found = false;
                foreach (var name in names)
                {
                    if (name.Name == property)
                    {
                        found = true;
                        continue;
                    }
                }
                if (!found)
                    continue;

                // Check the parameters
                var parameters = member.GetParameters();
                if (parameters.Length != 1)
                    continue;
                if (parameters[0].ParameterType != typeof(State))
                    continue;

                // Return a delegate
                return (Func<State, Complex>)member.CreateDelegate(typeof(Func<State, Complex>));
            }

            // Not found
            return null;
        }

        /// <summary>
        /// Initialize parameters for AC analysis
        /// </summary>
        public virtual void InitializeParameters(FrequencySimulation sim)
        {
            // Nothing to initialize by default
        }

        /// <summary>
        /// Get matrix pointers
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public virtual void GetMatrixPointers(Matrix matrix)
        {
            // No matrix pointers by default
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector for AC analysis
        /// </summary>
        /// <param name="sim">Frequency-based simulation</param>
        public abstract void Load(FrequencySimulation sim);
    }
}
