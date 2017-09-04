using System;
using SpiceSharp.Circuits;
using SpiceSharp.Simulations;
using SpiceSharp.Diagnostics;

namespace SpiceSharp
{
    /// <summary>
    /// A class that represents a circuit
    /// </summary>
    public class Circuit
    {
        #region Constants
        // Constants that can be used in Spice models
        public const double CHARGE = 1.6021918e-19;
        public const double CONSTCtoK = 273.15;
        public const double CONSTBoltz = 1.3806226e-23;
        public const double CONSTRefTemp = 300.15; // 27degC
        public const double CONSTroot2 = 1.41421356237;
        public const double CONSTvt0 = CONSTBoltz * (27.0 + CONSTCtoK) / CHARGE;
        public const double CONSTKoverQ = CONSTBoltz / CHARGE;
        public const double CONSTE = Math.E;
        public const double CONSTPI = Math.PI;
        #endregion

        /// <summary>
        /// Gets or sets the integration method
        /// </summary>
        public IntegrationMethod Method { get; set; }

        /// <summary>
        /// Get the nodes for the circuit
        /// </summary>
        public Nodes Nodes { get; } = new Nodes();

        /// <summary>
        /// Get the current simulation that is being run
        /// </summary>
        public ISimulation Simulation { get; private set; } = null;

        /// <summary>
        /// Get the circuit state
        /// </summary>
        public CircuitState State { get; } = new CircuitState();

        /// <summary>
        /// Gets the statistics
        /// </summary>
        public CircuitStatistics Statistics { get; } = new CircuitStatistics();

        /// <summary>
        /// Get the circuit components
        /// </summary>
        public CircuitObjects Objects { get; } = new CircuitObjects();

        /// <summary>
        /// Private variables
        /// </summary>
        private bool IsSetup = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public Circuit() { }

        /// <summary>
        /// Simulate the circuit
        /// </summary>
        /// <param name="sim">The simulation that needs to be executed</param>
        public void Simulate(ISimulation sim)
        {
            // Setup the circuit
            Setup();
            Simulation = sim;

            if (Objects.Count <= 0)
                throw new CircuitException("Circuit contains no objects");
            if (Nodes.Count <= 1)
                throw new CircuitException("Circuit contains no nodes");

            // Do temperature-dependent calculations
            foreach (var c in Objects)
                c.Temperature(this);

            // Execute the simulation
            sim.Execute(this);
        }

        /// <summary>
        /// Setup the circuit
        /// </summary>
        public void Setup()
        {
            if (IsSetup)
                return;
            IsSetup = true;

            // Rebuild the list of circuit components
            Objects.BuildOrderedComponentList();

            // Setup all devices
            foreach (var c in Objects)
                c.Setup(this);

            // Initialize the state
            State.Initialize(this);

            // Lock our nodes
            Nodes.Lock();
        }

        /// <summary>
        /// Unsetup/destroy the circuit
        /// </summary>
        public void Unsetup()
        {
            if (!IsSetup)
                return;
            IsSetup = false;

            // Remove all nodes
            Nodes.Clear();

            // Destroy state
            State.Destroy();

            // Unsetup devices
            foreach (var c in Objects)
                c.Unsetup(this);
        }

        /// <summary>
        /// Clear all objects, nodes, etc. in the circuit
        /// </summary>
        public void Clear()
        {
            // Unsetup if necessary
            Unsetup();

            // Clear all values
            Method = null;
            Nodes.Clear();
            Simulation = null;
            State.Destroy();
            Statistics.Clear();
            Objects.Clear();
        }

        /// <summary>
        /// Check the circuit
        /// </summary>
        public void Check()
        {
            CircuitCheck checker = new CircuitCheck();
            checker.Check(this);
        }
    }
}
