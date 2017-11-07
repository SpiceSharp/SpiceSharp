using System;
using System.Numerics;
using System.Collections.Generic;
using System.Windows.Forms;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;

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

            var plotOutput = chMain.Series.Add("Output");
            plotOutput.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
            var plotReference = chMain.Series.Add("Reference");
            plotReference.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastPoint;
            plotReference.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            chMain.ChartAreas[0].AxisX.IsLogarithmic = true;

            // Simulation by Spice 3f5
            double[] reference = new double[]
            {
                -1.688264281083676e-03, -6.256171908921069e-01, -4.240681451402584e-03, -9.915255104329067e-01, -1.065181562150848e-02, -1.571418573953089e+00, -2.675429265826203e-02, -2.490357609011980e+00, -6.719202140507280e-02, -3.946262298301279e+00, -1.687048030969052e-01, -6.251664871263374e+00, -4.233015949949251e-01, -9.897332180343838e+00, -1.060358405820734e+00, -1.564303168844958e+01, -2.645208298269774e+00, -2.462227160463906e+01, -6.531786973747334e+00, -3.836191269368651e+01, -1.573677982895236e+01, -5.831551384917359e+01, -3.584989404952447e+01, -8.382163136661943e+01, -7.298715801766126e+01, -1.076749482750201e+02, -1.242129947055924e+02, -1.156205991749138e+02, -1.723768968647264e+02, -1.012388382455914e+02, -2.038436403302176e+02, -7.553792287868039e+01, -2.198184972018414e+02, -5.139626354807145e+01, -2.268974568604466e+02, -3.347306931637866e+01, -2.298441757431590e+02, -2.139419177413151e+01, -2.310386967836807e+02, -1.356870019889037e+01, -2.315177067081831e+02, -8.578581367969413e+00, -2.317089572179777e+02, -5.416492823357626e+00, -2.317851833738230e+02, -3.417594558206259e+00, -2.318155434938701e+02, -2.154886248110806e+00, -2.318276322425509e+02, -1.356934399871164e+00, -2.318324450945829e+02, -8.517828801859809e-01, -2.318343608887390e+02, -5.304653655133196e-01, -2.318351228545550e+02, -3.236431096441236e-01, -2.318354243553034e+02, -1.866778920706296e-01, -2.318355397514484e+02, -9.000682163588355e-02, -2.318355740522236e+02, -1.276368093331558e-02, -2.318355584710393e+02, 6.172442161761246e-02, -2.318354788291473e+02, 1.495357217394321e-01, -2.318352626532035e+02, 2.696241584767764e-01, -2.318347132261877e+02, 4.479101964820005e-01, -2.318333305837931e+02, 7.228742344584919e-01, -2.318298565981921e+02, 1.153856507515514e+00, -2.318211303931419e+02, 1.833841379822644e+00, -2.317992138806615e+02, 2.909431239455916e+00, -2.317441802215046e+02, 4.612105461508964e+00, -2.316060565931579e+02, 7.306654143322226e+00, -2.312598277395532e+02, 1.156384690656623e+01, -2.303946694274438e+02, 1.825970689572349e+01, -2.282496552817779e+02, 2.867173655595526e+01, -2.230330396146007e+02, 4.440780279305579e+01, -2.109200301821816e+02, 6.657634054390823e+01
            };

            NetlistReader nr = new NetlistReader();
            string netlist = string.Join(Environment.NewLine,
                ".MODEL MM NMOS LEVEL = 1 IS = 1e-32 VTO = 3.03646 LAMBDA = 0 KP = 5.28747 CGSO = 6.5761e-06 CGDO = 1e-11",
                "M1 out g 0 0 MM w = 100u l = 100u",
                "V1 in 0 DC 0 AC 1 0",
                "V2 vdd 0 DC 5.0",
                "R1 vdd out 10k",
                "R2 out g 10k",
                "Cin in g 1u",
                ".SAVE VR(out) VI(out)",
                ".AC dec 5 10 10g"
                );
            nr.Parse(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(netlist)));

            int index = 0;
            nr.Netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double actual = nr.Netlist.Exports[0].Extract(data);
                plotOutput.Points.AddXY(data.GetFrequency(), actual);
                plotReference.Points.AddXY(data.GetFrequency(), reference[index]);
                index += 2;
            };
            nr.Netlist.Simulate();
        }
    }
}