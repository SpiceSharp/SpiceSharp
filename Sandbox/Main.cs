using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
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
using SpiceSharp.Parser.Readers;

namespace Sandbox
{
    public partial class Main : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Main()
        {
            InitializeComponent();

            // Set up the chart
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

            NetlistReader nr = new NetlistReader();
            var mr = nr.Netlist.Readers[StatementType.Component].Find<MosfetReader>();
            BSIMParser.AddMosfetGenerators(mr.Mosfets);
            var modr = nr.Netlist.Readers[StatementType.Model].Find<MosfetModelReader>();
            BSIMParser.AddMosfetModelGenerators(modr.Levels);
        }

        private void CircuitWarning_WarningGenerated(object sender, SpiceSharp.Diagnostics.WarningArgs e)
        {
            throw new Exception(e.Message);
        }
    }
}
