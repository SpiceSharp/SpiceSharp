using System.Text.RegularExpressions;

namespace Spice2SpiceSharp
{
    public class SpiceTruncate : SpiceIterator
    {
        public string states;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dev">Device</param>
        public SpiceTruncate(SpiceDevice dev, SpiceSetup setup)
        {
            string content = dev.GetMethod(SpiceDevice.Methods.Trunc);
            ReadMethod(content);

            // Copy the matrix nodes
            states = setup.StatesVariable;
        }

        /// <summary>
        /// Get the model code
        /// </summary>
        /// <param name="mparams">The model parameters</param>
        /// <returns></returns>
        public override string ExportModel(SpiceParam mparams)
        {
            string code = GetModelCode(mparams);

            return Code.Format(code);
        }

        /// <summary>
        /// Get the device code
        /// </summary>
        /// <param name="mparams">The model parameters</param>
        /// <param name="dparams">The device parameters</param>
        /// <returns></returns>
        public override string ExportDevice(SpiceParam mparams, SpiceParam dparams)
        {
            string code = GetDeviceCode(mparams, dparams);

            // Replace CKTterr by the method
            code = Regex.Replace(code, @"CKTterr\(\s*(?<state>\w+)\s*,\s*ckt\s*,\s*(?<var>\w+)\s*\)\s*;", (Match m) => $"method.Terr({states} + {m.Groups["state"].Value}, ckt, ref {m.Groups["var"].Value});");

            return Code.Format(code);
        }
    }
}
