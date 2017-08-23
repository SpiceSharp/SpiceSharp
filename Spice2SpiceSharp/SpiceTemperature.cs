using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spice2SpiceSharp
{
    /// <summary>
    /// This class represents the temperature dependent calculations
    /// </summary>
    public class SpiceTemperature : SpiceIterator
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dev">The device</param>
        public SpiceTemperature(SpiceDevice dev)
        {
            // Get the 
            string content = dev.GetMethod(SpiceDevice.Methods.Temperature);
            ReadMethod(content);
        }

        /// <summary>
        /// Export model code
        /// </summary>
        /// <param name="mparams">The model parameters</param>
        /// <returns></returns>
        public override string ExportModel(SpiceParam mparams)
        {
            string code = GetModelCode(mparams);

            // Find temperature stuff
            code = Regex.Replace(code, @"ckt\s*\-\>\s*CKTtemp", "ckt.State.Temperature");
            code = Regex.Replace(code, @"ckt\s*\-\>\s*CKTnomTemp", "ckt.State.NominalTemperature");

            // -> Is never possible, so let's go for dots
            code = Regex.Replace(code, @"\s*\-\>\s*", ".");

            return Code.Format(code);
        }

        /// <summary>
        /// Export device code
        /// </summary>
        /// <param name="mparams">The model parameters</param>
        /// <param name="dparams">The device parameters</param>
        /// <returns></returns>
        public override string ExportDevice(SpiceParam mparams, SpiceParam dparams)
        {
            string code = GetDeviceCode(mparams, dparams);

            // Find temperature stuff
            code = Regex.Replace(code, @"ckt\s*\-\>\s*CKTtemp", "ckt.State.Temperature");
            code = Regex.Replace(code, @"ckt\s*\-\>\s*CKTnomTemp", "ckt.State.NominalTemperature");

            // -> Is never possible, so let's go for dots
            code = Regex.Replace(code, @"\s*\-\>\s*", ".");

            return Code.Format(code);
        }
    }
}
