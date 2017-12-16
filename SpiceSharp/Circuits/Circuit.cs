using System;
using SpiceSharp.Circuits;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp
{
    /// <summary>
    /// Represents an electronic circuit.
    /// </summary>
    public class Circuit
    {
        #region Constants
        // Constants that can be used in Spice models
        public const double CHARGE = 1.6021918e-19;
        public const double CONSTCtoK = 273.15;
        public const double CONSTBoltz = 1.3806226e-23;
        public const double CONSTRefTemp = 300.15; // 27degC
        public static double CONSTroot2 = Math.Sqrt(2); // 1.4142135623730951;
        public const double CONSTvt0 = CONSTBoltz * (27.0 + CONSTCtoK) / CHARGE;
        public const double CONSTKoverQ = CONSTBoltz / CHARGE;
        public static double CONSTE = Math.Exp(1.0);
        public const double CONSTPI = Math.PI;
        #endregion

        /// <summary>
        /// Gets or sets the integration method used in transient simulations
        /// It should be set by the simulation
        /// </summary>
        public IntegrationMethod Method { get; set; }

        /// <summary>
        /// Get all nodes in the circuit
        /// Using nodes is only valid after calling <see cref="Setup"/>
        /// </summary>
        public Nodes Nodes { get; } = new Nodes();

        /// <summary>
        /// Gets the current simulation that is being run by the circuit
        /// </summary>
        public Simulation Simulation { get;  set; } = null;

        /// <summary>
        /// Gets the current state of the circuit
        /// </summary>
        public State State { get; } = new State();

        /// <summary>
        /// Gets statistics
        /// </summary>
        public Statistics Statistics { get; } = new Statistics();

        /// <summary>
        /// Gets a collection of all circuit objects
        /// </summary>
        public Entities Objects { get; } = new Entities();

        /// <summary>
        /// Constructor
        /// </summary>
        public Circuit()
        {
        }

        /// <summary>
        /// Clear all objects, nodes, etc. in the circuit
        /// </summary>
        public void Clear()
        {
            // Clear all values
            Method = null;
            Nodes.Clear();
            Simulation = null;
            State.Destroy();
            Objects.Clear();
        }

        /// <summary>
        /// Check the circuit for floating nodes, voltage loops and more
        /// </summary>
        public void Check()
        {
            Validator checker = new Validator();
            checker.Check(this);
        }
    }
}
