using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A class representing a resistor model
    /// </summary>
    public class ResistorModel : CircuitModel
    {
        /// <summary>
        /// Parameter table
        /// </summary>
        private static Dictionary<string, ParameterInfo> _pTable = new Dictionary<string, ParameterInfo>
        {
            { "rsh", new ParameterInfo(ParameterAccess.IOP, typeof(double), "Sheet resistance", "RESsheetRes") },
            { "narrow", new ParameterInfo(ParameterAccess.IOP, typeof(double), "Narrowing of resistor", "RESnarrow") },
            { "tc1", new ParameterInfo(ParameterAccess.IOP, typeof(double), "First order temp. coefficient", "REStempCoeff1") },
            { "tc2", new ParameterInfo(ParameterAccess.IOP, typeof(double), "Second order temp. coefficient", "REStempCoeff2") },
            { "defw", new ParameterInfo(ParameterAccess.IOPX, typeof(double), "Default device width", "RESdefWidth") },
            { "tnom", new ParameterInfo(ParameterAccess.IOPXU, typeof(double), "Parameter measurement temperature (in Kelvin)", "REStnom") },
            { "r", new ParameterInfo(ParameterAccess.IP, typeof(bool), "Device is a resistor model", "RESflag") }
        };

        /// <summary>
        /// Parameters
        /// </summary>
        public Parameter<double> REStnom { get; } = new Parameter<double>();
        public Parameter<double> REStempCoeff1 { get; } = new Parameter<double>();
        public Parameter<double> REStempCoeff2 { get; } = new Parameter<double>();
        public Parameter<double> RESsheetRes { get; } = new Parameter<double>();
        public Parameter<double> RESdefWidth { get; } = new Parameter<double>(10.0e-6);
        public Parameter<double> RESnarrow { get; } = new Parameter<double>();
        public bool RESflag { get; } = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public ResistorModel(string name) : base(name)
        {
        }

        public override Dictionary<string, ParameterInfo> Parameters => throw new NotImplementedException();
    }
}
