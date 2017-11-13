using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpTest.Utils;
using SpiceSharpTest.Models;
using SpiceSharp.Circuits;

namespace SpiceSharpTest.Components.Semiconductors.BJT
{
    [TestClass]
    public class BJTTest : Framework
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
                    var emitterVoltage =
                        GetEmitterVoltageFromEmitterFollowerCircuit(
                            baseVoltage,
                            collectorVoltage,
                            resistance);

                    var voltage_base_emitter = baseVoltage - emitterVoltage;

                    Assert.That.AreEqualWithTol(0.7, voltage_base_emitter, 0, 0.5);
                }
            }
        }

        private double GetEmitterVoltageFromEmitterFollowerCircuit(
            double baseVoltage, 
            double collectorVoltage,
            int resistance)
        {
            var netlist = GetEmitterFollowerNetlist(baseVoltage, collectorVoltage, resistance);
            double emitterVoltage = 0;
            netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                emitterVoltage = netlist.Exports[0].Extract(data);
            };

            netlist.Simulate();
            return emitterVoltage;
        }

        private Netlist GetEmitterFollowerNetlist(double baseVoltage, double collectorVoltage, int resistance)
        {
            var netlist = Run(
                  ".MODEL mjd44h11 npn",
                  "+ IS = 1.45468e-14 BF = 135.617 NF = 0.85 VAF = 10",
                  "+ IKF = 5.15565 ISE = 2.02483e-13 NE = 3.99964 BR = 13.5617",
                  "+ NR = 0.847424 VAR = 100 IKR = 8.44427 ISC = 1.86663e-13",
                  "+ NC = 1.00046 RB = 1.35729 IRB = 0.1 RBM = 0.1",
                  "+ RE = 0.0001 RC = 0.037687 XTB = 0.90331 XTI = 1",
                  "+ EG = 1.20459 CJE = 3.02297e-09 VJE = 0.649408 MJE = 0.351062",
                  "+ TF = 2.93022e-09 XTF = 1.5 VTF = 1.00001 ITF = 0.999997",
                  "+ CJC = 3.0004e-10 VJC = 0.600008 MJC = 0.409966 XCJC = 0.8",
                  "+ FC = 0.533878 CJS = 0 VJS = 0.75 MJS = 0.5",
                  "+ TR = 2.73328e-08 PTF = 0 KF = 0 AF = 1",
                  $"V1 V_B gnd {baseVoltage}",
                  $"V2 V_C gnd {collectorVoltage}",
                  $"R1 V_E gnd {resistance}",
                  "Q1 V_C V_B V_E 0 mjd44h11",
                  ".SAVE V(V_E)",
                  ".OP"
                  );

            return netlist;

        }
    }
}
