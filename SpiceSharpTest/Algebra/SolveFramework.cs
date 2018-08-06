using System;
using System.Text.RegularExpressions;
using System.IO;
using SpiceSharp.Algebra;

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
            Solver<double> result;

            using (var sr = new StreamReader(filename))
            {
                // The first line is a comment
                sr.ReadLine();

                // The second line tells us the dimensions
                var line = sr.ReadLine() ?? throw new Exception("Invalid Mtx file");
                var match = Regex.Match(line, @"^(?<rows>\d+)\s+(?<columns>\d+)\s+(\d+)");
                var size = int.Parse(match.Groups["rows"].Value);
                if (int.Parse(match.Groups["columns"].Value) != size)
                    throw new Exception("Matrix is not square");

                result = new RealSolver(size);

                // All subsequent lines are of the format [row] [column] [value]
                while (!sr.EndOfStream)
                {
                    // Read the next line
                    line = sr.ReadLine();
                    if (line == null)
                        break;

                    match = Regex.Match(line, @"^(?<row>\d+)\s+(?<column>\d+)\s+(?<value>.*)\s*$");
                    if (!match.Success)
                        throw new Exception("Could not recognize file");
                    var row = int.Parse(match.Groups["row"].Value);
                    var column = int.Parse(match.Groups["column"].Value);
                    var value = double.Parse(match.Groups["value"].Value, System.Globalization.CultureInfo.InvariantCulture);

                    // Set the value in the matrix
                    result.GetMatrixElement(row, column).Value = value;
                }
            }

            return result;
        }
    }
}
