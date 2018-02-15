using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using SpiceSharp.NewSparse;
using SpiceSharp.NewSparse.Solve;

namespace SpiceSharpTest.Sparse
{
    public class SolveFramework
    {
        /// <summary>
        /// Read a .MTX file
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns></returns>
        protected Solver<double> ReadMtxFile(string filename)
        {
            Solver<double> result = null;

            using (StreamReader sr = new StreamReader(filename))
            {
                // The first line is a comment
                sr.ReadLine();

                // The second line tells us the dimensions
                string line = sr.ReadLine();
                var match = Regex.Match(line, @"^(?<rows>\d+)\s+(?<columns>\d+)\s+(\d+)");
                int size = int.Parse(match.Groups["rows"].Value);
                if (int.Parse(match.Groups["columns"].Value) != size)
                    throw new Exception("Matrix is not square");

                result = new RealSolver(size);

                // All subsequent lines are of the format [row] [column] [value]
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    match = Regex.Match(line, @"^(?<row>\d+)\s+(?<column>\d+)\s+(?<value>.*)\s*$");
                    if (!match.Success)
                        throw new Exception("Could not recognize file");
                    int row = int.Parse(match.Groups["row"].Value);
                    int column = int.Parse(match.Groups["column"].Value);
                    double value = double.Parse(match.Groups["value"].Value, System.Globalization.CultureInfo.InvariantCulture);

                    // Set the value in the matrix
                    result.GetMatrixElement(row, column).Value = value;
                }
            }

            return result;
        }
    }
}
