using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

namespace Sandbox
{
    public partial class Main : Form
    {
        /// <summary>
        /// Get our test model
        /// </summary>
        private static DiodeModel TestModel
        {
            get
            {
                // LTSpice model
                // .model 1N914 D(Is= 2.52n Rs = .568 N= 1.752 Cjo= 4p M = .4 tt= 20n Iave = 200m Vpk= 75 mfg= OnSemi type= silicon)
                DiodeModel model = new DiodeModel("1N914");
                model.Set("is", 2.52e-9); model.Set("rs", 0.568);
                model.Set("n", 1.752); model.Set("cjo", 4e-12);
                model.Set("m", 0.4); model.Set("tt", 20e-9);
                return model;
            }
        }

        // Expected values:
        // As simulated by ngSpice via PartSim
        double[] reference = new double[]
        {
            -154.403442611, -150.403442674, -146.403442698, -142.403442708, -138.403442712, -134.403442714, -130.403442715, -126.403442715, -122.403442715, -118.403442715,
            -114.403442715, -110.403442715, -106.403442715, -102.403442715, -98.4034427151, -94.4034427154, -90.4034427162, -86.4034427181, -82.403442723, -78.4034427352,
            -74.4034427658
        };

        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();

            var series = chMain.Series.Add("AC magnitude");
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            var refseries = chMain.Series.Add("Reference ngSpice");
            refseries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;

            var diffseries = chMain.Series.Add("Difference ngSpice");
            diffseries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
            diffseries.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;

            chMain.ChartAreas[0].AxisX.IsLogarithmic = true;

            SpiceSharp.Diagnostics.CircuitWarning.WarningGenerated += CircuitWarning_WarningGenerated;

            // Generate the circuit
            Circuit ckt = new Circuit();
            Diode d = new Diode("D1");
            d.Connect("GND", "OUT");
            d.SetModel(TestModel);
            Voltagesource vsrc = new Voltagesource("V1");
            vsrc.VSRCacMag.Set(1.0);
            vsrc.VSRCdcValue.Set(1.0);
            vsrc.Connect("OUT", "GND");
            ckt.Objects.Add(vsrc, d);

            // Generate the simulation
            AC ac = new AC("TestDiodeAC");

            // Make the simulation slightly more accurate (ref. DC)
            ac.Config.RelTol = 0.25e-3;
            ac.StartFreq = 1e3; // 1kHz
            ac.StopFreq = 10e6; // 10megHz
            ac.NumberSteps = 5;
            ac.StepType = AC.StepTypes.Decade;
            int index = 0;
            ac.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double frequency = data.GetFrequency();
                var c = -vsrc.GetComplexCurrent(data.Circuit);

                // Check the amplitude
                double expected = reference[index];
                double actual = 20.0 * Math.Log10(c.Magnitude);
                double tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * 1e-3 + 1e-6;

                series.Points.AddXY(frequency, actual);
                refseries.Points.AddXY(frequency, expected);
                diffseries.Points.AddXY(frequency, Math.Abs(actual - expected));

                index++;
            };
            ckt.Simulate(ac);
        }

        private void CircuitWarning_WarningGenerated(object sender, SpiceSharp.Diagnostics.WarningArgs e)
        {
            throw new Exception(e.Message);
        }
    }
}
