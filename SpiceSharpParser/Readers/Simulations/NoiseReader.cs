using SpiceSharp.Simulations;
using static SpiceSharp.Parser.Readers.ReaderExtension;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads a <see cref="Noise"/> analysis
    /// </summary>
    public class NoiseReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NoiseReader() : base(StatementType.Control)
        {
            Identifier = "noise";
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="st">Statement</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(string type, Statement st, Netlist netlist)
        {
            Noise noise = new Noise("Noise " + (netlist.Simulations.Count + 1));

            // Check parameter count
            switch (st.Parameters.Count)
            {
                case 0: throw new ParseException("Output expected for .NOISE");
                case 1: throw new ParseException(st.Parameters[0], "Source expected", false);
                case 2: throw new ParseException(st.Parameters[1], "Step type expected", false);
                case 3: throw new ParseException(st.Parameters[2], "Number of points expected", false);
                case 4: throw new ParseException(st.Parameters[3], "Starting frequency expected", false);
                case 5: throw new ParseException(st.Parameters[4], "Stopping frequency expected", false);
                case 6: break;
                case 7: break;
                default:
                    throw new ParseException(st.Parameters[7], "Too many parameter");
            }

            // The first parameters needs to specify the output voltage
            if (st.Parameters[0].kind == BRACKET)
            {
                var bracket = st.Parameters[0] as BracketToken;
                if (bracket.Name.image.ToLower() == "v")
                {
                    Token t;
                    switch (bracket.Parameters.Length)
                    {
                        // V(A, B)
                        case 2:
                            t = bracket.Parameters[0];
                            noise.Output = IsNode(t) ? t.image.ToLower() : throw new ParseException(t, "Node expected");
                            t = bracket.Parameters[1];
                            noise.OutputRef = IsNode(t) ? t.image.ToLower() : throw new ParseException(t, "Node expected");
                            break;

                        // V(A)
                        case 1:
                            t = bracket.Parameters[0];
                            noise.Output = IsNode(t) ? t.image.ToLower() : throw new ParseException(t, "Node expected");
                            break;

                        default:
                            throw new ParseException(bracket.Name, "1 or 2 nodes expected", false);
                    }
                }
                else
                    throw new ParseException(bracket.Name, "Invalid output");
            }
            else
                throw new ParseException(st.Parameters[0], "Invalid output");

            // The second parameter needs to be source
            if (!IsName(st.Parameters[1]))
                throw new ParseException(st.Parameters[1], "Invalid source");
            noise.Input = st.Parameters[1].image.ToLower();

            // Sweep parameters
            switch (st.Parameters[2].image.ToLower())
            {
                case "lin": noise.StepType = Noise.StepTypes.Linear; break;
                case "dec": noise.StepType = Noise.StepTypes.Decade; break;
                case "oct": noise.StepType = Noise.StepTypes.Octave; break;
                default:
                    throw new ParseException(st.Parameters[2], "Invalid step type");
            }
            noise.NumberSteps = (int)(netlist.ParseDouble(st.Parameters[3]) + 0.25);
            noise.StartFreq = netlist.ParseDouble(st.Parameters[4]);
            noise.StopFreq = netlist.ParseDouble(st.Parameters[5]);

            Generated = noise;
            netlist.Simulations.Add(noise);
            return true;
        }
    }
}
