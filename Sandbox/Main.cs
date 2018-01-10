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

            Circuit ckt = new Circuit();
            var src = new Voltagesource("V1", "A", "0", 0);
            src.VSRCacMag.Set(12);
            ckt.Objects.Add(
              src,
              new Resistor("R1", "A", "B", 10),
              new Inductor("L1", "B", "0", 0.000018));

            ckt.Check();
        }
    }
}
