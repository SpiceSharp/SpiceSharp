using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Examples
{
    [TestFixture]
    public class Changing
    {
        // <example_change_parameter_entity>        
        /// <summary>
        /// The entity that will allow us to change the resistor parameters.
        /// </summary>
        /// <seealso cref="SpiceSharp.Circuits.Entity" />
        public class ParameterChanger : Entity
        {
            /// <summary>
            /// The load behavior that will allow us to change the resistor parameters.
            /// </summary>
            /// <seealso cref="SpiceSharp.Behaviors.BaseLoadBehavior" />
            private class LoadBehavior : BaseLoadBehavior
            {
                // Necessary behaviors and parameters
                private SpiceSharp.Components.ResistorBehaviors.TemperatureBehavior _temp;
                private SpiceSharp.Components.ResistorBehaviors.BaseParameters _bp;

                /// <summary>
                /// Initializes a new instance of the <see cref="LoadBehavior"/> class.
                /// </summary>
                public LoadBehavior()
                    : base("parameter changer")
                {
                }

                /// <summary>
                /// Setup the behavior.
                /// </summary>
                /// <param name="simulation">The simulation.</param>
                /// <param name="provider">The data provider.</param>
                public override void Setup(Simulation simulation, SetupDataProvider provider)
                {
                    _temp = simulation.EntityBehaviors["R2"]
                        .Get<SpiceSharp.Components.ResistorBehaviors.TemperatureBehavior>();
                    _bp = simulation.EntityParameters["R2"]
                        .Get<SpiceSharp.Components.ResistorBehaviors.BaseParameters>();
                }

                /// <summary>
                /// Loads the Y-matrix and Rhs-vector.
                /// </summary>
                /// <param name="simulation">The base simulation.</param>
                public override void Load(BaseSimulation simulation)
                {
                    var time = 0.0;
                    if (simulation is TimeSimulation ts)
                        time = ts.Method.Time;

                    // Then we need to calculate the resistance for "R2"
                    var resistance = 1.0e3 * (1 + time * 1.0e5);

                    // Change the value
                    _bp.Resistance.Value = resistance;
                    _temp.Temperature(simulation);
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ParameterChanger"/> class.
            /// </summary>
            public ParameterChanger()
                : base("parameter changer")
            {
                // Make sure the parameter changer executes before any other component
                Priority = 100;

                // Add the behavior that will change
                Behaviors.Add(typeof(BaseLoadBehavior), () => new LoadBehavior());
            }
        }
        // </example_change_parameter_entity>

        [Test]
        public void When_ChangeParameterInTransient_Expect_NoException()
        {
            // <example_change_parameter_circuit>
            // Build a circuit
            var ckt = new Circuit(
                new Resistor("R1", "in", "out", 1.0e3),
                new Resistor("R2", "out", "0", 1.0e3),
                new Capacitor("C1", "out", "0", 0.5e-9),
                new VoltageSource("V1", "in", "0", new Pulse(0, 5, 1e-6, 1e-6, 1e-6, 1e-5, 2e-5))
            );
            // </example_change_parameter_circuit>

            // <example_change_parameter_transient>
            // Create the transient analysis and exports
            var tran = new Transient("tran", 1e-6, 10e-5);
            var outputExport = new RealVoltageExport(tran, "out");
            tran.ExportSimulationData += (sender, args) =>
            {
                var time = args.Time;
                var output = outputExport.Value;
            };

            // Run the simulation
            tran.Run(ckt);
            // </example_change_parameter_transient>
        }
    }
}
