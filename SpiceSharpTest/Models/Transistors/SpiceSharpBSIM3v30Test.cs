using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace SpiceSharpTest.Models.Transistors
{
    [TestClass]
    public class SpiceSharpBSIM3v30Test
    {
        /// <summary>
        /// Unfortunately it is very hard to find BSIM3v3 models (I only found BSIM3v1) that are public.
        /// I only have a model by a FAB that we use, but I cannot disclose the model parameters and simulation results.
        /// 
        /// If you wish to run this test yourself, create a model file where each line contains
        /// [par1]=[val1]
        /// [par2]=[val2]
        /// ...
        /// 
        /// The test results are also stored in a text file where VGS and VDS are swept from 0V to 1.8V in
        /// steps of 0.3V (VDS is the main sweep). The result is stored in a text file where each line is
        /// [ids_VDS=0&VGS=0]
        /// [ids_VDS=0.3&VGS=0]
        /// ...
        /// [ids_VDS=1.8&VGS=0]
        /// [ids_VDS=0&VGS=0.3]
        /// [ids_VDS=0.3&VGS=0.3]
        /// ...
        /// [ids_VDS=1.8&VGS=1.8]
        /// 
        /// So there should be 7*7 = 49 lines in the file
        /// 
        /// Important note: My simulation data is exported from SmartSpice (Silvaco). There are some small
        /// differences, so read the [note]s in this document if you want to run this test.
        /// </summary>
        private static string nmos_model = @"D:\Visual Studio\Info\nmosmod.txt";
        private static string pmos_model = @"D:\Visual Studio\Info\pmosmod.txt";

        private static string nmos_reference_dc = @"D:\Visual Studio\Info\nmos_bsim3v3_dc.txt";
        private static string pmos_reference_dc = @"D:\Visual Studio\Info\pmos_bsim3v3_dc.txt";

        /// <summary>
        /// Test model NMOS
        /// </summary>
        private static BSIM3v30Model TestModelNMOS
        {
            get
            {
                BSIM3v30Model model = new BSIM3v30Model("TestModelNMOS");
                model.SetNMOS(true);
                using (StreamReader sr = new StreamReader(nmos_model))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        string[] assignment = line.Split('=');
                        double value = double.Parse(assignment[1], System.Globalization.CultureInfo.InvariantCulture);
                        model.Set(assignment[0], value);
                    }
                }
                return model;
            }
        }

        /// <summary>
        /// Test model NMOS
        /// </summary>
        private static BSIM3v30Model TestModelPMOS
        {
            get
            {
                BSIM3v30Model model = new BSIM3v30Model("TestModelPMOS");
                model.SetPMOS(true);
                using (StreamReader sr = new StreamReader(pmos_model))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        string[] assignment = line.Split('=');
                        double value = double.Parse(assignment[1], System.Globalization.CultureInfo.InvariantCulture);
                        model.Set(assignment[0], value);
                    }
                }
                return model;
            }
        }

        /// <summary>
        /// Get the test data
        /// </summary>
        private static double[] DCReferenceNMOS
        {
            get
            {
                List<double> r = new List<double>();
                using (StreamReader sr = new StreamReader(nmos_reference_dc))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        r.Add(double.Parse(line, System.Globalization.CultureInfo.InvariantCulture));
                    }
                }
                return r.ToArray();
            }
        }

        /// <summary>
        /// Get the test data
        /// </summary>
        private static double[] DCReferencePMOS
        {
            get
            {
                List<double> r = new List<double>();
                using (StreamReader sr = new StreamReader(pmos_reference_dc))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        r.Add(double.Parse(line, System.Globalization.CultureInfo.InvariantCulture));
                    }
                }
                return r.ToArray();
            }
        }

        [TestMethod]
        public void TestBSIM3v30_NMOS_DC()
        {
            // Simulated by SmartSpice (Silvaco)
            double[] reference = DCReferenceNMOS;

            // Generate the circuit
            Circuit ckt = new Circuit();

            BSIM3v30 nmos = new BSIM3v30("M1");
            nmos.SetModel(TestModelNMOS);
            nmos.Connect("2", "1", "0", "0");
            nmos.Set("w", 1e-6); nmos.Set("l", 1e-6);
            nmos.Set("ad", 0.85e-12); nmos.Set("as", 0.85e-12);
            nmos.Set("pd", 2.7e-6); nmos.Set("ps", 2.7e-6);
            nmos.Set("nrd", 0.3); nmos.Set("nrs", 0.3);
            ckt.Objects.Add(
                new Voltagesource("V2", "2", "0", 0.0),
                new Voltagesource("V1", "1", "0", 0.0),
                new Resistor("Rl", "2", "0", 1.0 / ckt.State.Gmin),
                nmos);

            // Generate the simulation
            DC dc = new DC("TestBSIM3_NMOS_DC");
            dc.Sweeps.Add(new DC.Sweep("V1", 0, 1.8, 0.3));
            dc.Sweeps.Add(new DC.Sweep("V2", 0, 1.8, 0.3));
            int index = 0;
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double vds = dc.Sweeps.Last().CurrentValue;
                double actual = -data.Ask("V2", "i");
                double expected = reference[index];
                double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * 1e-3 + 1e-12;
                Assert.AreEqual(expected, actual, tol);
                index++;
            };
            ckt.Simulate(dc);
        }

        [TestMethod]
        public void TestBSIM3v30_PMOS_DC()
        {
            // Simulated by SmartSpice (Silvaco)
            double[] reference = DCReferencePMOS;

            // Generate the circuit
            Circuit ckt = new Circuit();

            BSIM3v30 nmos = new BSIM3v30("M1");
            nmos.SetModel(TestModelPMOS);
            nmos.Connect("2", "1", "0", "0");
            nmos.Set("w", 1e-6); nmos.Set("l", 1e-6);
            nmos.Set("ad", 0.85e-12); nmos.Set("as", 0.85e-12);
            nmos.Set("pd", 2.7e-6); nmos.Set("ps", 2.7e-6);
            nmos.Set("nrd", 0.3); nmos.Set("nrs", 0.3);
            ckt.Objects.Add(
                new Voltagesource("V2", "0", "2", 0.0),
                new Voltagesource("V1", "0", "1", 0.0),
                new Resistor("Rl", "2", "0", 1.0 / ckt.State.Gmin),
                nmos);

            // Generate the simulation
            DC dc = new DC("TestBSIM3_PMOS_DC");
            dc.Sweeps.Add(new DC.Sweep("V1", 0, 1.8, 0.3));
            dc.Sweeps.Add(new DC.Sweep("V2", 0, 1.8, 0.3));
            int index = 0;
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double vds = dc.Sweeps.Last().CurrentValue;
                double actual = data.Ask("V2", "i");
                double expected = reference[index];
                double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * 1e-3 + 1e-12;
                Assert.AreEqual(expected, actual, tol);

                index++;
            };
            ckt.Simulate(dc);
        }
    }
}
