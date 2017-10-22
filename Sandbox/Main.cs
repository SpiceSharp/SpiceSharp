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
using SpiceSharp.Diagnostics;
using MathNet.Numerics.Interpolation;

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

            Series ns = chMain.Series.Add("Real part");
            ns.ChartType = SeriesChartType.FastLine;

            Series rs = chMain.Series.Add("Reference");
            rs.ChartType = SeriesChartType.FastPoint;

            chMain.ChartAreas[0].AxisX.IsLogarithmic = true;

            double[] refi = new double[]
            {
                -1.945791742986885e-12, -1.904705637099517e-08, -1.946103289747125e-12, -3.018754997881332e-08, -1.946885859826953e-12, -4.784404245850086e-08, -1.948851586992178e-12, -7.582769719229839e-08, -1.953789270386556e-12, -1.201788010800761e-07, -1.966192170307985e-12, -1.904705637099495e-07, -1.997346846331992e-12, -3.018754997881245e-07, -2.075603854314768e-12, -4.784404245849736e-07, -2.272176570837208e-12, -7.582769719228451e-07, -2.765944910274710e-12, -1.201788010800207e-06, -4.006234902415568e-12, -1.904705637097290e-06, -7.121702504803603e-12, -3.018754997872460e-06, -1.494740330300116e-11, -4.784404245814758e-06, -3.460467495474045e-11, -7.582769719089195e-06, -8.398150889530617e-11, -1.201788010744768e-05, -2.080105080892987e-10, -1.904705636876583e-05, -5.195572682013223e-10, -3.018754996993812e-05, -1.302127347221150e-09, -4.784404242316795e-05, -3.267854507347871e-09, -7.582769705163549e-05, -8.205537869558709e-09, -1.201788005200868e-04, -2.060843758802494e-08, -1.904705614805916e-04
            };

            NetlistReader nr = new NetlistReader();
            string netlist = string.Join(Environment.NewLine,
                ".model 1N914 D(Is= 2.52n Rs = .568 N= 1.752 Cjo= 4p M = .4 tt= 20n)",
                "V1 out 0 DC 1 AC 1 0",
                "D1 0 out 1N914",
                ".SAVE ir(V1) ii(V1)",
                ".AC dec 5 1k 10meg"
                );
            nr.Parse(new MemoryStream(Encoding.UTF8.GetBytes(netlist)));

            int index = 0;
            nr.Netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double n = 20.0 * Math.Log10(Math.Abs(nr.Netlist.Exports[0].Extract(data)));
                ns.Points.AddXY(data.GetFrequency(), n);

                n = 20.0 * Math.Log10(Math.Abs(refi[index]));
                rs.Points.AddXY(data.GetFrequency(), n);
                index += 2;
            };
            nr.Netlist.Simulate();
        }
    }
}
