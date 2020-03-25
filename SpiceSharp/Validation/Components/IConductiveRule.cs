using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An <see cref="IRule"/> that allows specifying an unconditionally conductive path.
    /// </summary>
    /// <seealso cref="IRule" />
    public interface IConductiveRule : IRule
    {
        /// <summary>
        /// Specifies variables as being unconditionally connected by a conductive path.
        /// </summary>
        /// <param name="subject">The subject that applies the conductive paths.</param>
        /// <param name="variables">The variables that are connected.</param>
        void AddPath(IRuleSubject subject, params IVariable[] variables);

        /// <summary>
        /// Specifies variables as being connected by a conductive path of the specified type.
        /// </summary>
        /// <param name="subject">The subject that applies the conductive paths.</param>
        /// <param name="type">The type of path between these variables.</param>
        /// <param name="variables">The variables that are connected.</param>
        void AddPath(IRuleSubject subject, ConductionTypes type, params IVariable[] variables);
    }

    /// <summary>
    /// An enumeration of conduvtive path types.
    /// </summary>
    [Flags]
    public enum ConductionTypes
    {
        /// <summary>
        /// Indicates an unconnected path.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Indicates a path that is conductive for DC solutions.
        /// </summary>
        Dc = 0x01,

        /// <summary>
        /// Indicates a path that is conductive for all non-zero frequencuencies.
        /// </summary>
        Ac = 0x02,

        /// <summary>
        /// Indicates a path that is conductive for time-varying signals. Ie. it has
        /// a solution if an initial condition is known.
        /// </summary>
        Time = 0x02,

        /// <summary>
        /// Indicates a path that is unconditionally conducting.
        /// </summary>
        All = 0x03
    }
}
