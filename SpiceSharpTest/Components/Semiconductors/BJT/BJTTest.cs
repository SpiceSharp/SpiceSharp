using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpTest.Utils;
using SpiceSharpTest.Models;
using SpiceSharp.Circuits;
using System;

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

            var collectorVoltage = 100.0;
            var baseVoltages = new double[] { 10.0 };
            var resistancesOfResistorConnectedToEmitter = new int[] { 900, 1000, 1200 };
            var references_ve = new double[] {
                9.450319661131841e+00, // V_C = 100, V_B = 10, R = 900 (from improved spice3f5 RELTOL=1e-16 ABSTOL=1e-20)
                9.452632245347569e+00, // V_C = 100, V_B = 10, R = 1000 (from improved spice3f5 RELTOL=1e-16 ABSTOL=1e-20)
                9.456633700898708e+00, // V_C = 100, V_B = 10, R = 1200 (from improved spice3f5 RELTOL=1e-16 ABSTOL=1e-20)
            };
            double abstol = 1e-12;
            double reltol = 1e-9;

            double spice3f5AbsTol = 1e-20;
            double spice3f5RelTol = 1e-16;

            int reference_ve_index = 0;
            foreach (var baseVoltage in baseVoltages)
            {
                foreach (var resistance in resistancesOfResistorConnectedToEmitter)
                {
                    double reference_ve = references_ve[reference_ve_index];

                    var actual_ve =
                        GetEmitterVoltageFromEmitterFollowerCircuit(
                            baseVoltage,
                            collectorVoltage,
                            resistance,
                            reltol,
                            abstol);

                    double tol = reltol * Math.Abs(actual_ve) + abstol;
                    double difference = Math.Abs(actual_ve - reference_ve);
                    tol += spice3f5RelTol * Math.Abs(reference_ve) + spice3f5AbsTol; // Contribution from tolerance of Spice 3f5

                    double allowed_difference = tol;

                    Assert.IsTrue(allowed_difference > difference);

                    reference_ve_index++;
                }
            }
        }

        private double GetEmitterVoltageFromEmitterFollowerCircuit(
            double baseVoltage, 
            double collectorVoltage,
            int resistance,
            double reltol,
            double abstol)
        {
            var netlist = GetEmitterFollowerNetlist(baseVoltage, collectorVoltage, resistance, reltol, abstol);
            double emitterVoltage = 0;
            netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                emitterVoltage = netlist.Exports[0].Extract(data);
            };

            netlist.Simulate();
            return emitterVoltage;
        }

        private Netlist GetEmitterFollowerNetlist(double baseVoltage, double collectorVoltage, int resistance, double reltol, double abstol)
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
                  ".OP",
                  ".SAVE V(V_E)",
                  $".OPTIONS RELTOL={reltol} ABSTOL={abstol}"
                  );

            return netlist;

        }
    }
}
