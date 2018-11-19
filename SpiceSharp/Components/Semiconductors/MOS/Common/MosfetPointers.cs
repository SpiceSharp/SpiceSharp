using System;
using System.Collections.Generic;
using System.Text;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    public class MosfetPointers
    {
        public MatrixElement<double> DrainDrainPtr { get; private set; }
        public MatrixElement<double> GateGatePtr { get; private set; }
        public MatrixElement<double> SourceSourcePtr { get; private set; }
        public MatrixElement<double> BulkBulkPtr { get; private set; }
        public MatrixElement<double> DrainPrimeDrainPrimePtr { get; private set; }
        public MatrixElement<double> SourcePrimeSourcePrimePtr { get; private set; }
        public MatrixElement<double> DrainDrainPrimePtr { get; private set; }
        public MatrixElement<double> GateBulkPtr { get; private set; }
        public MatrixElement<double> GateDrainPrimePtr { get; private set; }
        public MatrixElement<double> GateSourcePrimePtr { get; private set; }
        public MatrixElement<double> SourceSourcePrimePtr { get; private set; }
        public MatrixElement<double> BulkDrainPrimePtr { get; private set; }
        public MatrixElement<double> BulkSourcePrimePtr { get; private set; }
        public MatrixElement<double> DrainPrimeSourcePrimePtr { get; private set; }
        public MatrixElement<double> DrainPrimeDrainPtr { get; private set; }
        public MatrixElement<double> BulkGatePtr { get; private set; }
        public MatrixElement<double> DrainPrimeGatePtr { get; private set; }
        public MatrixElement<double> SourcePrimeGatePtr { get; private set; }
        public MatrixElement<double> SourcePrimeSourcePtr { get; private set; }
        public MatrixElement<double> DrainPrimeBulkPtr { get; private set; }
        public MatrixElement<double> SourcePrimeBulkPtr { get; private set; }
        public MatrixElement<double> SourcePrimeDrainPrimePtr { get; private set; }
        public VectorElement<double> BulkPtr { get; private set; }
        public VectorElement<double> DrainPrimePtr { get; private set; }
        public VectorElement<double> SourcePrimePtr { get; private set; }

        /// <summary>
        /// Gets the pointers for a mosfet.
        /// </summary>
        /// <param name="solver">The solver.</param>
        /// <param name="drain">The drain.</param>
        /// <param name="gate">The gate.</param>
        /// <param name="source">The source.</param>
        /// <param name="bulk">The bulk.</param>
        /// <param name="drainPrime">The drain prime.</param>
        /// <param name="sourcePrime">The source prime.</param>
        public void GetPointers(Solver<double> solver, int drain, int gate, int source, int bulk, int drainPrime, int sourcePrime)
        {
            // Get matrix pointers
            DrainDrainPtr = solver.GetMatrixElement(drain, drain);
            GateGatePtr = solver.GetMatrixElement(gate, gate);
            SourceSourcePtr = solver.GetMatrixElement(source, source);
            BulkBulkPtr = solver.GetMatrixElement(bulk, bulk);
            DrainPrimeDrainPrimePtr = solver.GetMatrixElement(drainPrime, drainPrime);
            SourcePrimeSourcePrimePtr = solver.GetMatrixElement(sourcePrime, sourcePrime);
            DrainDrainPrimePtr = solver.GetMatrixElement(drain, drainPrime);
            GateBulkPtr = solver.GetMatrixElement(gate, bulk);
            GateDrainPrimePtr = solver.GetMatrixElement(gate, drainPrime);
            GateSourcePrimePtr = solver.GetMatrixElement(gate, sourcePrime);
            SourceSourcePrimePtr = solver.GetMatrixElement(source, sourcePrime);
            BulkDrainPrimePtr = solver.GetMatrixElement(bulk, drainPrime);
            BulkSourcePrimePtr = solver.GetMatrixElement(bulk, sourcePrime);
            DrainPrimeSourcePrimePtr = solver.GetMatrixElement(drainPrime, sourcePrime);
            DrainPrimeDrainPtr = solver.GetMatrixElement(drainPrime, drain);
            BulkGatePtr = solver.GetMatrixElement(bulk, gate);
            DrainPrimeGatePtr = solver.GetMatrixElement(drainPrime, gate);
            SourcePrimeGatePtr = solver.GetMatrixElement(sourcePrime, gate);
            SourcePrimeSourcePtr = solver.GetMatrixElement(sourcePrime, source);
            DrainPrimeBulkPtr = solver.GetMatrixElement(drainPrime, bulk);
            SourcePrimeBulkPtr = solver.GetMatrixElement(sourcePrime, bulk);
            SourcePrimeDrainPrimePtr = solver.GetMatrixElement(sourcePrime, drainPrime);

            // Get rhs pointers
            BulkPtr = solver.GetRhsElement(bulk);
            DrainPrimePtr = solver.GetRhsElement(drainPrime);
            SourcePrimePtr = solver.GetRhsElement(sourcePrime);
        }
    }
}
