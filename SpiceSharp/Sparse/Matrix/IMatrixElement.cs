using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Sparse
{
    /// <summary>
    /// Element in a matrix
    /// </summary>
    public interface IMatrixElement<T>
    {
        /// <summary>
        /// Gets the row index
        /// </summary>
        int Row { get; }

        /// <summary>
        /// Gets the column index
        /// </summary>
        int Column { get; }
    }
}
