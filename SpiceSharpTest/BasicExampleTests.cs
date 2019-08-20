using System;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest
{
    [TestFixture]
    public class BasicExampleTests
    {
        [Test]
        public void When_BasicResistor_Expect_NoException()
        {
            // <example_structure_resistor>
            // Build the circuit
            var ckt = new Circuit(
                new Resistor("R1", "a", "b", 1e3)
            );

            // Change the value of the resistor
            var resParameters = ckt["R1"].ParameterSets;
            resParameters.Get<SpiceSharp.Components.ResistorBehaviors.BaseParameters>().Resistance.Value = 2.0e3;
            // </example_structure_resistor>
            // <example_structure_resistor_2>
            // Using the ParameterNameAttribute
            ckt["R1"].SetParameter("resistance", 2.0e3);
            ckt["R1"].ParameterSets.SetParameter("resistance", 2.0e3);

            // Using the ParameterInfoAttributes IsPrincipal=true
            ckt["R1"].ParameterSets.SetPrincipalParameter(2.0e3);
            // </example_structure_resistor_2>
        }

        [Test]
        public void When_BasicSimulation_Expect_NoException()
        {
            // <example_structure_dc>
            // Build the simulation
            var dc = new DC("DC 1");

            // Add a sweep
            var dcConfig = dc.Configurations.Get<DCConfiguration>();
            dcConfig.Sweeps.Add(new SweepConfiguration("V1", 0.0, 3.3, 0.1));
            // </example_structure_dc>
            // <example_structure_dc_2>
            var baseConfig = dc.Configurations.Get<BaseConfiguration>();
            baseConfig.RelativeTolerance = 1e-4;
            baseConfig.AbsoluteTolerance = 1e-10;
            // </example_structure_dc_2>
        }

        [Test]
        public void When_BasicParameters_Expect_NoException()
        {
            // Create the mosfet
            var model = new Mosfet1Model("M1");
            var parameters =
                model.ParameterSets.Get<SpiceSharp.Components.MosfetBehaviors.Level1.ModelBaseParameters>();

            // <example_parameters_mos1_creategetter>
            // Create a getter for the nominal temperature of the mosfet1 model
            var tnomGetter = parameters.CreateGetter<double>("tnom");
            double temperature = tnomGetter(); // In degrees Celsius
            // </example_parameters_mos1_creategetter>
            // <example_parameters_mos1_createsetter>
            // Create a setter for the gate-drain overlap capacitance of the mosfet1 model
            var cgdoSetter = parameters.CreateSetter<double>("cgdo");
            cgdoSetter(1e-15); // 1pF
            // </example_parameters_mos1_createsetter>
            // <example_parameters_mos1_getparameter>
            // Get the parameter that describes the oxide thickness of the mosfet1 model
            var toxParameter = parameters.GetParameter<double>("tox");
            // </example_parameters_mos1_getparameter>
            // <example_parameters_mos1_setparameter>
            // Flag the model as a PMOS type
            parameters.SetParameter("pmos", true);
            // </example_parameters_mos1_setparameter>
            // <example_parameters_res_setparameter>
            // Set the resistance of the resistor
            var res = new Resistor("R1");
            res.ParameterSets.SetPrincipalParameter(2.0e3); // 2kOhm
            // </example_parameters_res_setparameter>
        }

        [Test]
        public void When_BasicCircuit_Expect_NoException()
        {
            // <example01_build>
            // Build the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "out", 1.0e4),
                new Resistor("R2", "out", "0", 2.0e4)
                );
            // </example01_build>
            // <example01_simulate>
            // Create a DC simulation that sweeps V1 from -1V to 1V in steps of 100mV
            var dc = new DC("DC 1", "V1", -1.0, 1.0, 0.2);

            // Catch exported data
            dc.ExportSimulationData += (sender, args) =>
            {
                var input = args.GetVoltage("in");
                var output = args.GetVoltage("out");
            };
            dc.Run(ckt);
            // </example01_simulate>
        }

        [Test]
        public void When_BasicCircuitExports_Expect_NoException()
        {
            // Build the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "out", 1.0e4),
                new Resistor("R2", "out", "0", 2.0e4)
            );

            // <example01_simulate2>
            // Create a DC simulation that sweeps V1 from -1V to 1V in steps of 100mV
            var dc = new DC("DC 1", "V1", -1.0, 1.0, 0.2);

            // Create exports
            Export<double> inputExport = new RealVoltageExport(dc, "in");
            Export<double> outputExport = new RealVoltageExport(dc, "out");
            Export<double> currentExport = new RealPropertyExport(dc, "V1", "i");

            // Catch exported data
            dc.ExportSimulationData += (sender, args) =>
            {
                var input = inputExport.Value;
                var output = outputExport.Value;
                var current = currentExport.Value;
            };
            dc.Run(ckt);
            // </example01_simulate2>
        }

        [Test]
        public void When_NMOSIVCharacteristic_Expect_NoException()
        {
            // <example_DC>
            // Make the bipolar junction transistor
            var nmos = new Mosfet1("M1") {Model = "example"};
            nmos.Connect("d", "g", "0", "0");
            var nmosmodel = new Mosfet1Model("example");
            nmosmodel.SetParameter("kp", 150.0e-3);

            // Build the circuit
            var ckt = new Circuit(
                new VoltageSource("Vgs", "g", "0", 0),
                new VoltageSource("Vds", "d", "0", 0),
                nmosmodel,
                nmos
                );

            // Sweep the base current and vce voltage
            var dc = new DC("DC 1", new[]
            {
                new SweepConfiguration("Vgs", 0, 3, 0.2),
                new SweepConfiguration("Vds", 0, 5, 0.1),                
            });
            
            // Export the collector current
            var currentExport = new RealPropertyExport(dc, "M1", "id");

            // Run the simulation
            dc.ExportSimulationData += (sender, args) =>
            {
                var vgsVoltage = dc.Sweeps[0].CurrentValue;
                var vdsVoltage = dc.Sweeps[1].CurrentValue;
                var current = currentExport.Value;
            };
            dc.Run(ckt);
            // </example_DC>
        }

        [Test]
        public void When_RCFilterAC_Expect_NoException()
        {
            // <example_AC>
            // Build the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0.0)
                    .SetParameter("acmag", 1.0),
                new Resistor("R1", "in", "out", 10.0e3),
                new Capacitor("C1", "out", "0", 1e-6)
                );

            // Create the simulation
            var ac = new AC("AC 1", new DecadeSweep(1e-2, 1.0e3, 5));

            // Make the export
            var exportVoltage = new ComplexVoltageExport(ac, "out");

            // Simulate
            ac.ExportSimulationData += (sender, args) =>
            {
                var output = exportVoltage.Value;
                var decibels = 10.0 * Math.Log10(output.Real * output.Real + output.Imaginary * output.Imaginary);
            };
            ac.Run(ckt);
            // </example_AC>
        }

        [Test]
        public void When_RCFilterTransient_Expect_NoException()
        {
            // <example_Transient>
            // Build the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pulse(0.0, 5.0, 0.01, 1e-3, 1e-3, 0.02, 0.04)),
                new Resistor("R1", "in", "out", 10.0e3),
                new Capacitor("C1", "out", "0", 1e-6)
            );

            // Create the simulation
            var tran = new Transient("Tran 1", 1e-3, 0.1);

            // Make the exports
            var inputExport = new RealVoltageExport(tran, "in");
            var outputExport = new RealVoltageExport(tran, "out");

            // Simulate
            tran.ExportSimulationData += (sender, args) =>
            {
                var input = inputExport.Value;
                var output = outputExport.Value;
            };
            tran.Run(ckt);
            // </example_Transient>
        }

        [Test]
        public void When_ResistorModified_Expect_NoException()
        {
            // <example_Stochastic>
            // Build the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "0", 1.0e3));
            
            // Create the simulation
            var op = new OP("Op 1");
            
            // Attach events to apply stochastic variation
            var rndGenerator = new Random();
            var counter = 0;
            op.BeforeExecute += (sender, args) =>
            {
                // Apply a random value of 1kOhm with 5% tolerance
                var value = 950 + 100 * rndGenerator.NextDouble();
                var sim = (Simulation) sender;
                sim.EntityParameters["R1"].SetParameter("resistance", value);
            };
            op.AfterExecute += (sender, args) =>
            {
                // Run 10 times
                counter++;
                args.Repeat = counter < 10;
            };

            // Make the exports
            var current = new RealPropertyExport(op, "R1", "i");
            
            // Simulate
            op.ExportSimulationData += (sender, args) =>
            {
                // This will run 1o times
                var result = current.Value;
            };
            op.Run(ckt);
            // </example_Stochastic>
        }
    }
}
