using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using SpiceSharp.NewSparse;
using System.Diagnostics;

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

            // Build the matrix
            Matrix<double> matrix = new Matrix<double>();

            // We want to test all possible combinations for the row! We need 5 elements to be able to test it
            for (int i = 0; i < 32; i++)
            {
                // Our counter "i" will represent in binary which elements need to be filled.
                int fill = i;
                for (int k = 0; k < 5; k++)
                {
                    // Get whether or not the element needs to be filled
                    if ((fill & 0x01) != 0)
                    {
                        int expected = k * 32 + i + 1;
                        matrix.GetElement(i + 1, k + 1).Value = expected;
                    }
                    fill = (fill >> 1) & 0b011111;
                }
            }

            Console.WriteLine(matrix);

            // Swap the two rows of interest
            matrix.SwapColumns(2, 4);

            Console.WriteLine(matrix);
        }
    }
}