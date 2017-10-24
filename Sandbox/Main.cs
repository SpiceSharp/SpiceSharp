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

            // Provided by Spice 3f5
            double[] reference = new double[]
            {
                1.334417655221980e-06, 3.471965089573855e-04, 3.351830745696013e-06, 5.502570946227631e-04, 8.418945895773091e-06, 8.720498043349111e-04, 2.114445669597715e-05, 1.381911086546095e-03, 5.309368526798767e-05, 2.189406695272188e-03, 1.332469066652801e-04, 3.466895147088519e-03, 3.339563613753495e-04, 5.482432501910041e-03, 8.341980040370838e-04, 8.640775529878610e-03, 2.066558898597437e-03, 1.350614394562201e-02, 5.017427448376647e-03, 2.069020111657962e-02, 1.162687214173938e-02, 3.025147596713823e-02, 2.444806066150139e-02, 4.013545957396424e-02, 4.357949902126838e-02, 4.514050832244691e-02, 6.329927287469508e-02, 4.136984589249247e-02, 7.720778968190564e-02, 3.183821694967992e-02, 8.460892410281323e-02, 2.201455496720316e-02, 8.796592758796802e-02, 1.444187547976473e-02, 8.937770311337524e-02, 9.259273400620100e-03, 8.995244044657573e-02, 5.881087349156822e-03, 9.018332766108907e-02, 3.722325696488075e-03, 9.027562051537530e-02, 2.354340724190597e-03, 9.031252808298670e-02, 1.491338054090599e-03, 9.032751174287086e-02, 9.494226860852419e-04, 9.033418279536445e-02, 6.121839678644537e-04, 9.033858322545095e-02, 4.068213923146900e-04, 9.034456220674272e-02, 2.884413123461115e-04, 9.035667499179965e-02, 2.293512095670835e-04, 9.038149464070862e-02, 2.102372862964324e-04, 9.042443030168182e-02, 2.125125991591695e-04, 9.048191631184550e-02, 2.196186992195977e-04, 9.054643271040638e-02, 2.275933681030558e-04, 9.062185343008870e-02, 2.359674744810244e-04, 9.071079692976204e-02, 2.293406880105504e-04, 9.079365040405128e-02, 1.944976240778601e-04, 9.084916360239494e-02, 1.441205567769841e-04, 9.087793546139711e-02, 9.797275248013266e-05, 9.089082649598125e-02, 6.381079386729474e-05, 9.089621923422750e-02, 4.078876004594978e-05, 9.089840984364975e-02, 2.587116901301576e-05, 9.089928903096886e-02, 1.635803306437017e-05, 9.089964017582838e-02, 1.033019503723023e-05, 9.089978014956064e-02, 6.520640901851122e-06, 9.089983590285976e-02, 4.115679165634583e-06, 9.089985810348902e-02, 2.598362267948757e-06, 9.089986694319312e-02, 1.641720674824462e-06, 9.089987046435227e-02, 1.039399501946900e-06
            };

            NetlistReader nr = new NetlistReader();
            string netlist = string.Join(Environment.NewLine,
                "M1 out g vdd vdd DMOS L = 1u W = 1u",
                ".MODEL DMOS pmos(LEVEL = 3 VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5)",
                "Vsupply vdd 0 1.8",
                "Vin in 0 DC 0 AC 1 0",
                "R1 out 0 100k",
                "R2 g out 10k",
                "Cin in g 1u",
                ".SAVE VR(out) VI(out)",
                ".AC dec 5 10 10g"
                );
            nr.Parse(new MemoryStream(Encoding.UTF8.GetBytes(netlist)));

            int index = 0;
            nr.Netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double n = data.GetDb(new SpiceSharp.Circuits.CircuitIdentifier("out"));
                ns.Points.AddXY(data.GetFrequency(), n);

                n = 10.0 * Math.Log10(Math.Abs(reference[index] * reference[index] + reference[index + 1] * reference[index + 1]));
                rs.Points.AddXY(data.GetFrequency(), n);
                index += 2;
            };
            nr.Netlist.Simulate();
        }
    }
}
