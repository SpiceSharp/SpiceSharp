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

            // Provided by Spice 3f5
            double[] reference = new double[]
            {
                1.334417655221980e-06, 3.471965089573855e-04, 3.351830745696013e-06, 5.502570946227631e-04, 8.418945895773091e-06, 8.720498043349111e-04, 2.114445669597715e-05, 1.381911086546095e-03, 5.309368526798767e-05, 2.189406695272188e-03, 1.332469066652801e-04, 3.466895147088519e-03, 3.339563613753495e-04, 5.482432501910041e-03, 8.341980040370838e-04, 8.640775529878610e-03, 2.066558898597437e-03, 1.350614394562201e-02, 5.017427448376647e-03, 2.069020111657962e-02, 1.162687214173938e-02, 3.025147596713823e-02, 2.444806066150139e-02, 4.013545957396424e-02, 4.357949902126838e-02, 4.514050832244691e-02, 6.329927287469508e-02, 4.136984589249247e-02, 7.720778968190564e-02, 3.183821694967992e-02, 8.460892410281323e-02, 2.201455496720316e-02, 8.796592758796802e-02, 1.444187547976473e-02, 8.937770311337524e-02, 9.259273400620100e-03, 8.995244044657573e-02, 5.881087349156822e-03, 9.018332766108907e-02, 3.722325696488075e-03, 9.027562051537530e-02, 2.354340724190597e-03, 9.031252808298670e-02, 1.491338054090599e-03, 9.032751174287086e-02, 9.494226860852419e-04, 9.033418279536445e-02, 6.121839678644537e-04, 9.033858322545095e-02, 4.068213923146900e-04, 9.034456220674272e-02, 2.884413123461115e-04, 9.035667499179965e-02, 2.293512095670835e-04, 9.038149464070862e-02, 2.102372862964324e-04, 9.042443030168182e-02, 2.125125991591695e-04, 9.048191631184550e-02, 2.196186992195977e-04, 9.054643271040638e-02, 2.275933681030558e-04, 9.062185343008870e-02, 2.359674744810244e-04, 9.071079692976204e-02, 2.293406880105504e-04, 9.079365040405128e-02, 1.944976240778601e-04, 9.084916360239494e-02, 1.441205567769841e-04, 9.087793546139711e-02, 9.797275248013266e-05, 9.089082649598125e-02, 6.381079386729474e-05, 9.089621923422750e-02, 4.078876004594978e-05, 9.089840984364975e-02, 2.587116901301576e-05, 9.089928903096886e-02, 1.635803306437017e-05, 9.089964017582838e-02, 1.033019503723023e-05, 9.089978014956064e-02, 6.520640901851122e-06, 9.089983590285976e-02, 4.115679165634583e-06, 9.089985810348902e-02, 2.598362267948757e-06, 9.089986694319312e-02, 1.641720674824462e-06, 9.089987046435227e-02, 1.039399501946900e-06
            };

            NetlistReader nr = new NetlistReader();
            nr.Netlist.Circuit.Nodes.Map("in");
            nr.Netlist.Circuit.Nodes.Map("vdd");
            nr.Netlist.Circuit.Nodes.Map("out");
            string netlist = string.Join(Environment.NewLine,
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
                "V1 in 0 DC 0 AC 1 0",
                "Vsupply vdd 0 5",
                "R1 vdd out 1k",
                "R2 out b 10k",
                "Cin in b 1u",
                "Q1 c b 0 0 mjd44h11",
                ".SAVE vr(out) vi(out)",
                ".AC dec 5 10 10g"
                );
            nr.Parse(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(netlist)));

            int index = 1;
            nr.Netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                if (index < 20)
                {
                    double actual = nr.Netlist.Exports[1].Extract(data);
                    double ref_i = reference[index];
                    plotOutput.Points.AddXY(data.GetFrequency(), actual);
                    plotReference.Points.AddXY(data.GetFrequency(), ref_i);
                }
                index += 2;
            };
            nr.Netlist.Simulate();
        }
    }
}