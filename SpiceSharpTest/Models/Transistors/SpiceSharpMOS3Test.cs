using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models.Transistors
{
    [TestClass]
    public class SpiceSharpMOS3Test
    {
        /**
         * Note to self:
         * SmartSpice uses extended models, or propriety models for mosfet parasitic capacitances for LEVEL=1,2,3. If they are not specified by the model,
         * use CAPMOD=1 to use the legacy parasitic capacitance calculations!
         **/

        /// <summary>
        /// The testing model
        /// </summary>
        public MOS3Model TestModel
        {
            get
            {
                // Model part of the FDC604P (ONSemi)
                // M1 2 1 4x 4x DMOS L = 1u W = 1u
                // .MODEL DMOS PMOS(VTO= -0.7 KP= 3.8E+1 THETA = .25 VMAX= 3.5E5 LEVEL= 3)
                MOS3Model model = new MOS3Model("DMOS");
                model.SetPMOS(true);
                model.Set("vto", -0.7);
                model.Set("kp", 3.8e1);
                model.Set("theta", 0.25);
                model.Set("vmax", 3.5e5);
                return model;
            }
        }

        [TestMethod]
        public void TestMOS3_DC()
        {
            // Simulated by LTSpiceXVII
            // GMIN = 0
            // vds,vgs: 0->5V in 0.5V steps
            double[] reference = new double[]
            {
                0.000000e+000, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, 
                0.000000e+000, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, -1.000000e-014, 
                0.000000e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, -1.518862e+000, 
                0.000000e+000, -8.127778e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, -9.117779e+000, 
                0.000000e+000, -1.414177e+001, -2.031503e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, -2.085084e+001, 
                0.000000e+000, -1.917674e+001, -3.046696e+001, -3.505858e+001, -3.526929e+001, -3.526929e+001, -3.526929e+001, -3.526929e+001, -3.526929e+001, -3.526929e+001, -3.526929e+001, 
                0.000000e+000, -2.345376e+001, -3.916565e+001, -4.822222e+001, -5.151583e+001, -5.153519e+001, -5.153519e+001, -5.153519e+001, -5.153519e+001, -5.153519e+001, -5.153519e+001, 
                0.000000e+000, -2.713200e+001, -4.670229e+001, -5.970438e+001, -6.696503e+001, -6.912774e+001, -6.912774e+001, -6.912774e+001, -6.912774e+001, -6.912774e+001, -6.912774e+001, 
                0.000000e+000, -3.032897e+001, -5.329517e+001, -6.980789e+001, -8.063262e+001, -8.641838e+001, -8.770544e+001, -8.770544e+001, -8.770544e+001, -8.770544e+001, -8.770544e+001, 
                0.000000e+000, -3.313334e+001, -5.911111e+001, -7.876699e+001, -9.280997e+001, -1.018468e+002, -1.064000e+002, -1.070348e+002, -1.070348e+002, -1.070348e+002, -1.070348e+002, 
                0.000000e+000, -3.561322e+001, -6.427981e+001, -8.676569e+001, -1.037282e+002, -1.157347e+002, -1.232772e+002, -1.267850e+002, -1.269506e+002, -1.269506e+002, -1.269506e+002
            };

            // Create the circuit
            Circuit ckt = new Circuit();
            MOS3 m = new MOS3("M1");
            m.Set("w", 1e-6); m.Set("l", 1e-6);
            m.SetModel(TestModel);
            m.Connect("D", "G", "0", "0");
            ckt.Objects.Add(
                new Voltagesource("V1", "0", "G", 0.0),
                new Voltagesource("V2", "0", "D", 0.0),
                m);

            // Create the simulation
            DC dc = new DC("TestMOS3_DC");
            dc.Config.Gmin = 0;
            dc.Sweeps.Add(new DC.Sweep("V1", 0.0, 5.0, 0.5));
            dc.Sweeps.Add(new DC.Sweep("V2", 0.0, 5.0, 0.5));
            int index = 0;
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double vds = dc.Sweeps[1].CurrentValue;
                double expected = reference[index];
                double actual = data.Ask("V2", "i");
                double tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * 1e-6 + 1e-10; // Absolute tolerance weaker because of different BS/BD diode model
                Assert.AreEqual(expected, actual, tol);

                index++;
            };
            ckt.Simulate(dc);
        }
    }
}
