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
                1.539536799052918e-08, 1.425345435411005e-08, 1.387057594440403e-08, 1.422632996835039e-08, 1.533963544458775e-08, 1.726972771446756e-08, 2.011928701284424e-08, 2.403985484211667e-08, 2.923980093621382e-08, 3.599521393602984e-08, 4.466419381666479e-08, 5.570510421760067e-08, 6.969935295791849e-08, 8.737911233612782e-08, 1.096598686737091e-07, 1.376764079306097e-07, 1.728180388884790e-07, 2.167531130268350e-07, 2.714217539395848e-07, 3.389553269773518e-07, 4.214470619168126e-07, 5.204498170488096e-07, 6.360328975134899e-07, 7.652633336600888e-07, 9.002602032091956e-07, 1.026717692562145e-06, 1.124827913760457e-06, 1.174519607691990e-06, 1.163957750016862e-06, 1.095752686376782e-06, 9.855214927538404e-07, 8.541143022481549e-07, 7.198049873512028e-07, 5.946327641875588e-07, 4.845636355140244e-07, 3.912952737214274e-07, 3.141085332390589e-07, 2.511836994017152e-07, 2.003727806630066e-07, 1.595909797180171e-07, 1.269836933484485e-07, 1.009753304509658e-07, 8.026208279931426e-08, 6.378180050949626e-08, 5.067741158629848e-08, 4.026138502566187e-08, 3.198421061708310e-08, 2.540769734184064e-08, 2.018292494198937e-08, 1.603230850568468e-08, 1.273514004532339e-08, 1.011599746207860e-08, 8.035484136167294e-09, 6.382846382763908e-09, 5.070095882912550e-09, 4.027334244157314e-09, 3.199035798462045e-09, 2.541093235211037e-09, 2.018470017819231e-09, 1.603335207809911e-09, 1.273581690332554e-09, 1.011649052308116e-09, 8.035885079041564e-10, 6.383201156746202e-10, 5.070427517480938e-10, 4.027654281492569e-10, 3.199350023391544e-10, 2.541404547028331e-10, 2.018779869617789e-10, 1.603644327863574e-10, 1.273890443643801e-10, 1.011957621812022e-10, 8.038969852857269e-11, 6.386285468853768e-11, 5.073511598184553e-11, 4.030738246218394e-11, 3.202433929990102e-11, 2.544488424493813e-11, 2.021863732481874e-11, 1.606728183409455e-11, 1.276974295521784e-11, 1.015041471851632e-11, 8.069808344039258e-12, 6.417123955417489e-12, 5.104350082433485e-12, 4.061576729307079e-12, 3.233272412497215e-12, 2.575326906709407e-12, 2.052702214551339e-12, 1.637566665405663e-12, 1.307812777481265e-12
            };

            NetlistReader nr = new NetlistReader();
            string netlist = string.Join(Environment.NewLine,
                "M1 out g vdd vdd DMOS L = 1u W = 1u",
                ".MODEL DMOS pmos(LEVEL = 3 VTO = -0.7 KP = 3.8E+1 THETA = .25 VMAX = 3.5E5 KF=1e-24)",
                "Vsupply vdd 0 1.8",
                "V1 in 0 DC 0 AC 1 0",
                "R1 out 0 100k",
                "R2 g out 10k",
                "Cin in g 1u",
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
