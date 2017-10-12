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

            Series ns = chMain.Series.Add("Output noise density");
            ns.ChartType = SeriesChartType.FastLine;
            Series rs = chMain.Series.Add("Reference");
            rs.ChartType = SeriesChartType.FastPoint;

            chMain.ChartAreas[0].AxisX.IsLogarithmic = true;

            double[] reference = new double[]
            {
                1.179219862989481e-11, 9.366799509824981e-12, 7.440215819393452e-12, 5.909849953969606e-12, 4.694204355116928e-12, 3.728541470609732e-12, 2.961436248069141e-12, 2.352037572092826e-12, 1.867892810716822e-12, 1.483219628924207e-12, 1.177533053932103e-12, 9.345547120412678e-13, 7.413462160103590e-13, 5.876206817161517e-13, 4.651959586434446e-13, 3.675609264002581e-13, 2.895326170890468e-13, 2.269873731311376e-13, 1.766539773722950e-13, 1.359604399125598e-13, 1.029274183594503e-13, 7.609663023305161e-14, 5.446842860703937e-14, 3.740481097004802e-14, 2.446189680290874e-14, 1.518227588269855e-14, 8.959195761725242e-15, 5.059913505431299e-15, 2.760736675274261e-15, 1.470044930464820e-15, 7.716487852712253e-16, 4.034529515752228e-16, 2.127468258361768e-16, 1.151235562432188e-16, 6.553027189419247e-17, 4.046020619340668e-17, 2.782674437697462e-17, 2.147313449302714e-17, 1.828183346826315e-17, 1.668018849406525e-17, 1.587676549007375e-17, 1.547387872517497e-17, 1.527188688617226e-17, 1.517062892416029e-17, 1.511987266797110e-17, 1.509443203139195e-17, 1.508168078745598e-17, 1.507528979199142e-17, 1.507208662826275e-17, 1.507048121609172e-17, 1.506967659369265e-17, 1.506927332291652e-17, 1.506907120673515e-17, 1.506896990763351e-17, 1.506891913722307e-17, 1.506889369138883e-17, 1.506888093804600e-17, 1.506887454609958e-17, 1.506887134245389e-17, 1.506886973677496e-17, 1.506886893199605e-17, 1.506886852863024e-17, 1.506886832645527e-17, 1.506886822511944e-17, 1.506886817432599e-17, 1.506886814886564e-17, 1.506886813610314e-17, 1.506886812970543e-17, 1.506886812649814e-17, 1.506886812489016e-17, 1.506886812408394e-17, 1.506886812367966e-17, 1.506886812347690e-17, 1.506886812337520e-17, 1.506886812332418e-17, 1.506886812329857e-17, 1.506886812328572e-17, 1.506886812327926e-17, 1.506886812327602e-17, 1.506886812327438e-17, 1.506886812327356e-17, 1.506886812327315e-17, 1.506886812327294e-17, 1.506886812327284e-17, 1.506886812327278e-17, 1.506886812327275e-17, 1.506886812327274e-17, 1.506886812327273e-17, 1.506886812327273e-17, 1.506886812327273e-17, 1.506886812327273e-17
            };

            NetlistReader nr = new NetlistReader();
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
                "+ TR = 2.73328e-08 PTF = 1 KF = 1e-8 AF = 1",
                "V1 in 0 DC 0 AC 1 0",
                "Vsupply vdd 0 5",
                "R1 vdd out 1k",
                "R2 out b 10k",
                "Cin in b 1u",
                "Q1 c b 0 0 mjd44h11",
                ".NOISE v(out) V1 dec 10 10 10g");
            nr.Parse(new MemoryStream(Encoding.UTF8.GetBytes(netlist)));

            int index = 0;
            nr.Netlist.OnExportSimulationData += (object sender, SimulationData data) =>
            {
                double n = 10.0 * Math.Log10(data.GetOutputNoiseDensity());
                ns.Points.AddXY(data.GetFrequency(), n);

                n = 10.0 * Math.Log10(reference[index++]);
                rs.Points.AddXY(data.GetFrequency(), n);
            };
            nr.Netlist.Simulate();
        }
    }
}
