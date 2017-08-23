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
        /// <summary>
        /// Get the test model
        /// </summary>
        private MOS1Model TestModel
        {
            get
            {
                /* Model part of the ntd20n06 (ONSemi)
                 * M1 9 7 8 8 MM L=100u W=100u
                 * .MODEL MM NMOS LEVEL=1 IS=1e-32
                 * +VTO=3.03646 LAMBDA=0 KP=5.28747
                 * +CGSO=6.5761e-06 CGDO=1e-11 */
                MOS1Model model = new MOS1Model("ntd20n06");
                model.Set("is", 1e-32);
                model.Set("vto", 3.03646);
                model.Set("lambda", 0);
                model.Set("kp", 5.28747);
                model.Set("cgso", 6.5761e-06);
                model.Set("cgdo", 1e-11);
                return model;
            }
        }

        // Reference
        double[] reference = new double[]
        {
            0.000000000000000e+000, 1.000000000000000e-012, 2.000000000000000e-012, 3.000000000000000e-012, 4.000000000000000e-012, 5.000000000000000e-012, 6.000000000000000e-012, 7.000000000000000e-012, 8.000000000000000e-012, 9.000000000000000e-012, 9.999999999999999e-012,
            0.000000000000000e+000, 1.000000000000000e-012, 2.000000000000000e-012, 3.000000000000000e-012, 4.000000000000000e-012, 5.000000000000000e-012, 6.000000000000000e-012, 7.000000000000000e-012, 8.000000000000000e-012, 9.000000000000000e-012, 9.999999999999999e-012,
            0.000000000000000e+000, 1.000000000000000e-012, 2.000000000000000e-012, 3.000000000000000e-012, 4.000000000000000e-012, 5.000000000000000e-012, 6.000000000000000e-012, 7.000000000000000e-012, 8.000000000000000e-012, 9.000000000000000e-012, 9.999999999999999e-012,
            0.000000000000000e+000, 1.000000000000000e-012, 2.000000000000000e-012, 3.000000000000000e-012, 4.000000000000000e-012, 5.000000000000000e-012, 6.000000000000000e-012, 7.000000000000000e-012, 8.000000000000000e-012, 9.000000000000000e-012, 9.999999999999999e-012,
            0.000000000000000e+000, 1.000000000000000e-012, 2.000000000000000e-012, 3.000000000000000e-012, 4.000000000000000e-012, 5.000000000000000e-012, 6.000000000000000e-012, 7.000000000000000e-012, 8.000000000000000e-012, 9.000000000000000e-012, 9.999999999999999e-012,
            0.000000000000000e+000, 1.000000000000000e-012, 2.000000000000000e-012, 3.000000000000000e-012, 4.000000000000000e-012, 5.000000000000000e-012, 6.000000000000000e-012, 7.000000000000000e-012, 8.000000000000000e-012, 9.000000000000000e-012, 9.999999999999999e-012,
            0.000000000000000e+000, 1.000000000000000e-012, 2.000000000000000e-012, 3.000000000000000e-012, 4.000000000000000e-012, 5.000000000000000e-012, 6.000000000000000e-012, 7.000000000000000e-012, 8.000000000000000e-012, 9.000000000000000e-012, 9.999999999999999e-012,
            0.000000000000000e+000, 5.680575723785252e-001, 5.680575723795250e-001, 5.680575723805251e-001, 5.680575723815250e-001, 5.680575723825251e-001, 5.680575723835252e-001, 5.680575723845250e-001, 5.680575723855251e-001, 5.680575723865250e-001, 5.680575723875251e-001,
            0.000000000000000e+000, 1.886410671900999e+000, 2.454468244279524e+000, 2.454468244280524e+000, 2.454468244281524e+000, 2.454468244282523e+000, 2.454468244283524e+000, 2.454468244284524e+000, 2.454468244285524e+000, 2.454468244286524e+000, 2.454468244287524e+000,
            0.000000000000000e+000, 3.208278171900999e+000, 5.094688843801998e+000, 5.662746416180523e+000, 5.662746416181523e+000, 5.662746416182523e+000, 5.662746416183524e+000, 5.662746416184524e+000, 5.662746416185524e+000, 5.662746416186524e+000, 5.662746416187522e+000,
            0.000000000000000e+000, 4.530145671900999e+000, 7.738423843801998e+000, 9.624834515702997e+000, 1.019289208808152e+001, 1.019289208808252e+001, 1.019289208808352e+001, 1.019289208808452e+001, 1.019289208808552e+001, 1.019289208808652e+001, 1.019289208808752e+001
        };

        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();

            int n = 11;
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

            SpiceSharp.Diagnostics.CircuitWarning.WarningGenerated += CircuitWarning_WarningGenerated;

            // Build the circuit
            Circuit ckt = new Circuit();

            MOS1 m = new MOS1("M1");
            m.SetModel(TestModel);
            m.Set("w", 100e-6);
            m.Set("l", 100e-6);
            m.Connect("D", "G", "GND", "GND");
            ckt.Objects.Add(
                new Voltagesource("V1", "G", "GND", 0.0),
                new Voltagesource("V2", "D", "GND", 0.0),
                m);

            // Simulate the circuit
            DC dc = new DC("TestMOS1_DC");
            dc.Sweeps.Add(new DC.Sweep("V1", 0.0, 5.0, 0.5));
            dc.Sweeps.Add(new DC.Sweep("V2", 0.0, 5.0, 0.5));
            int index = 0;
            dc.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double vgs = dc.Sweeps[0].CurrentValue;
                double vds = dc.Sweeps[1].CurrentValue;

                double expected = reference[index] - vds * ckt.State.Gmin;
                double actual = -data.Ask("V2", "i");

                double tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * 1e-6 + 1e-12;

                int r = index / n;
                series[r].Points.AddXY(vds, actual);
                refseries[r].Points.AddXY(vds, expected);
                diffseries[r].Points.AddXY(vds, Math.Abs(actual - expected));

                index = index + 1;
            };
            ckt.Simulate(dc);
        }

        private void CircuitWarning_WarningGenerated(object sender, SpiceSharp.Diagnostics.WarningArgs e)
        {
            throw new Exception(e.Message);
        }
    }
}
