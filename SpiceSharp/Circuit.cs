using System;
using SpiceSharp.Circuits;

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
        public Simulation Simulation { get; private set; } = null;

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
        public CircuitComponents Components { get; } = new CircuitComponents();

        /// <summary>
        /// Private variables
        /// </summary>
        private bool IsSetup = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public Circuit()
        {
        }

        /// <summary>
        /// Setup the circuit
        /// </summary>
        public void Setup()
        {
            if (IsSetup)
                return;
            IsSetup = true;

            // Setup all devices
            foreach (var c in Components)
                c.Setup(this);

            // Initialize the state
            State.Initialize(this);
        }

        /// <summary>
        /// Unsetup/destroy the circuit
        /// </summary>
        public void Unsetup()
        {
            if (!IsSetup)
                return;
            IsSetup = false;

            // Unsetup devices
            foreach (var c in Components)
                c.Unsetup(this);

            // Destroy state
            State.Destroy();

            // Remove all nodes
            Nodes.Clear();
        }
    }
}
