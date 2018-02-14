using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SpiceSharpTest.Sparse
{
    [TestClass]
    public class NewSparseSolveTest : SolveFramework
    {
        [TestMethod]
        public void TestFidapm05_mtx()
        {
            var solver = ReadMtxFile("Sparse/Matrices/fidapm05.mtx");

            // Order and factor this matrix
            solver.OrderAndFactor();
        }
    }
}
