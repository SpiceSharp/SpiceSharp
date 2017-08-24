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
        // Simulation data by LTSpiceXVII
        double[] reference = new double[]
        {
            0.000000e+000, -1.010000e-012, -2.010000e-012, -3.010000e-012, -4.010000e-012, -5.010000e-012, -6.010000e-012, -7.010000e-012, -8.010000e-012, -9.010000e-012, -1.001000e-011, 0.000000e+000, -2.500001e-006, -2.500002e-006, -2.500003e-006, -2.500004e-006, -2.500005e-006, -2.500006e-006, -2.500007e-006, -2.500008e-006, -2.500009e-006, -2.500010e-006, 0.000000e+000, -7.500001e-006, -1.000000e-005, -1.000000e-005, -1.000000e-005, -1.000001e-005, -1.000001e-005, -1.000001e-005, -1.000001e-005, -1.000001e-005, -1.000001e-005, 0.000000e+000, -1.250000e-005, -2.000000e-005, -2.250000e-005, -2.250000e-005, -2.250001e-005, -2.250001e-005, -2.250001e-005, -2.250001e-005, -2.250001e-005, -2.250001e-005, 0.000000e+000, -1.750000e-005, -3.000000e-005, -3.750000e-005, -4.000000e-005, -4.000001e-005, -4.000001e-005, -4.000001e-005, -4.000001e-005, -4.000001e-005, -4.000001e-005, 0.000000e+000, -2.250000e-005, -4.000000e-005, -5.250000e-005, -6.000001e-005, -6.250000e-005, -6.250000e-005, -6.250001e-005, -6.250001e-005, -6.250001e-005, -6.250001e-005, 0.000000e+000, -2.750000e-005, -5.000000e-005, -6.750000e-005, -8.000001e-005, -8.750000e-005, -9.000001e-005, -9.000001e-005, -9.000001e-005, -9.000001e-005, -9.000001e-005, 0.000000e+000, -3.250000e-005, -6.000000e-005, -8.250000e-005, -1.000000e-004, -1.125000e-004, -1.200000e-004, -1.225000e-004, -1.225000e-004, -1.225000e-004, -1.225000e-004, 0.000000e+000, -3.750000e-005, -7.000000e-005, -9.750000e-005, -1.200000e-004, -1.375000e-004, -1.500000e-004, -1.575000e-004, -1.600000e-004, -1.600000e-004, -1.600000e-004, 0.000000e+000, -4.250000e-005, -8.000001e-005, -1.125000e-004, -1.400000e-004, -1.625000e-004, -1.800000e-004, -1.925000e-004, -2.000000e-004, -2.025000e-004, -2.025000e-004, 0.000000e+000, -4.750000e-005, -9.000000e-005, -1.275000e-004, -1.600000e-004, -1.875000e-004, -2.100000e-004, -2.275000e-004, -2.400000e-004, -2.475000e-004, -2.500000e-004
        };

        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();

            Series[] series = new Series[11];
            Series[] refseries = new Series[series.Length];
            Series[] diffseries = new Series[series.Length];
            for (int i = 0; i < series.Length; i++)
            {
                series[i] = chMain.Series.Add("Ids (" + i + ")");
                series[i].ChartType = SeriesChartType.FastLine;
                refseries[i] = chMain.Series.Add("Reference (" + i + ")");
                refseries[i].ChartType = SeriesChartType.FastLine;
                diffseries[i] = chMain.Series.Add("Difference (" + i + ")");
                diffseries[i].ChartType = SeriesChartType.FastPoint;
                diffseries[i].YAxisType = AxisType.Secondary;
            }
            // chMain.ChartAreas[0].AxisX.IsLogarithmic = true;

            SpiceSharp.Diagnostics.CircuitWarning.WarningGenerated += CircuitWarning_WarningGenerated;

            // Build the circuit
            Circuit ckt = new Circuit();
            MOS1 m = new MOS1("M1");
            m.SetModel(new MOS1Model("DefaultModel"));
            m.Connect("D", "G", "0", "0");
            ckt.Objects.Add(
                new Voltagesource("V1", "G", "0", 0.0),
                new Voltagesource("V2", "D", "0", 0.0),
                new Resistor("Rgmin", "D", "0", 1e12),
                m);

            // Build the simulation
            DC dc = new DC("TestMOS1_DC_Default");
            dc.Sweeps.Add(new DC.Sweep("V1", 0.0, 5.0, 0.5));
            dc.Sweeps.Add(new DC.Sweep("V2", 0.0, 5.0, 0.5));
            int index = 0;
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                // double vgs = dc.Sweeps[0].CurrentValue;
                double vds = dc.Sweeps[1].CurrentValue;
                double expected = reference[index];
                double actual = data.Ask("V2", "i");

                int r = index / 11;
                series[r].Points.AddXY(vds, actual);
                refseries[r].Points.AddXY(vds, expected);
                diffseries[r].Points.AddXY(vds, actual - expected);
                index++;
            };
            ckt.Simulate(dc);

            string w = "";
            foreach (var msg in SpiceSharp.Diagnostics.CircuitWarning.Warnings)
                w += msg + Environment.NewLine;
            if (w.Length > 0)
                MessageBox.Show(w);
        }

        private void CircuitWarning_WarningGenerated(object sender, SpiceSharp.Diagnostics.WarningArgs e)
        {
            throw new Exception(e.Message);
        }
    }
}
