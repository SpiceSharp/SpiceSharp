using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Parameters;

namespace Sandbox
{
    public partial class Main : Form
    {
        private static string nmos_model = @"D:\Visual Studio\Info\nmosmod.txt";
        private static string pmos_model = @"D:\Visual Studio\Info\pmosmod.txt";

        private static string nmos_reference_dc = @"D:\Visual Studio\Info\nmos_bsim3v24_dc.txt";
        private static string pmos_reference_dc = @"D:\Visual Studio\Info\pmos_bsim3v24_dc.txt";

        /// <summary>
        /// Test model NMOS
        /// </summary>
        private static BSIM3v24Model TestModelNMOS
        {
            get
            {
                BSIM3v24Model model = new BSIM3v24Model("TestModelNMOS");
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
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();

            Series[] series = new Series[12];
            Series[] refseries = new Series[series.Length];
            Series[] diffseries = new Series[series.Length];
            for (int i = 0; i < series.Length; i++)
            {
                series[i] = chMain.Series.Add("Ids (" + i + ")");
                series[i].ChartType = SeriesChartType.FastLine;
                refseries[i] = chMain.Series.Add("Reference (" + i + ")");
                refseries[i].ChartType = SeriesChartType.FastLine;
                refseries[i].BorderDashStyle = ChartDashStyle.Dash;
                diffseries[i] = chMain.Series.Add("Difference (" + i + ")");
                diffseries[i].ChartType = SeriesChartType.FastPoint;
                diffseries[i].YAxisType = AxisType.Secondary;
            }
            // chMain.ChartAreas[0].AxisX.IsLogarithmic = true;

            SpiceSharp.Diagnostics.CircuitWarning.WarningGenerated += CircuitWarning_WarningGenerated;

            // Generate the circuit
            Circuit ckt = new Circuit();

            BSIM3v24 nmos = new BSIM3v24("M1");
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
            DC dc = new DC("TestBSIM3v24_NMOS_DC");

            // Make the simulation slightly more accurate (error / 4)
            // Might want to check why some time though
            // dc.Config.RelTol = 1e-4;
            dc.Sweeps.Add(new DC.Sweep("V1", 0, 3.3, 0.3));
            dc.Sweeps.Add(new DC.Sweep("V2", 0, 3.3, 0.3));
            int index = 0;
            double[] reference = DCReferenceNMOS;
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double vds = dc.Sweeps.Last().CurrentValue;
                double actual = -data.Ask("V2", "i");

                // [note] I am using SmartSpice for verification here
                // SmartSpice adds an additional GMIN conductance between drain and source
                // for improving convergence. We don't do this, so we need to factor this in
                double expected = reference[index];
                double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * 1e-3 + 1e-12;

                int r = index / 12;
                series[r].Points.AddXY(vds, actual);
                refseries[r].Points.AddXY(vds, expected);
                diffseries[r].Points.AddXY(vds, actual - expected);

                index++;
            };
            ckt.Simulate(dc);
        }

        private void CircuitWarning_WarningGenerated(object sender, SpiceSharp.Diagnostics.WarningArgs e)
        {
            throw new Exception(e.Message);
        }
    }
}
