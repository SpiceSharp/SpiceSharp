using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Parameters
{
    /// <summary>
    /// An interface that can be used to set or get a parameter
    /// </summary>
    public interface IParameter
    {
        /// <summary>
        /// Get the value of the parameter
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        object Get();

        /// <summary>
        /// Set the value of the parameter
        /// </summary>
        /// <param name="value"></param>
        void Set(object value);
    }
}
