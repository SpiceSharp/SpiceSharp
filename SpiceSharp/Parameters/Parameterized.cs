using System;
using System.Collections.Generic;
using System.Reflection;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Parameters
{
    /// <summary>
    /// An abstract class that contains parameters
    /// These parameters can be write-only, read-only, ...
    /// A table of parameters is also available for such classes.
    /// </summary>
    public abstract class Parameterized
    {
        /// <summary>
        /// An enumeration of possible access types for parameters
        /// </summary>
        public enum ParameterAccess
        {
            Required = 0x4000,
            Set = 0x2000,
            Ask = 0x1000,
            Redundant = 0x0010000,
            Principal = 0x0020000,
            Ac = 0x0040000,
            Ac_only = 0x0080000,
            Noise = 0x0100000,
            Nonsense = 0x0200000,

            Uninteresting = 0x2000000, // For "show" command: do not print value in a table by default
            IOP = Set | Ask,
            IOPU = Set | Ask | Uninteresting,
            IOPP = Set | Ask | Principal,
            IOPA = Set | Ask | Ac,
            IOPAU = Set | Ask | Ac | Uninteresting,
            IOPAP = Set | Ask | Ac | Principal,
            IOPAA = Set | Ask | Ac_only,
            IOPAAU = Set | Ask | Ac_only | Uninteresting,
            IOPPA = Set | Ask | Ac_only | Principal,
            IOPN = Set | Ask | Noise,
            IOPR = Set | Ask | Redundant,
            IOPX = Set | Ask | Nonsense,
            IOPXU = Set | Ask | Nonsense | Uninteresting,

            IP = Set,
            OP = Ask,
            OPU = Ask | Uninteresting,
            OPR = Ask | Redundant,
            P = 0x00
        }

        /// <summary>
        /// Get the name of the object
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Get a list of all parameters
        /// Big dictionaries can be stored statically to save memory
        /// </summary>
        public abstract Dictionary<string, ParameterInfo> Parameters { get; }
        
        /// <summary>
        /// This class completely describes a parameter
        /// </summary>
        public class ParameterInfo
        {
            /// <summary>
            /// Get the access flags for this parameter
            /// </summary>
            public ParameterAccess Access { get; }

            /// <summary>
            /// Get the type of the parameter
            /// </summary>
            public Type Type { get; }

            /// <summary>
            /// Get the description of the parameter
            /// </summary>
            public string Description { get; }

            /// <summary>
            /// Usually, a Parameter(double) is linked
            /// </summary>
            public string Link = null;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="access">The access flag</param>
            /// <param name="id">A unique identifier</param>
            /// <param name="type">The type of the value</param>
            /// <param name="description">The description</param>
            public ParameterInfo(ParameterAccess access, Type type, string description, string link = null)
            {
                Access = access;
                Type = type;
                Description = description;
                Link = link;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public Parameterized(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Specify a parameter for this object
        /// </summary>
        /// <param name="id">The parameter identifier</param>
        /// <param name="value">The value</param>
        /// <param name="ckt">The circuit if applicable</param>
        public virtual void Set(string name, object value, Circuit ckt = null)
        {
            // Check existence of the parameter
            if (Parameters == null || !Parameters.ContainsKey(name))
            {
                CircuitWarning.Warning(this, $"{Name}: Unrecognized parameter {name}");
                return;
            }

            // Check access flag of the parameter
            var info = Parameters[name];
            if (!info.Access.HasFlag(ParameterAccess.Set))
            {
                CircuitWarning.Warning(this, $"{Name}: Parameter {name} is not accessible");
                return;
            }

            // We can treat this as a normal Parameter<> access
            if (info.Link != null)
            {
                if (value.GetType() == info.Type)
                {
                    var a = GetType().GetProperties();
                    object parameter = GetType().GetProperty(info.Link, BindingFlags.Instance | BindingFlags.Public).GetValue(this);
                    if (parameter != null && parameter is IParameter)
                        ((IParameter)parameter).Set(value);
                }
                else
                    throw new CircuitException($"{Name}: Type mismatch for parameter {name}");
            }
        }

        /// <summary>
        /// Request a parameter
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="ckt">The circuit if applicable</param>
        /// <returns></returns>
        public virtual object Ask(string name, Circuit ckt = null)
        {
            // Check existence of the parameter
            if (Parameters == null || !Parameters.ContainsKey(name))
            {
                CircuitWarning.Warning(this, $"{Name}: Unrecognized parameter {name}");
                return null;
            }

            // Check access flag of the parameter
            var info = Parameters[name];
            if (!info.Access.HasFlag(ParameterAccess.Ask))
            {
                CircuitWarning.Warning(this, $"{Name}: Parameter {name} is not accessible for reading");
                return null;
            }

            // Pass through to main method
            if (info.Link != null)
            {
                object parameter = GetType().GetProperty(info.Link).GetValue(this, null);
                if (parameter != null && parameter is IParameter)
                    return ((IParameter)parameter).Get();
                else
                    return parameter;
            }

            return null;
        }
    }
}
