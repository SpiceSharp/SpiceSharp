using System;
using System.Numerics;
using System.Collections.Generic;
using System.Windows.Forms;

using SpiceSharp.Sparse;

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

            // Test a simple matrix
            Matrix matrix = new Matrix(3, false);
            matrix.spGetElement(3, 2).Real = 32.0;
            matrix.spGetElement(2, 3).Real = 23.0;
            matrix.spGetElement(1, 1).Real = 11.0;

            matrix.spcLinkRows();

            Console.WriteLine(matrix.spPrint(false, true, true));
        }
    }
}