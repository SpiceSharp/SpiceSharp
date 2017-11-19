using System;
using System.Numerics;
using System.Collections.Generic;
using System.Windows.Forms;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Parameters;
using SpiceSharp.Parser.Readers;
using SpiceSharp.Diagnostics;
using MathNet.Numerics.Interpolation;
using SpiceSharp.Circuits;

namespace Sandbox
{
    public partial class Main : Form
    {
        public List<double> input = new List<double>();
        public List<double> output = new List<double>();

        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();

            double reference_ve = 9.452632245345402e+00;
            double abstol = 1e-16;
            double reltol = 1e-20;

            // Create the netlist
            string strNetlist = string.Join(Environment.NewLine,
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
                "V1 V_B 0 10",
                "V2 V_C 0 100",
                "R1 V_E 0 1000",
                "Q1 V_C V_B V_E 0 mjd44h11");
            var nr = new NetlistReader();
            nr.Parse(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(strNetlist)));

            // Create the OP simulation
            OP op = new OP("OP 1");
            op.CurrentConfig.AbsTol = abstol;
            op.CurrentConfig.RelTol = reltol;

            // Run the simulation and store the result
            var netlist = nr.Netlist;
            op.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double actual_ve = data.GetVoltage("V_E");

                // Calculate the allowed tolernace as specified in the configuration
                double tol = reltol * Math.Abs(actual_ve) + abstol;
                double difference = Math.Abs(actual_ve - reference_ve);
                tol += 1e-16 * Math.Abs(reference_ve) + 1e-20; // Contribution from tolerance of Spice 3f5

                // Display this to the user
                string msg = string.Join(Environment.NewLine,
                    $"Actual value: {actual_ve}",
                    $"Expected value: {reference_ve}",
                    $"Difference: {difference}",
                    $"Allowed tolerance: {tol}",
                    difference < tol ? "Within tolerance levels" : "NOT within tolerance levels");
                MessageBox.Show(msg);
            };
            netlist.Circuit.Simulation = op;
            op.Circuit = netlist.Circuit;
            op.SetupAndExecute();
        }
    }
}