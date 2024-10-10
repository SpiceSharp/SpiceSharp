using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using SpiceSharpTest.Models;
using System;
using System.Collections.Generic;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class TransientTests : Framework
    {
        [Test]
        public void When_RCFilterConstantTransientTrapezoidal_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10),
                new Capacitor("C1", "out", "0", 20)
            );

            // Create the transient analysis
            var tran = new Transient("tran 1", 1.0, 10.0);
            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
                Assert.That(tran.GetVoltage("out"), Is.EqualTo(10.0).Within(1e-12));

            // Let's run the simulation twice to check if it is consistent
            try
            {
                foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
                    Assert.That(tran.GetVoltage("out"), Is.EqualTo(10.0).Within(1e-12));
            }
            catch (Exception)
            {
                throw new Exception(@"Cannot run transient analysis twice");
            }
        }

        [Test]
        public void When_RCFilterConstantTransientGear_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10),
                new Capacitor("C1", "out", "0", 20));

            // Create the transient analysis
            var tran = new Transient("tran 1", new Gear { InitialStep = 1, StopTime = 10 });
            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
                Assert.That(tran.GetVoltage("out"), Is.EqualTo(10.0).Within(1e-10));

            // Let's run the simulation twice to check if it is consistent
            try
            {
                foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
                    Assert.That(tran.GetVoltage("out"), Is.EqualTo(10.0).Within(1e-10));
                tran.Run(ckt);
            }
            catch (Exception)
            {
                throw new Exception(@"Cannot run transient analysis twice");
            }
        }

        [Test]
        public void When_FloatingRTransient_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10.0)
            );

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-6, 10.0);
            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                Assert.That(tran.GetVoltage("out"), Is.EqualTo(10.0).Within(1e-12));
            }
        }

        [Test]
        public void When_LazyLoadExport_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10),
                new Resistor("R1", "in", "0", 1e3));

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-6, 10.0);
            IExport<double> export = null;
            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                // If the time > 5.0 then start exporting our stuff
                if (tran.Time > 5.0)
                {
                    export ??= new RealPropertyExport(tran, "R1", "i");
                    Assert.That(export.Value, Is.EqualTo(10.0 / 1e3).Within(1e-12));
                }
            }
        }


        [Test]
        public void When_IsNotValidExport_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10),
                new Resistor("R1", "in", "0", 1e3));

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-6, 10.0);
            IExport<double> export = null;
            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                // If the time > 5.0 then start exporting our stuff
                if (tran.Time > 5.0)
                {
                    export ??= new RealPropertyExport(tran, "rx", "i");
                    Assert.That(export.IsValid, Is.False);
                }
            }
        }

        [Test]
        public void When_IsValidExport_Expect_Reference()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10),
                new Resistor("R1", "in", "0", 1e3));

            // Create the transient analysis
            var tran = new Transient("Tran 1", 1e-6, 10.0);
            IExport<double> export = null;
            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
            {
                // If the time > 5.0 then start exporting our stuff
                if (tran.Time > 5.0)
                {
                    export ??= new RealPropertyExport(tran, "R1", "i");
                    Assert.That(export.IsValid, Is.True);
                }
            }
        }

        [Test]
        public void When_FixedEuler_Expect_NoException()
        {
            // Create a circuit with a nonlinear component
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pulse(0, 5, 1e-6, 1e-6, 1e-6, 1e-5, 2e-5)),
                new NonlinearResistor("NLR1", "in", "out")
                    .SetParameter("a", 100.0)
                    .SetParameter("b", 0.7),
                new Capacitor("C1", "out", "0", 1.0e-9)
                );

            // Create a transient analysis using Backward Euler with fixed timesteps
            var tran = new Transient("tran", new FixedEuler { Step = 1e-7, StopTime = 10e-5 });
            foreach (int _ in tran.Run(ckt)) { }
        }

        [Test]
        public void When_FixedTrapezoidal_Expect_NoException()
        {
            // Create a circuit with a nonlinear component
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pulse(0, 5, 1e-6, 1e-6, 1e-6, 1e-5, 2e-5)),
                new NonlinearResistor("NLR1", "in", "out")
                    .SetParameter("a", 100.0)
                    .SetParameter("b", 0.7),
                new Capacitor("C1", "out", "0", 1.0e-9)
                );

            // Create a transient analysis using Backward Euler with fixed timesteps
            var tran = new Transient("tran", new FixedTrapezoidal { Step = 1e-7, StopTime = 10e-5 });
            foreach (int _ in tran.Run(ckt)) { }
        }

        [Test]
        public void When_FixedEulerDerivative_Expect_Reference()
        {
            // Test a single derivative state
            double f = 100.0;
            double w = 2 * Math.PI * f;
                 
            var tran = new Transient("tran", new FixedEuler { Step = 1e-7, StopTime = 1 / f });
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Sine(0, 1, f)),

                // Fixed Euler has the worst accuracy, relaxed errors.
                new DerivativeTester(time => w * Math.Cos(w * time), 0.0, 1e-3, 1e-1));
            foreach (int _ in tran.Run(ckt)) { }
        }

        [Test]
        public void When_FixedTrapezoidalDerivative_Expect_Reference()
        {
            // Test a single derivative state
            double f = 100.0;
            double w = 2 * Math.PI * f;

            var tran = new Transient("tran", new FixedTrapezoidal { Step = 1e-7, StopTime = 1 / f });
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Sine(0, 1, f)),

                // Fixed Euler has the worst accuracy, relaxed errors.
                new DerivativeTester(time => w * Math.Cos(w * time), 0.0, 1e-4, 1e-6));
            foreach (int _ in tran.Run(ckt)) { }
        }

        [Test]
        public void When_FixedEulerIntegral_Expect_Reference()
        {
            // Test a single derivative state
            double f = 100.0;
            double w = 2 * Math.PI * f;

            var tran = new Transient("tran", new FixedEuler { Step = 1e-7, StopTime = 1 / f });
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Sine(0, 1, f)),

                // Fixed Euler has the worst accuracy, relaxed errors.
                new IntegralTester(time => (1 - Math.Cos(w * time)) / w, 0, 1e-3, 1e-8));
            foreach (int _ in tran.Run(ckt)) { }
        }

        [Test]
        public void When_FixedTrapezoidalIntegral_Expect_Reference()
        {
            // Test a single derivative state
            double f = 100.0;
            double w = 2 * Math.PI * f;

            var tran = new Transient("tran", new FixedTrapezoidal { Step = 1e-7, StopTime = 1 / f });
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Sine(0, 1, f)),

                // Fixed trapezoidal has worse accuracy, relaxed errors.
                new IntegralTester(time => (1 - Math.Cos(w * time)) / w, 0, 1e-4, 1e-6));
            foreach (int _ in tran.Run(ckt)) { }
        }

        [Test]
        public void When_TrapezoidalDerivative_Expect_Reference()
        {
            // Test a single derivative state
            double f = 100.0;
            double w = 2 * Math.PI * f;

            var tran = new Transient("tran", new Trapezoidal { InitialStep = 1e-9, MaxStep = 1e-5, StopTime = 1 / f });
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Sine(0, 1, f)),

                // Trapezoidal integration allows for more accurate derivatives
                new DerivativeTester(time => w * Math.Cos(w * time), 0.0, 1e-3, 1e-3));
            foreach (int _ in tran.Run(ckt)) { }
        }

        [Test]
        public void When_TrapezoidalIntegral_Expect_Reference()
        {
            // Test a single derivative state
            double f = 100.0;
            double w = 2 * Math.PI * f;

            var tran = new Transient("tran", new Trapezoidal { InitialStep = 1e-9, MaxStep = 1e-5, StopTime = 1 / f });
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Sine(0, 1, f)),

                // Fixed Euler has the worst accuracy, we only limit to an absolute error of 0.1.
                new IntegralTester(time => (1 - Math.Cos(w * time)) / w, 0, 1e-6, 1e-6));
            foreach (int _ in tran.Run(ckt)) { }
        }

        [Test]
        public void When_GearDerivative_Expect_Reference()
        {
            // Test a single derivative state
            double f = 100.0;
            double w = 2 * Math.PI * f;

            var tran = new Transient("tran", new Gear { InitialStep = 1e-9, MaxStep = 1e-5, StopTime = 1 / f });
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Sine(0, 1, f)),

                // Trapezoidal integration allows for more accurate derivatives
                new DerivativeTester(time => w * Math.Cos(w * time), 0.0, 1e-3, 1e-3));
            foreach (int _ in tran.Run(ckt)) { }
        }

        [Test]
        public void When_GearIntegral_Expect_Reference()
        {
            // Test a single derivative state
            double f = 100.0;
            double w = 2 * Math.PI * f;

            var tran = new Transient("tran", new Gear { InitialStep = 1e-9, MaxStep = 1e-5, StopTime = 1 / f });
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Sine(0, 1, f)),

                // Fixed Euler has the worst accuracy, we only limit to an absolute error of 0.1.
                new IntegralTester(time => (1 - Math.Cos(w * time)) / w, 0, 1e-6, 1e-6));
            foreach (int _ in tran.Run(ckt)) { }
        }

        [Test]
        public void When_FloatingCapacitor_Expect_Reference()
        {
            // Build the circuit
            var ckt = new Circuit(
                new CurrentSource("I1", "in", "0", 0.1),
                new Capacitor("C1", "in", "out", 1e-6),
                new Resistor("R2", "out", "0", 2.0e3)
                );

            // Build the simulation
            var tran = new Transient("tran", 1e-6, 10e-6);
            var exports = new IExport<double>[]
            {
                new RealVoltageExport(tran, "in"),
                new RealVoltageExport(tran, "out")
            };
            var references = new Func<double, double>[]
            {
                time => time > 0 ? -0.1 / 1e-6 * time - 200.0 : 0.0,
                time => time > 0 ? -200.0 : 0.0
            };

            // Set initial conditions
            tran.TimeParameters.InitialConditions["in"] = 0.0;

            // Analyze
            AnalyzeTransient(tran, ckt, exports, references);
        }

        [Test]
        public void When_LargeExample_Expect_Reference()
        {
            // This is badly conditioned problem. We test the limit of the solver here.

            // First create the models
            var diodeModelA = new DiodeModel("DA")
                .SetParameter("n", 0.1e-3);
            var diodeModelB = new DiodeModel("DB")
                .SetParameter("is", 100e-14);
            var bjtModelQp1 = new BipolarJunctionTransistorModel("QP1")
                .SetParameter("pnp", true)
                .SetParameter("is", 16e-15)
                .SetParameter("bf", 1700.0);
            var bjtModelQp2 = new BipolarJunctionTransistorModel("QP2")
                .SetParameter("pnp", true)
                .SetParameter("is", 16e-15)
                .SetParameter("bf", 1610.5);
            SpiceSharpWarning.WarningGenerated += (sender, args) => Console.WriteLine(args.Message);

            var ckt = new Circuit(
                diodeModelA,
                diodeModelB,
                bjtModelQp1,
                bjtModelQp2,
                new VoltageSource("test:VS1", "VDD", "test:11", 0),
                new CurrentSource("test:IBIAS", "test:11", "test:12", 61.3e-6),
                new Diode("test:DBIAS", "VEE", "test:12", "DA"),
                new Resistor("test:RE1", "test:12", "test:91", 1.005e3),
                new Resistor("test:RE2", "test:12", "test:92", 1.005e3),
                new BipolarJunctionTransistor("test:QI1", "test:13", "test:15", "test:91", "test:91", "QP1"),
                new BipolarJunctionTransistor("test:QI2", "test:14", "test:16", "test:92", "test:92", "QP2"),
                new Resistor("test:RIN1", "test:15", "test:20", 3e6),
                new Resistor("test:RIN2", "test:16", "test:20", 3e6),
                new Diode("test:DIN1", "test:15", "VDD", "DB"),
                new Diode("test:DIN2", "VEE", "test:15", "DB"),
                new Diode("test:DIN3", "test:16", "VDD", "DB"),
                new Diode("test:DIN4", "VEE", "test:16", "DB"),
                new VoltageControlledVoltageSource("test:ECR1", "test:15", "test:32", "test:111", "test:20", 1),
                new VoltageSource("test:VOS", "test:17", "test:32", 1e-3),
                new Diode("test:DP1", "test:17", "test:112", "DB"),
                new Diode("test:DP2", "test:112", "test:113", "DB"),
                new Diode("test:DP3", "test:113", "test:16", "DB"),
                new Diode("test:DP4", "test:16", "test:114", "DB"),
                new Diode("test:DP5", "test:114", "test:115", "DB"),
                new Diode("test:DP6", "test:115", "test:17", "DB"),
                new Resistor("test:RP1", "test:17", "INP", 3.5e3),
                new Resistor("test:RP2", "test:16", "INN", 3.5e3),
                new Resistor("test:RC1", "test:13", "VEE", 1.856e3),
                new Resistor("test:RC2", "test:14", "VEE", 1.856e3),
                new Capacitor("test:C1", "test:13", "test:14", 14.0e-12),
                new VoltageControlledCurrentSource("test:GA", "test:21", "test:20", "test:14", "test:13",
                    326.8e-6),
                new CurrentControlledCurrentSource("test:FSUP", "VEE", "VDD", "test:VS1", 1),
                new Resistor("test:RO1", "test:21", "test:20", 30606),
                new VoltageControlledCurrentSource("test:GB", "test:22", "test:20", "test:21", "test:20", 1),
                new Resistor("test:RO2", "test:22", "test:20", 20e3),
                new VoltageControlledVoltageSource("test:EF", "test:23", "test:20", "test:22", "test:20", 1),
                new Capacitor("test:CC", "test:23", "test:21", 36.8e-12),
                new VoltageControlledVoltageSource("test:EG", "test:20", "VEE", "VDD", "VEE", 0.5),
                new VoltageControlledCurrentSource("test:GCP", "test:22", "test:20", "test:110", "test:20", 10),
                new Diode("test:DVL1", "test:22", "test:33", "DA"),
                new VoltageSource("test:VMIN1", "test:59", "test:33", 6.85e-3),
                new Diode("test:DVL2", "test:34", "test:22", "DA"),
                new VoltageSource("test:VMIN2", "test:34", "test:60", 4e-3),
                new VoltageControlledVoltageSource("test:ELIM2", "test:59", "test:61", "VDD", "VEE", 0.5),
                new VoltageControlledVoltageSource("test:ELIM1", "test:63", "test:60", "VDD", "VEE", 0.5),
                new VoltageControlledVoltageSource("test:ECOMP2", "test:62", "test:20", "test:65", "test:66", 1),
                new VoltageControlledVoltageSource("test:ECOMP1", "test:64", "test:20", "test:65", "test:66", 1),
                new VoltageControlledVoltageSource("test:EOUT", "test:65", "test:20", "test:22", "test:20", 1),
                new Resistor("test:ROUT1", "test:65", "test:66", 150),
                new VoltageSource("test:VIS3", "test:66", "test:67", 0),
                new Diode("test:DSC1", "test:67", "test:68", "DA"),
                new Diode("test:DSC2", "test:69", "test:67", "DA"),
                new Diode("test:DSC3", "test:69", "test:70", "DA"),
                new Diode("test:DSC4", "test:70", "test:68", "DA"),
                new CurrentSource("test:ISC1", "test:68", "test:69", 25e-3),
                new Resistor("test:RSC", "test:68", "test:69", 10e6),
                new Resistor("test:ROUT2", "test:70", "OUT", 0.1e-3),
                new Resistor("test:RLOAD", "OUT", "test:20", 10e6),
                new Diode("test:DSUP", "VEE", "VDD", "DB"),
                new Resistor("test:RSUP", "VDD", "VEE", 200e3),
                new CurrentSource("test:ISUP", "VDD", "VEE", 95e-6),
                new CurrentControlledCurrentSource("test:FSUP1", "test:20", "test:77", "test:VIS3", 1),
                new Capacitor("test:CSUP", "test:77", "test:20", 1e-12),
                new Diode("test:DSUP1", "test:20", "test:77", "DB"),
                new Diode("test:DSUP2", "test:77", "test:78", "DB"),
                new VoltageSource("test:VIS4", "test:78", "test:20", 0),
                new CurrentControlledCurrentSource("test:FSUP2", "VDD", "VEE", "test:VIS4", 1),
                new VoltageControlledVoltageSource("test:ESUP1", "test:79", "test:20", "VDD", "VEE", 1),
                new Capacitor("test:CPSRR", "test:79", "test:80", 7.5e-12),
                new Resistor("test:RPSRR", "test:80", "test:20", 5e3),
                new VoltageControlledCurrentSource("test:GPSRR1", "test:20", "test:110", "test:80", "test:20",
                    0.05),
                new VoltageControlledCurrentSource("test:GPSRR2", "test:20", "test:111", "test:79", "test:20",
                    10.5e-6),
                new Resistor("test:RRDC", "test:111", "test:20", 1),
                new Resistor("test:RRR", "test:110", "test:20", 1),
                new VoltageControlledVoltageSource("test:ECM1", "test:81", "test:20", "test:12", "VEE", 1),
                new VoltageControlledCurrentSource("test:GCM2", "test:20", "test:111", "test:81", "test:20", 10e-6),
                new Diode("test:DIL", "test:12", "test:83", "DA"),
                new Resistor("test:RIL", "test:83", "test:84", 50),
                new VoltageSource("test:VIL", "test:85", "test:84", 0.9),
                new VoltageControlledVoltageSource("test:EIL", "test:85", "VEE", "VDD", "VEE", 1),
                new Diode("test:DVL3", "VEE", "test:12", "DA"),
                new Diode("test:DVL4", "test:12", "test:86", "DA"),
                new CurrentControlledCurrentSource("test:FVL", "test:86", "VEE", "test:VIS5", 1),
                new VoltageSource("test:VVL", "test:87", "VEE", 2.3),
                new VoltageSource("test:VIS5", "test:87", "test:88", 0),
                new Diode("test:DVL5", "test:88", "test:89", "DA"),
                new Resistor("test:RVL", "test:89", "test:90", 300),
                new VoltageControlledVoltageSource("test:EVL1", "test:90", "VEE", "VDD", "VEE", 1),
                new Resistor("RF", "OUT", "INN", 10),
                new VoltageSource("VSIG", "INP", "0", new Pulse(-1.5, 1.5, 1e-6, 10e-9, 10e-9, 5e-6, 10e-6)),
                new VoltageSource("VSUP", "VDD", "VEE", 5),
                new VoltageControlledVoltageSource("EG1", "0", "VEE", "VDD", "VEE", 0.2)
            );

            // Calculate the operating point
            var tran = new Transient("tran", 1e-9, 10e-6);
            // tran.BiasingParameters.Solver = new SparseRealSolver();
            // tran.BiasingParameters.Solver.SetParameter("pivrel", 0.999);
            foreach (int _ in tran.Run(ckt)) { }
        }

        [Test]
        public void When_ExportSwitch_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Sine(1, 1, 10)),
                new Resistor("R1", "in", "out", 1e3),
                new Resistor("R2", "out", "0", 1e3));

            var sim1 = new DC("dc", "V1", 0, 2, 0.2);
            var sim2 = new Transient("tran", 1e-3, 0.5);

            var vexport = new RealVoltageExport(sim1, "out");
            var iexport = new RealCurrentExport(sim1, "V1");
            var pexport = new RealPropertyExport(sim1, "R1", "p");
            foreach (int _ in sim1.Run(ckt, Transient.ExportTransient))
            {
                double input = sim1.GetVoltage("in");
                Assert.That(vexport.Value, Is.EqualTo(input * 0.5).Within(1e-9));
                Assert.That(iexport.Value, Is.EqualTo(-input / 2.0e3).Within(1e-9));
                Assert.That(pexport.Value, Is.EqualTo(input * input / 4.0 / 1.0e3).Within(1e-9));
            }

            // Switch exports
            vexport.Simulation = sim2;
            iexport.Simulation = sim2;
            pexport.Simulation = sim2;

            foreach (int _ in sim2.Run(ckt, Transient.ExportTransient))
            {
                double input = sim2.GetVoltage("in");
                Assert.That(input, Is.EqualTo(Math.Sin(2 * Math.PI * 10 * sim2.Time) + 1.0).Within(1e-9));
                Assert.That(vexport.Value, Is.EqualTo(input * 0.5).Within(1e-9));
                Assert.That(iexport.Value, Is.EqualTo(-input / 2.0e3).Within(1e-9));
                Assert.That(pexport.Value, Is.EqualTo(input * input / 4.0 / 1.0e3).Within(1e-9));
            }
        }

        [Test]
        public void When_MultipleTransient_Expect_Reference()
        {
            /*
             * We test if the simulation can run twice on different circuits with
             * different number of equations.
             */
            var cktA = new Circuit(
                new CurrentSource("I1", "0", "in", new Pulse(0, 1e-3, 0, 1e-15, 1, 10, 20)),
                new Resistor("R1", "in", "0", 1e9),
                new Capacitor("C1", "in", "0", 1e-6));
            var cktB = new Circuit(
                new CurrentSource("I1", "0", "in", new Pulse(0, 1e-3, 0, 1e-15, 1, 10, 20)),
                new VoltageSource("V1", "in", "out", 0.0),
                new Resistor("R1", "in", "0", 1e9),
                new Capacitor("C1", "out", "0", 1e-6),
                new Capacitor("C2", "out", "0", 1e-6));

            var tran = new Transient("tran", 1e-9, 1e-6);

            foreach (int _ in tran.Run(cktA, Transient.ExportTransient))
                Assert.That(tran.GetVoltage("in"), Is.EqualTo(tran.Time * 1e-3 / 1e-6).Within(1e-12));
            foreach (int _ in tran.Run(cktB, Transient.ExportTransient))
                Assert.That(tran.GetVoltage("in"), Is.EqualTo(tran.Time * 1e-3 / 2e-6).Within(1e-12));
        }

        [Test]
        public void When_TransientRerun_Expect_Same()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10),
                new Capacitor("C1", "out", "0", 20)
            );

            // Create the transient analysis
            var tran = new Transient("tran 1", 1.0, 10.0);
            tran.TimeParameters.InitialConditions["out"] = 0;
            var export = new RealVoltageExport(tran, "out");

            // Run the simulation a first time for building the reference values
            var r = new List<double>();
            foreach (int _ in tran.Run(ckt, Transient.ExportTransient))
                r.Add(export.Value);

            // Rerun the simulation for building the reference values
            int index = 0;
            foreach (int _ in tran.Rerun(Transient.ExportTransient))
                Assert.That(export.Value, Is.EqualTo(r[index++]).Within(1e-20));
        }

        [Test]
        public void When_TransientRun_Expect_YieldFlags()
        {
            // Create the circuit
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 10.0),
                new Resistor("R1", "in", "out", 10),
                new Capacitor("C1", "out", "0", 20)
            );

            // Create the transient analysis
            var tran = new Transient("tran 1", 1.0, 10.0);

            int flags = 0;
            foreach (int flag in tran.Run(ckt, mask: -1))
                flags |= flag;

            Assert.That(flags, Is.EqualTo(
                Simulation.BeforeSetup |
                Simulation.AfterSetup |
                Simulation.BeforeValidation |
                Simulation.AfterValidation |
                Simulation.BeforeExecute |
                Simulation.AfterExecute |
                Simulation.BeforeUnsetup |
                Simulation.AfterUnsetup |
                BiasingSimulation.BeforeTemperature |
                BiasingSimulation.AfterTemperature |
                Transient.ExportOperatingPoint |
                Transient.ExportTransient));
        }
    }
}
