using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;
using SpiceSharpTest.Utils;

namespace SpiceSharpTest.Components.Semiconductors.BJT
{
    [TestClass]
    public class BJTTest
    {
        [TestMethod]
        public void Emitter_Follower_DC()
        {
            // A test for BJT transistor (model mjd44h11) acting in an emitter-follower transistor circuit.
            // A circuit consists of a BJT transistor and resistor connected to the transistor emitter.
            // The voltage betwen base and emitter is around 0.7V when a transistor is in active mode.

            var collectorVoltage = 100.0;
            var baseVoltages = new double[] { 1.0, 2.0, 10.0, 20.00, 30.0, 60.0, 90.0, 98.0 };
            var resistancesOfResistorConnectedToEmitter = new int[] { 5, 10, 30, 40, 50, 60, 70, 1000 };

            foreach (var baseVoltage in baseVoltages)
            {
                foreach (var resistance in resistancesOfResistorConnectedToEmitter)
                {
                    double resistorCurrent = 0;

                    var emitterVoltage =
                        GetEmitterVoltageFromEmitterFollowerCircuit(
                            baseVoltage,
                            collectorVoltage,
                            resistance,
                            out resistorCurrent);

                    var V_BE = baseVoltage - emitterVoltage;
                    Assert.That.AreEqualWithTol(0.7, V_BE, 0, 0.5);
                }
            }
        }

        private static double GetEmitterVoltageFromEmitterFollowerCircuit(double baseVoltage, double collectorVoltage,
            int resistance, out double resistorCurrent)
        {
            var baseVoltageSource = new Voltagesource(
                new CircuitIdentifier("V1"),
                new CircuitIdentifier("V_B"),
                new CircuitIdentifier("gnd"),
                baseVoltage);

            var collectorVoltageSource = new Voltagesource(
                new CircuitIdentifier("V2"),
                new CircuitIdentifier("V_C"),
                new CircuitIdentifier("gnd"),
                collectorVoltage);

            var resistor = new Resistor(new CircuitIdentifier("R"),
                new CircuitIdentifier("V_E"),
                new CircuitIdentifier("gnd"),
                resistance
            );

            var bjt = new SpiceSharp.Components.BJT(new CircuitIdentifier("Q1"));
            var model = GetBjtModel();
            bjt.SetModel(model);

            bjt.Connect(
                new CircuitIdentifier("V_C"),
                new CircuitIdentifier("V_B"),
                new CircuitIdentifier("V_E"),
                new CircuitIdentifier("gnd"));

            Circuit ckt = new Circuit();
            ckt.Objects.Add(baseVoltageSource, collectorVoltageSource, resistor, bjt);
            ckt.Method = new Trapezoidal();
            ckt.Method.Initialize();

            OP simulation = new OP("Simulation");
            
            double voltage = 0;
            double currrent = 0;
            simulation.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                var emitterVoltage = data.GetVoltage(
                    new CircuitIdentifier("V_E"), 
                    new CircuitIdentifier("gnd"));

                currrent = data.Ask(new CircuitIdentifier("R"), "i");

                voltage = emitterVoltage;
            };

            simulation.Circuit = ckt;
            simulation.SetupAndExecute();
            resistorCurrent = currrent;
            return voltage;
        }

        private static BJTModel GetBjtModel()
        {
            var bjtModelSpecification = ".MODEL mjd44h11 npn" +
                                        " IS = 1.45468e-14 BF = 135.617 NF = 0.85 VAF = 10" +
                                        " IKF = 5.15565 ISE = 2.02483e-13 NE = 3.99964 BR = 13.5617" +
                                        " NR = 0.847424 VAR = 100 IKR = 8.44427 ISC = 1.86663e-13" +
                                        " NC = 1.00046 RB = 1.35729 IRB = 0.1 RBM = 0.1" +
                                        " RE = 0.0001 RC = 0.037687 XTB = 0.90331 XTI = 1" +
                                        " EG = 1.20459 CJE = 3.02297e-09 VJE = 0.649408 MJE = 0.351062" +
                                        " TF = 2.93022e-09 XTF = 1.5 VTF = 1.00001 ITF = 0.999997" +
                                        " CJC = 3.0004e-10 VJC = 0.600008 MJC = 0.409966 XCJC = 0.8" +
                                        " FC = 0.533878 CJS = 0 VJS = 0.75 MJS = 0.5" +
                                        " TR = 2.73328e-08 PTF = 0 KF = 0 AF = 1";

            NetlistReader r = new NetlistReader();
            r.Parse(new MemoryStream(Encoding.UTF8.GetBytes(bjtModelSpecification)));
            var model = (BJTModel)r.Netlist.Circuit.Objects[new CircuitIdentifier("mjd44h11")];
            return model;
        }
    }
}
