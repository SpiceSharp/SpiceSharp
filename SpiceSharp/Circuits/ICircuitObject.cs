using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// An interface that can be used by a Circuit-object
    /// </summary>
    public interface ICircuitObject
    {
        /// <summary>
        /// Get the name of the object
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get the priority of this object
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Setup the component
        /// </summary>
        /// <param name="ckt">The circuit</param>
        void Setup(Circuit ckt);

        /// <summary>
        /// Use initial conditions for the device
        /// </summary>
        /// <param name="ckt"></param>
        void SetIc(Circuit ckt);

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt"></param>
        void Temperature(Circuit ckt);

        /// <summary>
        /// Load the component in the current circuit state
        /// </summary>
        /// <param name="ckt">The circuit</param>
        void Load(Circuit ckt);

        /// <summary>
        /// Load the component in the current circuit state for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        void AcLoad(Circuit ckt);

        /// <summary>
        /// Accept the current timepoint as the solution
        /// </summary>
        /// <param name="ckt">The circuit</param>
        void Accept(Circuit ckt);

        /// <summary>
        /// Unsetup/destroy the component
        /// </summary>
        /// <param name="ckt">The circuit</param>
        void Unsetup(Circuit ckt);
    }
}
