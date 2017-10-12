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
                1.101938772677249e-12, 1.020351798553683e-12, 9.930914904110209e-13, 1.018704151068570e-12, 1.098549779218339e-12, 1.236872518232169e-12, 1.441022728380007e-12, 1.721840550874621e-12, 2.094217487153999e-12, 2.577858124390719e-12, 3.198267097692130e-12, 3.987983099859464e-12, 4.988064706261346e-12, 6.249786808519018e-12, 7.836401914012315e-12, 9.824601582961330e-12, 1.230487975385098e-11, 1.537918793701761e-11, 1.915286183784488e-11, 2.371564318075358e-11, 2.910415518231322e-11, 3.523777729653164e-11, 4.182805832714524e-11, 4.828952148482491e-11, 5.372934740827330e-11, 5.712303094004328e-11, 5.769531786489053e-11, 5.530480561891652e-11, 5.052178742786626e-11, 4.432835400406183e-11, 3.770017350647562e-11, 3.134531801206027e-11, 2.565584102156048e-11, 2.077916019829584e-11, 1.671370313489508e-11, 1.338397388087558e-11, 1.068731671781018e-11, 8.518878155014311e-12, 6.783039968978641e-12, 5.397456921217182e-12, 4.293446467982328e-12, 3.414789199740856e-12, 2.715987838819629e-12, 2.160479000532345e-12, 1.719006299308645e-12, 1.368223534346394e-12, 1.089532434335449e-12, 8.681329107747076e-13, 6.922553133235565e-13, 5.525438940163232e-13, 4.415637162479891e-13, 3.534072935825517e-13, 2.833812867202454e-13, 2.277572142155273e-13, 1.835732224046120e-13, 1.484765190559835e-13, 1.205981604962914e-13, 9.845356466124910e-14, 8.086347233940640e-14, 6.689115769628357e-14, 5.579254942299496e-14, 4.697660884412611e-14, 3.997385626141668e-14, 3.441137009036628e-14, 2.999292772256991e-14, 2.648323069715085e-14, 2.369537418245358e-14, 2.148089349130362e-14, 1.972185755582100e-14, 1.832458826459864e-14, 1.721467110986965e-14, 1.633299130424805e-14, 1.563258405234841e-14, 1.507613088730997e-14, 1.463396817940087e-14, 1.428250087013048e-14, 1.400293554605381e-14, 1.378026315480551e-14, 1.360243367639641e-14, 1.345967328032927e-14, 1.334389898454301e-14, 1.324818629450979e-14, 1.316624109090014e-14, 1.309181726819219e-14, 1.301800572736213e-14, 1.293629969876902e-14, 1.283532383362070e-14, 1.269912466960263e-14, 1.250502043061476e-14, 1.222133156026873e-14, 1.180607868779705e-14
            };

            NetlistReader nr = new NetlistReader();
            string netlist = string.Join(Environment.NewLine,
                ".MODEL MM NMOS LEVEL = 1 IS = 1e-32 VTO = 3.03646 LAMBDA = 0 KP = 5.28747 CGSO = 6.5761e-06 CGDO = 1e-11 KF=1e-25",
                "M1 out g 0 0 MM w = 100u l = 100u",
                "V1 in 0 DC 0 AC 1 0",
                "V2 vdd 0 DC 5.0",
                "R1 vdd out 10k",
                "R2 out g 10k",
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
